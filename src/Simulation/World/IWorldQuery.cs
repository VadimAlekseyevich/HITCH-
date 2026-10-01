namespace Hitch.Simulation.World;

/// <summary>
/// Narrow boundary between gameplay simulation and engine/world collision queries.
/// Godot/Jolt implementations belong outside the simulation assembly.
/// </summary>
public interface IWorldQuery
{
    bool TryRaycast(in RayQuery query, out WorldHit hit);

    bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit);
}
