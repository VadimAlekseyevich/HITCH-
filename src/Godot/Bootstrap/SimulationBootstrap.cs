using Godot;
using Hitch.GodotIntegration.Debug;
using Hitch.GodotIntegration.Input;
using Hitch.GodotIntegration.World;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using NumericsVector3 = System.Numerics.Vector3;

namespace Hitch.GodotIntegration.Bootstrap;

public partial class SimulationBootstrap : Node
{
    private readonly NullWorldQuery _world = new();
    private readonly DeviceInputAdapter _input = new();

    private GameSimulation _simulation = null!;
    private Node3D _playerRoot = null!;
    private Node3D _yawPivot = null!;
    private Node3D _pitchPivot = null!;
    private SimulationDebugOverlay _overlay = null!;
    private DebugLineDrawer3D _debugLines = null!;
    private PlayerInput _lastInput = PlayerInput.Neutral;

    public override void _Ready()
    {
        _playerRoot = GetNode<Node3D>("PlayerView");
        _yawPivot = GetNode<Node3D>("PlayerView/Yaw");
        _pitchPivot = GetNode<Node3D>("PlayerView/Yaw/Pitch");
        _overlay = GetNode<SimulationDebugOverlay>("Hud/Status");
        _debugLines = GetNode<DebugLineDrawer3D>("DebugLines");

        var config = new SimulationConfig();
        config.Validate();

        var spawn = _playerRoot.Position;
        var initialState = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new NumericsVector3(spawn.X, spawn.Y, spawn.Z),
            },
        };

        _simulation = new GameSimulation(config, initialState);

        _yawPivot.Position = new Vector3(
            0f,
            config.Locomotion.EyeOffsetFromCapsuleCenter,
            0f);

        _input.CaptureMouse();
        ApplySimulationPresentation();
        DrawDebugVectors();
        RefreshOverlay();
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

        _lastInput = _input.ConsumePhysicsTickInput();
        _simulation.Step(_lastInput, _world);

        ApplySimulationPresentation();
        DrawDebugVectors();

        // Developer UI does not need to rebuild strings every simulation tick.
        if (_simulation.State.Tick.Value % 10UL == 0UL)
        {
            RefreshOverlay();
        }
    }

    private void ApplySimulationPresentation()
    {
        var player = _simulation.State.Player;

        _playerRoot.Position = new Vector3(
            player.Position.X,
            player.Position.Y,
            player.Position.Z);

        _yawPivot.Rotation = new Vector3(0f, player.ViewYawRadians, 0f);
        _pitchPivot.Rotation = new Vector3(player.ViewPitchRadians, 0f, 0f);
    }

    private void DrawDebugVectors()
    {
        var player = _simulation.State.Player;
        var origin = _yawPivot.GlobalPosition;
        var forward = -_pitchPivot.GlobalTransform.Basis.Z;
        var velocity = new Vector3(
            player.Velocity.X,
            player.Velocity.Y,
            player.Velocity.Z);

        _debugLines.BeginFrame();
        _debugLines.DrawLine(
            origin,
            origin + (forward * 3f),
            Colors.Cyan);
        _debugLines.DrawLine(
            origin,
            origin + velocity,
            Colors.Orange);
        _debugLines.Commit();
    }

    private void RefreshOverlay()
    {
        _overlay.Refresh(
            _simulation.State,
            _simulation.Config,
            _lastInput,
            _input.IsMouseCaptured);
    }
}
