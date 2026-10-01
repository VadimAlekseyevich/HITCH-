using Godot;
using Hitch.GodotIntegration.Input;
using Hitch.GodotIntegration.World;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;

namespace Hitch.GodotIntegration.Bootstrap;

public partial class SimulationBootstrap : Node
{
    private readonly NullWorldQuery _world = new();
    private readonly DeviceInputAdapter _input = new();

    private GameSimulation _simulation = null!;
    private Node3D _yawPivot = null!;
    private Node3D _pitchPivot = null!;
    private Label _status = null!;

    public override void _Ready()
    {
        var config = new SimulationConfig();
        config.Validate();

        _simulation = new GameSimulation(config, SimulationState.Initial);
        _yawPivot = GetNode<Node3D>("PlayerView/Yaw");
        _pitchPivot = GetNode<Node3D>("PlayerView/Yaw/Pitch");
        _status = GetNode<Label>("Hud/Status");

        _input.CaptureMouse();
        ApplySimulationView();
        UpdateStatus(PlayerInput.Neutral);
    }

    public override void _Input(InputEvent @event)
    {
        if (_input.HandleEvent(@event))
        {
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _ = delta;

        var input = _input.ConsumePhysicsTickInput();
        _simulation.Step(input, _world);

        ApplySimulationView();

        // Stage 3 debug UI will replace this bootstrap text with a proper overlay.
        if (_simulation.State.Tick.Value % 10UL == 0UL)
        {
            UpdateStatus(input);
        }
    }

    private void ApplySimulationView()
    {
        var player = _simulation.State.Player;

        _yawPivot.Rotation = new Vector3(0f, player.ViewYawRadians, 0f);
        _pitchPivot.Rotation = new Vector3(player.ViewPitchRadians, 0f, 0f);
    }

    private void UpdateStatus(PlayerInput input)
    {
        var state = _simulation.State.Player;

        _status.Text =
            "Stage 3 input/camera shell\n" +
            $"Tick: {_simulation.State.Tick.Value} | {_simulation.Config.TickRateHz} Hz\n" +
            $"Yaw: {state.ViewYawRadians:F2} | Pitch: {state.ViewPitchRadians:F2}\n" +
            $"Move: ({input.Move.X:F2}, {input.Move.Y:F2}) | Mouse captured: {_input.IsMouseCaptured}";
    }
}
