using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Current Stage 5 direct-pull prototype.
///
/// LMB selects/replaces a world point. Holding RMB pulls directly toward that point with immediate
/// velocity. Reaching the point clears the target, stops the pull, zeroes velocity, and lets
/// ordinary gravity take over.
/// </summary>
public static class WinchSystem
{
    private const float TinyDistanceSquared = 1e-10f;

    public static WinchStepResult Step(
        in PlayerState player,
        in WinchState previousWinch,
        in PlayerInput input,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world,
        float fixedDeltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(locomotionConfig);
        ArgumentNullException.ThrowIfNull(world);

        _ = fixedDeltaSeconds;

        var winch = previousWinch;
        var updatedPlayer = player;

        if (input.Has(PlayerButtons.SelectGrapplePointPressed))
        {
            winch = TrySelectTarget(
                player,
                winch,
                config,
                locomotionConfig,
                world);
        }

        if (input.Has(PlayerButtons.PullReleased))
        {
            winch = winch with
            {
                IsPulling = false,
                LastPullSpeed = 0f,
            };
        }
        else if (input.Has(PlayerButtons.PullPressed) && winch.HasTarget)
        {
            winch = winch with { IsPulling = true };
        }

        if (!winch.HasTarget)
        {
            return new WinchStepResult(
                updatedPlayer,
                WinchState.Initial);
        }

        var toTarget = winch.Path.CurrentPullPoint - player.Position;
        var distanceSquared = toTarget.LengthSquared();
        var distance = distanceSquared <= TinyDistanceSquared
            ? 0f
            : MathF.Sqrt(distanceSquared);

        if (distance <= config.ArrivalDistance)
        {
            // Arrival is intentionally simple for this iteration:
            // stop at the point, consume the target, and begin falling under normal locomotion.
            updatedPlayer = player with { Velocity = Vector3.Zero };

            return new WinchStepResult(
                updatedPlayer,
                WinchState.Initial);
        }

        if (!winch.IsPulling)
        {
            return new WinchStepResult(
                updatedPlayer,
                winch with
                {
                    LastActualDistance = distance,
                    LastPullSpeed = 0f,
                });
        }

        var direction = toTarget / distance;
        updatedPlayer = player with
        {
            Velocity = direction * config.PullSpeed,
            IsGrounded = false,
        };

        return new WinchStepResult(
            updatedPlayer,
            winch with
            {
                LastActualDistance = distance,
                LastPullSpeed = config.PullSpeed,
            });
    }

    public static RayQuery BuildAimRay(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        var eye = player.Position
            + (Vector3.UnitY * locomotionConfig.EyeOffsetFromCapsuleCenter);
        var direction = ViewForward(
            player.ViewYawRadians,
            player.ViewPitchRadians);

        return new RayQuery(
            eye,
            eye + (direction * config.GrappleRange),
            config.GrappleCollisionMask);
    }

    private static WinchState TrySelectTarget(
        in PlayerState player,
        in WinchState current,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world)
    {
        var query = BuildAimRay(player, config, locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            // A miss does not destroy an existing useful target.
            return current;
        }

        return new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(hit.Position),
            current.IsPulling,
            Vector3.Distance(player.Position, hit.Position),
            current.IsPulling ? config.PullSpeed : 0f);
    }

    private static Vector3 ViewForward(
        float yawRadians,
        float pitchRadians)
    {
        var cosPitch = MathF.Cos(pitchRadians);

        return Vector3.Normalize(new Vector3(
            -MathF.Sin(yawRadians) * cosPitch,
            MathF.Sin(pitchRadians),
            -MathF.Cos(yawRadians) * cosPitch));
    }
}
