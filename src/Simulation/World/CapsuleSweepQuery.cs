using System.Numerics;

namespace Hitch.Simulation.World;

/// <summary>
/// Sweep of an upright capsule.
///
/// HalfSegmentLength is the distance from the capsule center to either hemisphere center.
/// Total capsule height is 2 * (Radius + HalfSegmentLength).
/// </summary>
public readonly record struct CapsuleSweepQuery(
    Vector3 StartCenter,
    Vector3 Displacement,
    float Radius,
    float HalfSegmentLength,
    uint CollisionMask)
{
    public void Validate()
    {
        if (!float.IsFinite(Radius) || Radius <= 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Radius),
                Radius,
                "Capsule radius must be finite and greater than zero.");
        }

        if (!float.IsFinite(HalfSegmentLength) || HalfSegmentLength < 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(HalfSegmentLength),
                HalfSegmentLength,
                "Capsule half-segment length must be finite and non-negative.");
        }
    }
}
