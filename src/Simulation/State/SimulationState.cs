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
    /// <summary>
    /// Second independent ODM cable. Winch remains the left/primary cable for compatibility.
    /// </summary>
    public WinchState SecondaryWinch { get; init; } = WinchState.Initial;

    /// <summary>
    /// 0 = next RMB targets left/primary cable, 1 = right/secondary cable.
    /// Successful play does not require holding RMB; clicks alternate cable sides.
    /// </summary>
    public byte NextGrappleSlot { get; init; }

    public static SimulationState Initial => new(
        SimulationTick.Zero,
        PlayerState.Initial,
        WinchState.Initial);
}
