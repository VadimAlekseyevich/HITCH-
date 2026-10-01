namespace Hitch.Simulation.Winch;

/// <summary>
/// Current Stage 5 iteration: simple selected-point direct pull.
///
/// This deliberately replaces the previous spring/reel experiment for the active human playtest.
/// It is not assumed to be the final HITCH! winch model.
/// </summary>
public sealed record WinchConfig
{
    public float GrappleRange { get; init; } = 24f;

    /// <summary>
    /// World layers that may be selected as grapple targets.
    /// </summary>
    public uint GrappleCollisionMask { get; init; } = 1u;

    /// <summary>
    /// While RMB pull is active, player velocity is immediately set toward the selected point
    /// at this speed. There is intentionally no acceleration ramp in this prototype.
    /// </summary>
    public float PullSpeed { get; init; } = 18f;

    /// <summary>
    /// The target lives on a surface while the player is represented by a capsule center.
    /// Arrival must therefore happen before the center tries to reach the exact surface point.
    /// </summary>
    public float ArrivalDistance { get; init; } = 0.75f;

    public void Validate()
    {
        RequireFinitePositive(GrappleRange, nameof(GrappleRange));
        RequireFinitePositive(PullSpeed, nameof(PullSpeed));
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
