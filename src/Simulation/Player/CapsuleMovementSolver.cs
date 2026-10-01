using System.Numerics;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Player;

/// <summary>
/// Custom upright-capsule sweep/slide movement.
///
/// This solver owns no engine state. World collision observations are supplied through IWorldQuery.
/// </summary>
public static class CapsuleMovementSolver
{
    private const float MinimumMotionSquared = 1e-10f;
    private const float MinimumUsefulFraction = 1e-5f;

    public static PlayerState Move(
        in PlayerState player,
        PlayerLocomotionConfig config,
        IWorldQuery world,
        float fixedDeltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(world);

        if (!float.IsFinite(fixedDeltaSeconds) || fixedDeltaSeconds <= 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fixedDeltaSeconds),
                fixedDeltaSeconds,
                "Fixed delta must be finite and positive.");
        }

        var position = player.Position;
        var velocity = player.Velocity;
        var remaining = velocity * fixedDeltaSeconds;

        for (var iteration = 0;
             iteration < config.MaxSlideIterations
             && remaining.LengthSquared() > MinimumMotionSquared;
             iteration++)
        {
            var query = new CapsuleSweepQuery(
                position,
                remaining,
                config.CapsuleRadius,
                config.CapsuleHalfSegmentLength,
                config.CollisionMargin,
                config.WorldCollisionMask);

            if (!world.TrySweepCapsule(query, out var hit))
            {
                position += remaining;
                remaining = Vector3.Zero;
                break;
            }

            var safeFraction = Math.Clamp(hit.TravelFraction, 0f, 1f);
            var traveled = remaining * safeFraction;
            position += traveled;

            var normal = hit.Normal;
            if (normal.LengthSquared() <= MinimumMotionSquared)
            {
                break;
            }

            normal = Vector3.Normalize(normal);

            var velocityIntoSurface = Vector3.Dot(velocity, normal);
            if (velocityIntoSurface < 0f)
            {
                velocity -= normal * velocityIntoSurface;
            }

            var untraveled = remaining * (1f - safeFraction);
            var motionIntoSurface = Vector3.Dot(untraveled, normal);
            if (motionIntoSurface < 0f)
            {
                untraveled -= normal * motionIntoSurface;
            }

            remaining = untraveled;

            // A zero-distance hit with no meaningful slide direction can otherwise repeat forever.
            if (safeFraction <= MinimumUsefulFraction
                && remaining.LengthSquared() <= MinimumMotionSquared)
            {
                break;
            }
        }

        return player with
        {
            Position = position,
            Velocity = velocity,
        };
    }
}
