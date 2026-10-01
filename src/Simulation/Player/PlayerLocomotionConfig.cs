namespace Hitch.Simulation.Player;

/// <summary>
/// Tunable parameters for the intentionally modest non-winch locomotion foundation.
/// Units are meters, seconds, radians, and derived SI-style values.
/// </summary>
public sealed record PlayerLocomotionConfig
{
    public float CapsuleRadius { get; init; } = 0.45f;

    public float CapsuleHeight { get; init; } = 1.80f;

    /// <summary>
    /// Vertical eye position relative to the capsule center.
    /// With the default capsule this produces an eye height of 1.55 m above the feet.
    /// </summary>
    public float EyeOffsetFromCapsuleCenter { get; init; } = 0.65f;

    public float Gravity { get; init; } = 11.0f;

    public float GroundAcceleration { get; init; } = 50.0f;

    public float GroundMaxSpeed { get; init; } = 8.0f;

    public float GroundBraking { get; init; } = 65.0f;

    public float JumpSpeed { get; init; } = 5.8f;

    public float AirJumpSpeed { get; init; } = 4.8f;

    public float AirAcceleration { get; init; } = 6.0f;

    public float AirControlMaxSpeed { get; init; } = 7.5f;

    public float GroundProbeDistance { get; init; } = 0.08f;

    public float CollisionMargin { get; init; } = 0.02f;

    public float MaxGroundSlopeDegrees { get; init; } = 50.0f;

    public int MaxSlideIterations { get; init; } = 4;

    public uint WorldCollisionMask { get; init; } = 0b11u;

    public float CapsuleHalfSegmentLength =>
        (CapsuleHeight * 0.5f) - CapsuleRadius;

    public float MinGroundNormalY =>
        MathF.Cos(MaxGroundSlopeDegrees * (MathF.PI / 180f));

    public void Validate()
    {
        RequireFinitePositive(CapsuleRadius, nameof(CapsuleRadius));
        RequireFinitePositive(CapsuleHeight, nameof(CapsuleHeight));

        if (CapsuleHeight <= CapsuleRadius * 2f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CapsuleHeight),
                CapsuleHeight,
                "Capsule height must be greater than its diameter.");
        }

        if (!float.IsFinite(EyeOffsetFromCapsuleCenter))
        {
            throw new ArgumentOutOfRangeException(
                nameof(EyeOffsetFromCapsuleCenter),
                EyeOffsetFromCapsuleCenter,
                "Eye offset must be finite.");
        }

        var halfHeight = CapsuleHeight * 0.5f;
        if (EyeOffsetFromCapsuleCenter <= -halfHeight
            || EyeOffsetFromCapsuleCenter >= halfHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(EyeOffsetFromCapsuleCenter),
                EyeOffsetFromCapsuleCenter,
                "Eye offset must remain inside the capsule's vertical extent.");
        }

        RequireFinitePositive(Gravity, nameof(Gravity));
        RequireFiniteNonNegative(GroundAcceleration, nameof(GroundAcceleration));
        RequireFinitePositive(GroundMaxSpeed, nameof(GroundMaxSpeed));
        RequireFiniteNonNegative(GroundBraking, nameof(GroundBraking));
        RequireFinitePositive(JumpSpeed, nameof(JumpSpeed));
        RequireFinitePositive(AirJumpSpeed, nameof(AirJumpSpeed));
        RequireFiniteNonNegative(AirAcceleration, nameof(AirAcceleration));
        RequireFinitePositive(AirControlMaxSpeed, nameof(AirControlMaxSpeed));
        RequireFinitePositive(GroundProbeDistance, nameof(GroundProbeDistance));
        RequireFiniteNonNegative(CollisionMargin, nameof(CollisionMargin));

        if (!float.IsFinite(MaxGroundSlopeDegrees)
            || MaxGroundSlopeDegrees <= 0f
            || MaxGroundSlopeDegrees >= 90f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxGroundSlopeDegrees),
                MaxGroundSlopeDegrees,
                "Maximum ground slope must be between 0 and 90 degrees.");
        }

        if (MaxSlideIterations is < 1 or > 16)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxSlideIterations),
                MaxSlideIterations,
                "Slide iteration count must be between 1 and 16.");
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
