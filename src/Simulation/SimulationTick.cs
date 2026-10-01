namespace Hitch.Simulation;

/// <summary>
/// Monotonic simulation-step index. It is deliberately independent from render frames and wall-clock time.
/// </summary>
public readonly record struct SimulationTick(ulong Value)
{
    public static SimulationTick Zero => new(0);

    public SimulationTick Next() => new(checked(Value + 1));
}
