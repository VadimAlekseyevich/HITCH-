using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 iteration 4.
///
/// Every RMB click raycasts a fresh world point, replaces any previous cable, and immediately
/// starts automatic pull in the same simulation tick. There is no separate cable-placement action.
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

        var winch = previousWinch;
        var updatedPlayer = player;
        var startedThisTick = false;

        if (input.Has(PlayerButtons.GrapplePullPressed))
        {
            // A new RMB click always replaces the previous cable. If the ray misses, the old
            // cable is gone and the player simply continues with existing momentum.
            winch = TryShootActiveCable(
                player,
                config,
                locomotionConfig,
                world);

            startedThisTick = winch.IsPulling;
        }

        if (!winch.HasTarget || !winch.IsPulling)
        {
            return new WinchStepResult(
                updatedPlayer,
                winch.HasTarget ? winch : WinchState.Initial);
        }

        var toTarget = winch.Path.CurrentPullPoint - player.Position;
        var distanceSquared = toTarget.LengthSquared();
        var distance = distanceSquared <= TinyDistanceSquared
            ? 0f
            : MathF.Sqrt(distanceSquared);

        var direction = distance > 0f
            ? toTarget / distance
            : Vector3.Zero;

        if (startedThisTick && distance > 0f)
        {
            updatedPlayer = updatedPlayer with
            {
                Velocity = updatedPlayer.Velocity
                    + (direction * config.PullInitialImpulse),
                IsGrounded = false,
            };
        }

        if (distance <= config.ArrivalDistance)
        {
            // Full reel-in completes at the anchor.
            // The previous iterations preserved tangential velocity here, which caused the
            // player to orbit/slide around the target after "arriving". The current human
            // requirement is explicit: complete pull -> complete stop -> ordinary gravity.
            updatedPlayer = updatedPlayer with
            {
                Velocity = Vector3.Zero,
                IsGrounded = false,
            };

            return new WinchStepResult(
                updatedPlayer,
                WinchState.Initial);
        }

        var appliedRadialAcceleration = 0f;

        if (distance > 0f)
        {
            var inwardSpeed = Vector3.Dot(
                updatedPlayer.Velocity,
                direction);

            if (inwardSpeed < config.PullTargetInwardSpeed)
            {
                var neededSpeed =
                    config.PullTargetInwardSpeed - inwardSpeed;
                var addedSpeed = MathF.Min(
                    neededSpeed,
                    config.PullRadialAcceleration * fixedDeltaSeconds);

                updatedPlayer = updatedPlayer with
                {
                    Velocity = updatedPlayer.Velocity
                        + (direction * addedSpeed),
                    IsGrounded = false,
                };

                appliedRadialAcceleration =
                    addedSpeed / fixedDeltaSeconds;
            }
        }

        return new WinchStepResult(
            updatedPlayer,
            winch with
            {
                LastActualDistance = distance,
                LastPullAcceleration = appliedRadialAcceleration,
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

    private static WinchState TryShootActiveCable(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world)
    {
        var query = BuildAimRay(player, config, locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            return WinchState.Initial;
        }

        return new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(hit.Position),
            true,
            Vector3.Distance(player.Position, hit.Position),
            0f);
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
