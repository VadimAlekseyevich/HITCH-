using System.Numerics;

namespace Hitch.Simulation.State;

/// <summary>
/// Minimal gameplay-relevant player state.
/// Position is the center of the upright gameplay capsule.
///
/// BodyOrientation is intentionally separate from view yaw/pitch. Future physical impact rotation
/// may affect the body without automatically deciding how the first-person camera should react.
/// </summary>
public readonly record struct PlayerState(
    Vector3 Position,
    Vector3 Velocity,
    Quaternion BodyOrientation,
    float ViewYawRadians,
    float ViewPitchRadians,
    bool IsGrounded)
{
    public static PlayerState Initial => new(
        Vector3.Zero,
        Vector3.Zero,
        Quaternion.Identity,
        0f,
        0f,
        false);
}
