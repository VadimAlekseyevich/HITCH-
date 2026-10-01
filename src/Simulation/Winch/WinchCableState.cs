namespace Hitch.Simulation.Winch;

/// <summary>
/// Snapshot-friendly state for one grapple cable.
/// </summary>
public readonly record struct WinchCableState(
    WinchTargetState TargetState,
    WinchPathState Path,
    bool IsPulling,
    float LastActualDistance,
    float LastPullAcceleration)
{
    public bool HasTarget => TargetState == WinchTargetState.Selected;

    public static WinchCableState Initial => new(
        WinchTargetState.None,
        default,
        false,
        0f,
        0f);
}
