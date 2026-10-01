namespace Hitch.Simulation.Winch;

/// <summary>
/// Experimental Stage 5 world-anchor winch tuning.
///
/// These defaults are starting values for human feel testing, not final design truth.
/// Units are meters, seconds, and acceleration-style values because player mass is normalized
/// inside the gameplay simulation.
/// </summary>
public sealed record WinchConfig
{
    public float GrappleRange { get; init; } = 24f;

    /// <summary>
    /// World layers that may be attached to. Player locomotion may collide with additional layers.
    /// </summary>
    public uint GrappleCollisionMask { get; init; } = 1u;

    public float ReattachCooldownSeconds { get; init; } = 0.08f;

    public float MinimumRopeLength { get; init; } = 1.5f;

    public float ReelMaxSpeed { get; init; } = 14f;

    public float ReelAcceleration { get; init; } = 48f;

    public float ReelDeceleration { get; init; } = 64f;

    /// <summary>
    /// Inward acceleration contributed by each meter of spring extension.
    /// </summary>
    public float SpringAccelerationPerMeter { get; init; } = 42f;

    /// <summary>
    /// Additional extension used near rest length so the line tends to stay under light tension.
    /// </summary>
    public float PretensionDistance { get; init; } = 0.25f;

    /// <summary>
    /// Damps only radial motion moving away from the anchor.
    /// </summary>
    public float OutwardDampingPerSecond { get; init; } = 5.5f;

    /// <summary>
    /// When the player moves well inside the rest length, take slack up without pushing outward.
    /// </summary>
    public float SlackTakeUpSpeed { get; init; } = 30f;

    public float ReelFalloffStartSpeed { get; init; } = 22f;

    public float ReelFalloffEndSpeed { get; init; } = 55f;

    public float MinimumReelInMultiplier { get; init; } = 0.25f;

    public void Validate()
    {
        RequireFinitePositive(GrappleRange, nameof(GrappleRange));
        RequireFiniteNonNegative(ReattachCooldownSeconds, nameof(ReattachCooldownSeconds));
        RequireFinitePositive(MinimumRopeLength, nameof(MinimumRopeLength));
        RequireFinitePositive(ReelMaxSpeed, nameof(ReelMaxSpeed));
        RequireFinitePositive(ReelAcceleration, nameof(ReelAcceleration));
        RequireFinitePositive(ReelDeceleration, nameof(ReelDeceleration));
        RequireFiniteNonNegative(SpringAccelerationPerMeter, nameof(SpringAccelerationPerMeter));
        RequireFiniteNonNegative(PretensionDistance, nameof(PretensionDistance));
        RequireFiniteNonNegative(OutwardDampingPerSecond, nameof(OutwardDampingPerSecond));
        RequireFiniteNonNegative(SlackTakeUpSpeed, nameof(SlackTakeUpSpeed));
        RequireFiniteNonNegative(ReelFalloffStartSpeed, nameof(ReelFalloffStartSpeed));
        RequireFinitePositive(ReelFalloffEndSpeed, nameof(ReelFalloffEndSpeed));

        if (ReelFalloffEndSpeed <= ReelFalloffStartSpeed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ReelFalloffEndSpeed),
                ReelFalloffEndSpeed,
                "Reel falloff end speed must be greater than its start speed.");
        }

        if (!float.IsFinite(MinimumReelInMultiplier)
            || MinimumReelInMultiplier <= 0f
            || MinimumReelInMultiplier > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumReelInMultiplier),
                MinimumReelInMultiplier,
                "Minimum reel-in multiplier must be within (0, 1].");
        }
    }

    private static void RequireFinitePositive(float value, string name)
    {
        if (!float.IsFinite(value) || value <= 0f)
        {
            throw new ArgumentOutOfRangeException(name, value, "Value must be finite and positive.");
        }
    }

    private static void RequireFiniteNonNegative(float value, string name)
    {
        if (!float.IsFinite(value) || value < 0f)
        {
            throw new ArgumentOutOfRangeException(name, value, "Value must be finite and non-negative.");
        }
    }
}
