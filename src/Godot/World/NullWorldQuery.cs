using Hitch.Simulation.World;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Temporary Stage 2 adapter used until real Godot/Jolt queries are introduced.
/// </summary>
internal sealed class NullWorldQuery : IWorldQuery
{
    public bool TryRaycast(in RayQuery query, out WorldHit hit)
    {
        hit = default;
        return false;
    }

    public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
    {
        hit = default;
        return false;
    }
}
