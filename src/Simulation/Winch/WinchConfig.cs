namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 iteration 4: one-click grapple + automatic pull.
///
/// One RMB click raycasts a world point, replaces any previous cable, and immediately starts a strong automatic pull.
/// </summary>
public sealed record WinchConfig
{
    public float GrappleRange { get; init; } = 72f;

    public uint GrappleCollisionMask { get; init; } = 1u;

    /// <summary>
    /// Immediate delta-velocity applied once when RMB starts the pull.
    /// This removes the slow spool-up feel from the previous prototype.
    /// </summary>
    public float PullInitialImpulse { get; init; } = 24f;

    /// <summary>
    /// Maximum rate at which the cable changes radial speed toward the anchor.
    /// Tangential velocity is preserved.
    /// </summary>
    public float PullRadialAcceleration { get; init; } = 300f;

    /// <summary>
    /// While pulling, the cable aggressively establishes at least this much inward radial speed.
    /// Existing faster inward speed is not clamped.
    /// </summary>
    public float PullTargetInwardSpeed { get; init; } = 42f;

    /// <summary>
    /// Target surface points cannot be reached by the capsule center exactly.
    /// Pull ends once the center is within this distance.
    /// </summary>
    public float ArrivalDistance { get; init; } = 0.9f;

    public void Validate()
    {
        RequireFinitePositive(GrappleRange, nameof(GrappleRange));
        RequireFinitePositive(PullInitialImpulse, nameof(PullInitialImpulse));
        RequireFinitePositive(PullRadialAcceleration, nameof(PullRadialAcceleration));
        RequireFinitePositive(PullTargetInwardSpeed, nameof(PullTargetInwardSpeed));
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
