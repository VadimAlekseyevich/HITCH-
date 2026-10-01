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

    public float PullTargetInwardSpeed { get; init; } = 42f;

    /// <summary>
    /// Extra distance beyond the capsule's geometric support radius used to recognize that
    /// the body has effectively reached the grapple surface.
    ///
    /// The actual completion distance depends on cable direction and capsule shape.
    /// </summary>
    public float ArrivalContactTolerance { get; init; } = 0.06f;

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
        RequireFiniteNonNegative(
            ArrivalContactTolerance,
            nameof(ArrivalContactTolerance));
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
