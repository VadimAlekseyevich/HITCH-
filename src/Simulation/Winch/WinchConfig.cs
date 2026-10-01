namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 single-cable automatic pull tuning.
/// Gameplay rope length is unlimited inside the playable space.
/// </summary>
public sealed record WinchConfig
{
    public uint GrappleCollisionMask { get; init; } = 1u;

    public float PullInitialImpulse { get; init; } = 24f;

    public float PullRadialAcceleration { get; init; } = 300f;

    /// <summary>
    /// Direct grapple speed for short/medium pulls.
    /// Iteration 15 scales absolute speed down with the compact city while preserving strong relative motion.
    /// </summary>
    public float PullTargetInwardSpeed { get; init; } = 22.5f;

    /// <summary>
    /// Maximum direct grapple speed reached on long lines.
    /// </summary>
    public float PullLongRangeInwardSpeed { get; init; } = 40f;

    /// <summary>
    /// Anchor distance at which the long-range grapple speed reaches its maximum.
    /// </summary>
    public float PullLongRangeDistance { get; init; } = 62.5f;

    /// <summary>
    /// Immediate speed multiplier on the exact tick a fresh grapple starts.
    /// </summary>
    public float PullLaunchInitialMultiplier { get; init; } = 2.0f;

    /// <summary>
    /// Stronger multiplier reached shortly after launch, creating a distinct second-stage blast.
    /// </summary>
    public float PullLaunchPeakMultiplier { get; init; } = 4.0f;

    /// <summary>
    /// Time from grapple fire to the second-stage launch peak.
    /// </summary>
    public float PullLaunchPeakSeconds { get; init; } = 0.12f;

    /// <summary>
    /// Time after the peak for the launch bonus to decay naturally back to sustained pull speed.
    /// </summary>
    public float PullLaunchDecaySeconds { get; init; } = 0.65f;

    /// <summary>
    /// Extra distance beyond the capsule's geometric support radius used to recognize that
    /// the body has effectively reached the grapple surface.
    ///
    /// The actual completion distance depends on cable direction and capsule shape.
    /// </summary>
    public float ArrivalContactTolerance { get; init; } = 0.06f;

    /// <summary>
    /// Maximum tangential offset from the selected anchor point at which touching the selected
    /// surface counts as a completed pull. This prevents endless orbiting caused by collision
    /// preserving tangential velocity while still requiring the player to reach the anchor area.
    /// </summary>
    public float ArrivalSurfaceCaptureRadius { get; init; } = 1.75f;

    public void Validate()
    {
        RequireFinitePositive(
            PullInitialImpulse,
            nameof(PullInitialImpulse));
        RequireFinitePositive(
            PullRadialAcceleration,
            nameof(PullRadialAcceleration));
        RequireFinitePositive(
            PullTargetInwardSpeed,
            nameof(PullTargetInwardSpeed));
        RequireFinitePositive(
            PullLongRangeInwardSpeed,
            nameof(PullLongRangeInwardSpeed));
        RequireFinitePositive(
            PullLongRangeDistance,
            nameof(PullLongRangeDistance));
        RequireFinitePositive(
            PullLaunchInitialMultiplier,
            nameof(PullLaunchInitialMultiplier));
        RequireFinitePositive(
            PullLaunchPeakMultiplier,
            nameof(PullLaunchPeakMultiplier));
        RequireFinitePositive(
            PullLaunchPeakSeconds,
            nameof(PullLaunchPeakSeconds));
        RequireFinitePositive(
            PullLaunchDecaySeconds,
            nameof(PullLaunchDecaySeconds));

        if (PullLaunchInitialMultiplier < 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PullLaunchInitialMultiplier),
                PullLaunchInitialMultiplier,
                "Initial launch multiplier must be at least 1.");
        }

        if (PullLaunchPeakMultiplier < PullLaunchInitialMultiplier)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PullLaunchPeakMultiplier),
                PullLaunchPeakMultiplier,
                "Launch peak multiplier must be at least the initial launch multiplier.");
        }

        if (PullLongRangeInwardSpeed < PullTargetInwardSpeed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PullLongRangeInwardSpeed),
                PullLongRangeInwardSpeed,
                "Long-range pull speed must be at least the base pull speed.");
        }
        RequireFiniteNonNegative(
            ArrivalContactTolerance,
            nameof(ArrivalContactTolerance));
        RequireFinitePositive(
            ArrivalSurfaceCaptureRadius,
            nameof(ArrivalSurfaceCaptureRadius));
    }

    private static void RequireFinitePositive(
        float value,
        string name)
    {
        if (!float.IsFinite(value) || value <= 0f)
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                "Value must be finite and positive.");
        }
    }

    private static void RequireFiniteNonNegative(
        float value,
        string name)
    {
        if (!float.IsFinite(value) || value < 0f)
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                "Value must be finite and non-negative.");
        }
    }
}
