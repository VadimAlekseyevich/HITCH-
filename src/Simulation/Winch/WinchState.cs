namespace Hitch.Simulation.Winch;

/// <summary>
/// Snapshot-friendly state for the single active grapple cable.
/// </summary>
public readonly record struct WinchState(
    WinchTargetState TargetState,
    WinchPathState Path,
    bool IsPulling,
    float LastActualDistance,
    float LastPullAcceleration)
{
    /// <summary>
    /// Time since the current pull started. Retargeting creates a fresh winch state and resets it.
    /// Used only for the launch-speed envelope; it does not represent physical inertia.
    /// </summary>
    public float PullElapsedSeconds { get; init; }

    public bool HasTarget =>
        TargetState == WinchTargetState.Selected;

    public bool IsLatched =>
        HasTarget && !IsPulling;

    public static WinchState Initial => new(
        WinchTargetState.None,
        default,
        false,
        0f,
        0f);
}
