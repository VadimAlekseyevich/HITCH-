using System.Numerics;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Gameplay rope-path abstraction.
///
/// Stage 5 stores the world anchor plus the surface normal captured by the grapple raycast.
/// The normal is part of arrival semantics: reaching the anchor surface must not require the
/// capsule center to converge on one exact mathematical point while collision allows sliding.
/// </summary>
public readonly record struct WinchPathState(
    Vector3 WorldAnchor,
    Vector3 WorldAnchorNormal)
{
    public Vector3 CurrentPullPoint => WorldAnchor;

    public int ContactCount => 0;

    public bool HasAnchorSurfaceNormal =>
        WorldAnchorNormal.LengthSquared() > 1e-10f;

    public static WinchPathState AtWorldAnchor(Vector3 worldAnchor) =>
        new(worldAnchor, Vector3.Zero);

    public static WinchPathState AtWorldAnchor(
        Vector3 worldAnchor,
        Vector3 worldAnchorNormal)
    {
        var normal = worldAnchorNormal.LengthSquared() > 1e-10f
            ? Vector3.Normalize(worldAnchorNormal)
            : Vector3.Zero;

        return new WinchPathState(worldAnchor, normal);
    }
}
