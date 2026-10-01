using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 single-cable action prototype.
///
/// RMB raycasts/replaces the cable and starts pull immediately.
/// Gameplay rope length is unlimited inside the playable space.
/// </summary>
public static class WinchSystem
{
    private const float TinyDistanceSquared = 1e-10f;

    // Finite endpoint required by the world-query API only.
    // This is deliberately far beyond the playable room and is NOT a gameplay range cap.
    private const float EngineSafeRaycastDistance = 10_000f;

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
        if (input.Has(PlayerButtons.GrapplePullPressed))
        {
            winch = TryShootActiveCable(
                player,
                config,
                locomotionConfig,
                world);

            // RMB miss is an explicit release. Do not carry old grapple momentum forward.
            if (!winch.HasTarget)
            {
                updatedPlayer = updatedPlayer with
                {
                    Velocity = Vector3.Zero,
                    IsGrounded = false,
                };
            }
        }

        if (!winch.IsPulling)
        {
            return new WinchStepResult(
                updatedPlayer,
                winch.HasTarget ? winch : WinchState.Initial,
                false);
        }

        var toTarget =
            winch.Path.CurrentPullPoint - player.Position;
        var distanceSquared = toTarget.LengthSquared();
        var distance = distanceSquared <= TinyDistanceSquared
            ? 0f
            : MathF.Sqrt(distanceSquared);

        var direction = distance > 0f
            ? toTarget / distance
            : Vector3.Zero;

        if (HasReachedAnchor(
                updatedPlayer,
                winch,
                config,
                locomotionConfig))
        {
            // Complete reel-in: no residual tangential/orbital velocity.
            // The anchor remains latched so the player can actually arrive and stay stopped.
            updatedPlayer = updatedPlayer with
            {
                Velocity = Vector3.Zero,
                IsGrounded = false,
            };

            return new WinchStepResult(
                updatedPlayer,
                winch with
                {
                    IsPulling = false,
                    LastActualDistance = distance,
                    LastPullAcceleration = 0f,
                },
                true);
        }

        // Iteration 11 intentionally removes inherited momentum from grapple travel.
        // The cable owns movement while pulling: every tick points velocity directly at the anchor.
        // No tangential/orbital component from an earlier trajectory survives.
        var velocity = distance > 0f
            ? direction * config.PullTargetInwardSpeed
            : Vector3.Zero;

        updatedPlayer = updatedPlayer with
        {
            Velocity = velocity,
            IsGrounded = false,
        };

        var appliedRadialAcceleration =
            distance > 0f
                ? config.PullTargetInwardSpeed / fixedDeltaSeconds
                : 0f;

        return new WinchStepResult(
            updatedPlayer,
            winch with
            {
                LastActualDistance = distance,
                LastPullAcceleration = appliedRadialAcceleration,
            },
            false);
    }

    public static RayQuery BuildAimRay(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        var eye = player.Position
            + (Vector3.UnitY
               * locomotionConfig.EyeOffsetFromCapsuleCenter);
        var direction = ViewForward(
            player.ViewYawRadians,
            player.ViewPitchRadians);

        return new RayQuery(
            eye,
            eye + (direction * EngineSafeRaycastDistance),
            config.GrappleCollisionMask);
    }

    public static bool HasReachedAnchor(
        in PlayerState player,
        in WinchState winch,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        if (!winch.IsPulling)
        {
            return false;
        }

        var anchor = winch.Path.CurrentPullPoint;
        var fromAnchorToPlayer = player.Position - anchor;

        if (winch.Path.HasAnchorSurfaceNormal)
        {
            var normal = winch.Path.WorldAnchorNormal;
            var signedSurfaceDistance =
                Vector3.Dot(fromAnchorToPlayer, normal);

            // The selected raycast surface should remain on the outward side of the capsule.
            // A small negative allowance covers numerical skin/margin noise without treating a
            // completely different side of geometry as the same arrival.
            if (signedSurfaceDistance < -config.ArrivalContactTolerance)
            {
                return false;
            }

            var surfaceArrivalDistance =
                ComputeCapsuleAwareArrivalDistance(
                    normal,
                    config,
                    locomotionConfig);

            if (signedSurfaceDistance > surfaceArrivalDistance)
            {
                return false;
            }

            var tangentialOffset =
                fromAnchorToPlayer - (normal * signedSurfaceDistance);

            return tangentialOffset.LengthSquared()
                <= config.ArrivalSurfaceCaptureRadius
                   * config.ArrivalSurfaceCaptureRadius;
        }

        // Fallback for legacy/tests that do not carry an anchor surface normal.
        var distanceSquared = fromAnchorToPlayer.LengthSquared();
        var distance = distanceSquared <= TinyDistanceSquared
            ? 0f
            : MathF.Sqrt(distanceSquared);

        var direction = distance > 0f
            ? -fromAnchorToPlayer / distance
            : Vector3.Zero;

        return distance <= ComputeCapsuleAwareArrivalDistance(
            direction,
            config,
            locomotionConfig);
    }

    public static float ComputeCapsuleAwareArrivalDistance(
        Vector3 cableDirection,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        if (cableDirection.LengthSquared() <= TinyDistanceSquared)
        {
            return config.ArrivalContactTolerance;
        }

        // Support distance of a vertical capsule along an arbitrary direction:
        // sphere radius + projected half-segment length.
        var capsuleSupport =
            locomotionConfig.CapsuleRadius
            + (locomotionConfig.CapsuleHalfSegmentLength
               * MathF.Abs(cableDirection.Y))
            + locomotionConfig.CollisionMargin;

        return capsuleSupport
            + config.ArrivalContactTolerance;
    }

    private static WinchState TryShootActiveCable(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world)
    {
        var query = BuildAimRay(
            player,
            config,
            locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            return WinchState.Initial;
        }

        return new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(
                hit.Position,
                hit.Normal),
            true,
            Vector3.Distance(
                player.Position,
                hit.Position),
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
