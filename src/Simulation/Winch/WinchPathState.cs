using System.Numerics;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Gameplay rope-path abstraction.
///
/// Stage 5 has only a world anchor and therefore no intermediate contacts yet. Callers use
/// CurrentPullPoint instead of depending directly on a forever-single-segment representation.
/// Stage 6 may extend this type with obstruction contacts without changing the winch force API.
/// </summary>
public readonly record struct WinchPathState(
    Vector3 WorldAnchor)
{
    public Vector3 CurrentPullPoint => WorldAnchor;

    public int ContactCount => 0;

    public static WinchPathState AtWorldAnchor(Vector3 worldAnchor) =>
        new(worldAnchor);
}
