using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation;

/// <summary>
/// Owns authoritative gameplay state for one simulation instance.
///
/// One call to Step advances exactly one fixed simulation tick.
/// The kernel intentionally has no movement behavior yet.
/// </summary>
public sealed class GameSimulation
{
    public GameSimulation(
        SimulationConfig config,
        SimulationState initialState)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();

        Config = config;
        State = initialState;
    }

    public SimulationConfig Config { get; }

    public SimulationState State { get; private set; }

    public double FixedDeltaSeconds => Config.FixedDeltaSeconds;

    public SimulationState Step(
        in PlayerInput input,
        IWorldQuery world)
    {
        ArgumentNullException.ThrowIfNull(world);

        // Stage 2 establishes state ownership and tick semantics only.
        // Later stages consume input and world queries to evolve PlayerState.
        _ = input;

        State = State with
        {
            Tick = State.Tick.Next(),
        };

        return State;
    }
}
