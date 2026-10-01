using System.Numerics;

namespace Hitch.Simulation.State;

/// <summary>
/// Minimal gameplay-relevant player state needed before locomotion is implemented.
/// This is value data so snapshots/replay do not depend on a Godot node.
/// </summary>
public readonly record struct PlayerState(
    Vector3 Position,
    Vector3 Velocity,
    Quaternion Orientation,
    bool IsGrounded)
{
    public static PlayerState Initial => new(
        Vector3.Zero,
        Vector3.Zero,
        Quaternion.Identity,
        false);
}
