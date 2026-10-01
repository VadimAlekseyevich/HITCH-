namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 single-cable ODM-inspired rope, reel, and gas tuning.
/// </summary>
public sealed record WinchConfig
{
    public uint GrappleCollisionMask { get; init; } = 1u;

    /// <summary>
    /// Maximum deployed rope length and grapple acquisition range.
    /// Large enough for the compact city, but deliberately not map-wide.
    /// </summary>
    public float MaxRopeLength { get; init; } = 75f;

    public float PullInitialImpulse { get; init; } = 8f;

    public float PullRadialAcceleration { get; init; } = 40f;

    /// <summary>
    /// Direct grapple speed for short/medium pulls.
    /// Iteration 15 scales absolute speed down with the compact city while preserving strong relative motion.
    /// </summary>
    public float PullTargetInwardSpeed { get; init; } = 9f;

    /// <summary>
    /// Maximum direct grapple speed reached on long lines.
    /// </summary>
    public float PullLongRangeInwardSpeed { get; init; } = 16f;

    /// <summary>
    /// Anchor distance at which the long-range grapple speed reaches its maximum.
    /// </summary>
    public float PullLongRangeDistance { get; init; } = 55f;

    /// <summary>
    /// Immediate speed multiplier on the exact tick a fresh grapple starts.
    /// </summary>
    public float PullLaunchInitialMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Stronger multiplier reached shortly after launch, creating a distinct second-stage blast.
    /// </summary>
    public float PullLaunchPeakMultiplier { get; init; } = 1.0f;

    /// <summary>
    /// Time from grapple fire to the second-stage launch peak.
    /// </summary>
    public float PullLaunchPeakSeconds { get; init; } = 0.10f;

    /// <summary>
    /// Time after the peak for the launch bonus to decay naturally back to sustained pull speed.
    /// </summary>
    public float PullLaunchDecaySeconds { get; init; } = 0.40f;

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

    /// <summary>
    /// Small outward offset applied to a rope bend so the next visibility ray starts outside
    /// the obstacle surface rather than immediately re-hitting the same face.
    /// </summary>
    public float RopeContactSurfaceOffset { get; init; } = 0.08f;

    /// <summary>
    /// A ray hit this close to the requested segment endpoint is treated as reaching that
    /// endpoint rather than discovering a new obstruction.
    /// </summary>
    public float RopeEndpointTolerance { get; init; } = 0.22f;

    /// <summary>
    /// Prevents near-identical contacts from being appended on consecutive ticks.
    /// </summary>
    public float RopeMinimumContactSpacing { get; init; } = 0.35f;

    /// <summary>
    /// Maximum distance to slide a newly discovered contact along its hit surface while
    /// searching for the nearest edge/corner that clears both neighboring rope segments.
    /// </summary>
    public float RopeContactEdgeSearchDistance { get; init; } = 16f;

    /// <summary>
    /// Maximum extra inward correction speed used only when geometry temporarily makes the
    /// polyline longer than the deployed rope. Keeps wrap changes from becoming catapults.
    /// </summary>
    public float RopeConstraintCorrectionSpeed { get; init; } = 10f;

    /// <summary>
    /// Small slack band before the rope is considered taut.
    /// </summary>
    public float RopeTautTolerance { get; init; } = 0.08f;

    /// <summary>
    /// Compressed-gas style acceleration applied while reeling.
    /// It acts along the cable tangent, so reel motor and gas have distinct jobs.
    /// </summary>
    public float GasAcceleration { get; init; } = 14f;

    /// <summary>
    /// Gas retains full authority below this total player speed.
    /// </summary>
    public float GasFullAccelerationSpeed { get; init; } = 18f;

    /// <summary>
    /// Gas authority smoothly fades to zero by this speed. This is not a hard speed cap:
    /// gravity and swing can still carry the player faster.
    /// </summary>
    public float GasCutoffSpeed { get; init; } = 34f;

    public void Validate()
    {
        RequireFinitePositive(
            MaxRopeLength,
            nameof(MaxRopeLength));
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
        RequireFinitePositive(
            RopeContactSurfaceOffset,
            nameof(RopeContactSurfaceOffset));
        RequireFinitePositive(
            RopeEndpointTolerance,
            nameof(RopeEndpointTolerance));
        RequireFinitePositive(
            RopeMinimumContactSpacing,
            nameof(RopeMinimumContactSpacing));
        RequireFinitePositive(
            RopeContactEdgeSearchDistance,
            nameof(RopeContactEdgeSearchDistance));
        RequireFinitePositive(
            RopeConstraintCorrectionSpeed,
            nameof(RopeConstraintCorrectionSpeed));
        RequireFiniteNonNegative(
            RopeTautTolerance,
            nameof(RopeTautTolerance));
        RequireFiniteNonNegative(
            GasAcceleration,
            nameof(GasAcceleration));
        RequireFiniteNonNegative(
            GasFullAccelerationSpeed,
            nameof(GasFullAccelerationSpeed));
        RequireFinitePositive(
            GasCutoffSpeed,
            nameof(GasCutoffSpeed));

        if (GasCutoffSpeed <= GasFullAccelerationSpeed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(GasCutoffSpeed),
                GasCutoffSpeed,
                "Gas cutoff speed must exceed the full-acceleration speed.");
        }
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
