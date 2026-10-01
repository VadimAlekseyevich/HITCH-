namespace Hitch.Simulation.Winch;

/// <summary>
/// Snapshot-friendly state for the current selected-point automatic-pull prototype.
/// </summary>
public readonly record struct WinchState(
    WinchTargetState TargetState,
    WinchPathState Path,
    bool IsPulling,
    float LastActualDistance,
    float LastPullAcceleration)
{
    public bool HasTarget => TargetState == WinchTargetState.Selected;

    public static WinchState Initial => new(
        WinchTargetState.None,
        default,
        false,
        0f,
        0f);
}
