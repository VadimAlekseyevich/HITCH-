using Hitch.Simulation.Winch;

namespace Hitch.Simulation.State;

/// <summary>
/// Explicit snapshot of current gameplay simulation state.
/// More entities/subsystems are added only when their roadmap stages require them.
/// </summary>
public readonly record struct SimulationState(
    SimulationTick Tick,
    PlayerState Player,
    WinchState Winch)
{
    public static SimulationState Initial => new(
        SimulationTick.Zero,
        PlayerState.Initial,
        WinchState.Initial);
}
