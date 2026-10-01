namespace Hitch.Simulation;

/// <summary>
/// Configuration that controls the simulation clock itself.
/// Gameplay tuning values belong to subsystem-specific configuration added in later stages.
/// </summary>
public sealed record SimulationConfig
{
    /// <summary>
    /// Temporary bootstrap baseline. Stage 4 will compare 60 Hz and 120 Hz before the project
    /// treats a gameplay simulation frequency as a feel/performance decision.
    /// </summary>
    public const int TemporaryDefaultTickRateHz = 60;

    public int TickRateHz { get; init; } = TemporaryDefaultTickRateHz;

    /// <summary>
    /// Prevents the first-person view from reaching the exact vertical singularity.
    /// </summary>
    public float ViewPitchLimitRadians { get; init; } = (MathF.PI / 2f) - 0.01f;

    public double FixedDeltaSeconds => 1.0 / TickRateHz;

    public void Validate()
    {
        if (TickRateHz <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TickRateHz),
                TickRateHz,
                "Simulation tick rate must be greater than zero.");
        }

        if (!float.IsFinite(ViewPitchLimitRadians)
            || ViewPitchLimitRadians <= 0f
            || ViewPitchLimitRadians >= MathF.PI / 2f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ViewPitchLimitRadians),
                ViewPitchLimitRadians,
                "View pitch limit must be finite, positive, and lower than PI/2.");
        }
    }
}
