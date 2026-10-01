using System.Numerics;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Player;

public static class GroundDetector
{
    public static bool IsGrounded(
        in PlayerState player,
        PlayerLocomotionConfig config,
        IWorldQuery world)
    {
        if (player.Velocity.Y > 0f)
        {
            return false;
        }

        var query = new CapsuleSweepQuery(
            player.Position,
            -Vector3.UnitY * config.GroundProbeDistance,
            config.CapsuleRadius,
            config.CapsuleHalfSegmentLength,
            0f,
            config.WorldCollisionMask);

        if (!world.TrySweepCapsule(query, out var hit))
        {
            return false;
        }

        var normal = hit.Normal;
        if (normal.LengthSquared() <= 1e-10f)
        {
            return false;
        }

        normal = Vector3.Normalize(normal);
        return normal.Y >= config.MinGroundNormalY;
    }
}
