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
