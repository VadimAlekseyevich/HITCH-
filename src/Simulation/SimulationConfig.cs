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
    }
}
