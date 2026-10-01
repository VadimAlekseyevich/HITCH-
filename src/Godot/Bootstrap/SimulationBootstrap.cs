using Godot;
using Hitch.GodotIntegration.World;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;

namespace Hitch.GodotIntegration.Bootstrap;

public partial class SimulationBootstrap : Control
{
    private readonly NullWorldQuery _world = new();
    private GameSimulation _simulation = null!;
    private Label _status = null!;

    public override void _Ready()
    {
        var config = new SimulationConfig();
        config.Validate();

        _simulation = new GameSimulation(config, SimulationState.Initial);
        _status = GetNode<Label>("Center/Status");

        UpdateStatus();
    }

    public override void _PhysicsProcess(double delta)
    {
        _ = delta;

        _simulation.Step(PlayerInput.Neutral, _world);

        // Bootstrap-only presentation. Keep UI updates less frequent than the simulation tick.
        if (_simulation.State.Tick.Value % 10UL == 0UL)
        {
            UpdateStatus();
        }
    }

    private void UpdateStatus()
    {
        _status.Text =
            "Simulation kernel is running.\n" +
            $"Tick: {_simulation.State.Tick.Value} | Rate: {_simulation.Config.TickRateHz} Hz";
    }
}
