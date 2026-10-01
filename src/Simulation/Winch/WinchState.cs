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

    /// <summary>
    /// Current deployed rope length along the full piecewise path.
    /// RMB initializes it; LMB reel-in reduces it.
    /// </summary>
    public float RopeLength { get; init; }

    /// <summary>
    /// True only after actual arrival at the selected grapple surface.
    /// Attached-but-not-reeling is no longer treated as a frozen latch.
    /// </summary>
    public bool IsArrivedLatched { get; init; }

    public bool HasTarget =>
        TargetState == WinchTargetState.Selected;

    public bool IsLatched =>
        HasTarget && IsArrivedLatched;

    public bool IsAttachedFree =>
        HasTarget && !IsPulling && !IsArrivedLatched;

    public static WinchState Initial => new(
        WinchTargetState.None,
        default,
        false,
        0f,
        0f);
}
