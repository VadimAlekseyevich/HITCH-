using Hitch.Simulation.State;

namespace Hitch.Simulation.Winch;

public readonly record struct WinchStepResult(
    PlayerState Player,
    WinchState Winch,
    bool CompletedThisTick);
