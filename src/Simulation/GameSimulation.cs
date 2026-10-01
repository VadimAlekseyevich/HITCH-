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

    public SimulationConfig Config { get; }

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

        // View orientation is gameplay state because aiming/grapple direction will depend on it.
        // Input adapters provide LookDelta in radians; raw mouse pixels never enter simulation.
        var player = State.Player;
        var yaw = WrapRadians(player.ViewYawRadians + input.LookDelta.X);
        var pitch = Math.Clamp(
            player.ViewPitchRadians + input.LookDelta.Y,
            -Config.ViewPitchLimitRadians,
            Config.ViewPitchLimitRadians);

        var viewUpdatedPlayer = player with
        {
            ViewYawRadians = yaw,
            ViewPitchRadians = pitch,
        };

        var detachConsumed =
            State.Winch.HasTarget
            && input.Has(PlayerButtons.GrappleDetachPressed);

        var winchResult = WinchSystem.Step(
            viewUpdatedPlayer,
            State.Winch,
            input,
            Config.Winch,
            Config.Locomotion,
            world,
            (float)FixedDeltaSeconds);

        var finalWinch = winchResult.Winch;
        PlayerState movedPlayer;

        if (winchResult.CompletedThisTick || finalWinch.IsLatched)
        {
            // Reaching the selected surface is a real stop, not a one-frame zero.
            // Keep the cable latched and suppress gravity/air control until RMB retargets
            // or a miss releases the cable.
            movedPlayer = winchResult.Player with
            {
                Velocity = System.Numerics.Vector3.Zero,
                IsGrounded = false,
            };
        }
        else
        {
            if (finalWinch.HasTarget)
            {
                // Attached rope physics already contains gravity and rope tension whether or not
                // reel-in has started. Move/collide directly so locomotion does not apply gravity twice.
                movedPlayer = CapsuleMovementSolver.Move(
                    winchResult.Player,
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
                    winchResult.Player,
                    locomotionInput,
                    Config.Locomotion,
                    world,
                    (float)FixedDeltaSeconds);
            }

            // High-speed movement can reach/collide with the anchor surface during movement.
            if (WinchSystem.HasReachedAnchor(
                    movedPlayer,
                    finalWinch,
                    Config.Winch,
                    Config.Locomotion))
            {
                finalWinch = finalWinch with
                {
                    IsPulling = false,
                    IsArrivedLatched = true,
                    LastActualDistance = WinchSystem.ComputeRopePathLength(
                        movedPlayer.Position,
                        finalWinch.Path),
                    LastPullAcceleration = 0f,
                };
                movedPlayer = movedPlayer with
                {
                    Velocity = System.Numerics.Vector3.Zero,
                    IsGrounded = false,
                };
            }
        }

        if (finalWinch.HasTarget)
        {
            finalWinch = finalWinch with
            {
                LastActualDistance = WinchSystem.ComputeRopePathLength(
                    movedPlayer.Position,
                    finalWinch.Path),
            };
        }

        State = State with
        {
            Tick = State.Tick.Next(),
            Player = movedPlayer,
            Winch = finalWinch,
        };

        Telemetry.Observe(State);

        return State;
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
