namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 iteration 3: selected point + one-click automatic pull.
///
/// LMB selects a world point and cancels any previous pull.
/// One RMB click starts a strong automatic pull until the target is reached or LMB is clicked again.
/// </summary>
public sealed record WinchConfig
{
    public float GrappleRange { get; init; } = 72f;

    public uint GrappleCollisionMask { get; init; } = 1u;

    /// <summary>
    /// Immediate delta-velocity applied once when RMB starts the pull.
    /// This removes the slow spool-up feel from the previous prototype.
    /// </summary>
    public float PullInitialImpulse { get; init; } = 18f;

    /// <summary>
    /// Continuous acceleration toward the selected target while automatic pull is active.
    /// Existing tangential momentum is preserved instead of being replaced every tick.
    /// </summary>
    public float PullAcceleration { get; init; } = 32f;

    /// <summary>
    /// Target surface points cannot be reached by the capsule center exactly.
    /// Pull ends once the center is within this distance.
    /// </summary>
    public float ArrivalDistance { get; init; } = 0.9f;

    public void Validate()
    {
        RequireFinitePositive(GrappleRange, nameof(GrappleRange));
        RequireFinitePositive(PullInitialImpulse, nameof(PullInitialImpulse));
        RequireFinitePositive(PullAcceleration, nameof(PullAcceleration));
        RequireFinitePositive(ArrivalDistance, nameof(ArrivalDistance));
    }

    private static void RequireFinitePositive(float value, string name)
    {
        if (!float.IsFinite(value) || value <= 0f)
        {
            throw new ArgumentOutOfRangeException(
                name,
                value,
                "Value must be finite and positive.");
        }
    }
}
