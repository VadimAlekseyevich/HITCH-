using Hitch.Simulation.Debug;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;
using Hitch.Simulation.Winch;

namespace Hitch.Simulation;

/// <summary>
/// Owns authoritative gameplay state for one simulation instance.
///
/// One call to Step advances exactly one fixed simulation tick.
/// Movement behavior remains explicit and engine-independent; Godot only supplies world queries.
/// </summary>
public sealed class GameSimulation
{
    public GameSimulation(
        SimulationConfig config,
        SimulationState initialState)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();

        Config = config;
        State = initialState;
        Telemetry = new SimulationTelemetry();
    }

    public SimulationConfig Config { get; private set; }

    /// <summary>
    /// Applies validated feel tuning at runtime without resetting player/cable state.
    /// Intended for the Stage 5 in-game tuning panel.
    /// </summary>
    public void ApplyConfig(SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();
        Config = config;
    }

    public SimulationState State { get; private set; }

    public SimulationTelemetry Telemetry { get; }

    public double FixedDeltaSeconds => Config.FixedDeltaSeconds;

    /// <summary>
    /// Adds gameplay velocity without replacing momentum already present.
    ///
    /// The future winch, player collisions, and impulse weapons should use this path instead of
    /// mutating Godot transforms or presentation nodes.
    /// </summary>
    public void ApplyPlayerVelocityImpulse(System.Numerics.Vector3 deltaVelocity)
    {
        if (!float.IsFinite(deltaVelocity.X)
            || !float.IsFinite(deltaVelocity.Y)
            || !float.IsFinite(deltaVelocity.Z))
        {
            throw new ArgumentOutOfRangeException(
                nameof(deltaVelocity),
                deltaVelocity,
                "Velocity impulse components must be finite.");
        }

        State = State with
        {
            Player = State.Player with
            {
                Velocity = State.Player.Velocity + deltaVelocity,
            },
        };
    }

    public SimulationState Step(
        in PlayerInput input,
        IWorldQuery world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var player = State.Player;
        var yaw = WrapRadians(
            player.ViewYawRadians + input.LookDelta.X);
        var pitch = Math.Clamp(
            player.ViewPitchRadians + input.LookDelta.Y,
            -Config.ViewPitchLimitRadians,
            Config.ViewPitchLimitRadians);

        var viewUpdatedPlayer = player with
        {
            ViewYawRadians = yaw,
            ViewPitchRadians = pitch,
        };

        var leftBefore = State.Winch;
        var rightBefore = State.SecondaryWinch;
        var shootPressed =
            input.Has(PlayerButtons.GrappleShootPressed);
        var shootLeft =
            shootPressed && State.NextGrappleSlot == 0;
        var shootRight =
            shootPressed && State.NextGrappleSlot != 0;

        var detachConsumed =
            (leftBefore.HasTarget || rightBefore.HasTarget)
            && input.Has(PlayerButtons.GrappleDetachPressed);

        // If the other side is already attached, a fresh alternating RMB shot is treated as
        // entering dual-cable mode for motor scaling. A miss can therefore soften one motor
        // for only this single tick, which is preferable to coupling acquisition to hidden state.
        var dualCableMotorExpected =
            (leftBefore.HasTarget && rightBefore.HasTarget)
            || (shootLeft && rightBefore.HasTarget)
            || (shootRight && leftBefore.HasTarget);
        var motorScale =
            dualCableMotorExpected
                ? Config.Winch.DualCableMotorScale
                : 1f;

        var leftInput = BuildCableInput(
            input,
            includeShoot: shootLeft);
        var rightInput = BuildCableInput(
            input,
            includeShoot: shootRight);

        // Gravity must enter rope physics exactly once even with two active cables.
        var leftAppliesGravity =
            leftBefore.HasTarget
            || (!rightBefore.HasTarget && shootLeft);
        var rightAppliesGravity =
            !leftAppliesGravity
            && (rightBefore.HasTarget || shootRight);

        var leftResult = WinchSystem.Step(
            viewUpdatedPlayer,
            leftBefore,
            leftInput,
            Config.Winch,
            Config.Locomotion,
            world,
            (float)FixedDeltaSeconds,
            applyGravity: leftAppliesGravity,
            motorScale: motorScale);

        var rightResult = WinchSystem.Step(
            leftResult.Player,
            rightBefore,
            rightInput,
            Config.Winch,
            Config.Locomotion,
            world,
            (float)FixedDeltaSeconds,
            applyGravity: rightAppliesGravity,
            motorScale: motorScale);

        var leftWinch = leftResult.Winch;
        var rightWinch = rightResult.Winch;
        var ropePlayer = rightResult.Player;
        var anyCable =
            leftWinch.HasTarget || rightWinch.HasTarget;

        PlayerState movedPlayer;

        if (AreAllAttachedCablesLatched(
                leftWinch,
                rightWinch))
        {
            movedPlayer = ropePlayer with
            {
                Velocity = System.Numerics.Vector3.Zero,
                IsGrounded = false,
            };
        }
        else if (anyCable)
        {
            // Both cable solvers have already contributed gravity/tension/gas to one velocity.
            // Resolve world collision only once after those forces are combined.
            movedPlayer = CapsuleMovementSolver.Move(
                ropePlayer,
                Config.Locomotion,
                world,
                (float)FixedDeltaSeconds);
        }
        else
        {
            var locomotionInput = detachConsumed
                ? input with
                {
                    Buttons =
                        input.Buttons
                        & ~PlayerButtons.JumpPressed,
                }
                : input;

            movedPlayer = PlayerLocomotionSystem.Step(
                ropePlayer,
                locomotionInput,
                Config.Locomotion,
                world,
                (float)FixedDeltaSeconds);
        }

        leftWinch = CompleteCableAfterMovement(
            movedPlayer,
            leftWinch);
        rightWinch = CompleteCableAfterMovement(
            movedPlayer,
            rightWinch);

        if (AreAllAttachedCablesLatched(
                leftWinch,
                rightWinch))
        {
            movedPlayer = movedPlayer with
            {
                Velocity = System.Numerics.Vector3.Zero,
                IsGrounded = false,
            };
        }

        if (leftWinch.HasTarget)
        {
            leftWinch = leftWinch with
            {
                LastActualDistance =
                    WinchSystem.ComputeRopePathLength(
                        movedPlayer.Position,
                        leftWinch.Path),
            };
        }

        if (rightWinch.HasTarget)
        {
            rightWinch = rightWinch with
            {
                LastActualDistance =
                    WinchSystem.ComputeRopePathLength(
                        movedPlayer.Position,
                        rightWinch.Path),
            };
        }

        State = State with
        {
            Tick = State.Tick.Next(),
            Player = movedPlayer,
            Winch = leftWinch,
            SecondaryWinch = rightWinch,
            NextGrappleSlot =
                shootPressed
                    ? (byte)(State.NextGrappleSlot == 0 ? 1 : 0)
                    : State.NextGrappleSlot,
        };

        Telemetry.Observe(State);

        return State;
    }

    private WinchState CompleteCableAfterMovement(
        in PlayerState movedPlayer,
        in WinchState cable)
    {
        if (!WinchSystem.HasReachedAnchor(
                movedPlayer,
                cable,
                Config.Winch,
                Config.Locomotion))
        {
            return cable;
        }

        return cable with
        {
            IsPulling = false,
            IsArrivedLatched = true,
            LastActualDistance =
                WinchSystem.ComputeRopePathLength(
                    movedPlayer.Position,
                    cable.Path),
            LastPullAcceleration = 0f,
        };
    }

    private static bool AreAllAttachedCablesLatched(
        in WinchState left,
        in WinchState right)
    {
        var any =
            left.HasTarget || right.HasTarget;

        return any
            && (!left.HasTarget || left.IsLatched)
            && (!right.HasTarget || right.IsLatched);
    }

    private static PlayerInput BuildCableInput(
        in PlayerInput input,
        bool includeShoot)
    {
        var buttons =
            input.Buttons
            & ~PlayerButtons.GrappleShootPressed;

        if (includeShoot)
        {
            buttons |=
                PlayerButtons.GrappleShootPressed;
        }

        return input with
        {
            Buttons = buttons,
        };
    }

    private static float WrapRadians(float angle)
    {
        var wrapped = (angle + MathF.PI) % MathF.Tau;

        if (wrapped < 0f)
        {
            wrapped += MathF.Tau;
        }

        return wrapped - MathF.PI;
    }
}
