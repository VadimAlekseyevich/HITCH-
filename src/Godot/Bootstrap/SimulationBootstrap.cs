using Godot;
using Hitch.GodotIntegration.Debug;
using Hitch.GodotIntegration.Input;
using Hitch.GodotIntegration.World;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using NumericsVector3 = System.Numerics.Vector3;

namespace Hitch.GodotIntegration.Bootstrap;

public partial class SimulationBootstrap : Node
{
    private GodotWorldQuery _world = null!;
    private readonly DeviceInputAdapter _input = new();

    private GameSimulation _simulation = null!;
    private Node3D _playerRoot = null!;
    private Node3D _yawPivot = null!;
    private Node3D _pitchPivot = null!;
    private SimulationDebugOverlay _overlay = null!;
    private DebugLineDrawer3D _debugLines = null!;
    private PlayerInput _lastInput = PlayerInput.Neutral;
    private bool _spawnSmokeValidationEnabled;
    private float _expectedSpawnCenterY;

    private const ulong SpawnSmokeValidationTick = 45UL;
    private const float SpawnSmokeMaximumDropMeters = 0.10f;

    public override void _Ready()
    {
        var spawnMarker = GetNode<Marker3D>("Spawn");
        _playerRoot = GetNode<Node3D>("PlayerView");
        _yawPivot = GetNode<Node3D>("PlayerView/Yaw");
        _pitchPivot = GetNode<Node3D>("PlayerView/Yaw/Pitch");
        _overlay = GetNode<SimulationDebugOverlay>("Hud/Status");
        _debugLines = GetNode<DebugLineDrawer3D>("DebugLines");

        var config = SimulationConfigLoader.Load();
        Engine.PhysicsTicksPerSecond = config.TickRateHz;

        var spawn = spawnMarker.GlobalPosition;
        var capsuleCenterY =
            spawn.Y
            + (config.Locomotion.CapsuleHeight * 0.5f)
            + config.Locomotion.CollisionMargin;

        var initialState = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new NumericsVector3(spawn.X, capsuleCenterY, spawn.Z),
            },
        };

        _expectedSpawnCenterY = capsuleCenterY;
        _spawnSmokeValidationEnabled = HasUserArgument("--hitch-smoke");

        _simulation = new GameSimulation(config, initialState);
        _world = new GodotWorldQuery(_playerRoot);

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

        if (ValidateSpawnSmokeIfRequested())
        {
            return;
        }

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
        var state = _simulation.State;
        var player = state.Player;
        var origin = _yawPivot.GlobalPosition;
        var velocity = ToGodot(player.Velocity);
        var aimRay = WinchSystem.BuildAimRay(
            player,
            _simulation.Config.Winch,
            _simulation.Config.Locomotion);

        _debugLines.BeginFrame();

        _debugLines.DrawLine(
            ToGodot(aimRay.From),
            ToGodot(aimRay.To),
            new Color(0.2f, 0.8f, 1f, 0.45f));

        _debugLines.DrawLine(
            origin,
            origin + velocity,
            Colors.Orange);

        if (state.Winch.IsAttached)
        {
            var anchor = ToGodot(state.Winch.Path.CurrentPullPoint);
            var playerCenter = ToGodot(player.Position);

            _debugLines.DrawLine(
                playerCenter,
                anchor,
                Colors.Yellow);

            const float markerSize = 0.35f;
            _debugLines.DrawLine(
                anchor - (Vector3.Right * markerSize),
                anchor + (Vector3.Right * markerSize),
                Colors.LimeGreen);
            _debugLines.DrawLine(
                anchor - (Vector3.Up * markerSize),
                anchor + (Vector3.Up * markerSize),
                Colors.LimeGreen);
            _debugLines.DrawLine(
                anchor - (Vector3.Back * markerSize),
                anchor + (Vector3.Back * markerSize),
                Colors.LimeGreen);
        }

        _debugLines.Commit();
    }

    private bool ValidateSpawnSmokeIfRequested()
    {
        if (!_spawnSmokeValidationEnabled
            || _simulation.State.Tick.Value < SpawnSmokeValidationTick)
        {
            return false;
        }

        var player = _simulation.State.Player;
        var minimumAllowedY =
            _expectedSpawnCenterY - SpawnSmokeMaximumDropMeters;

        if (player.Position.Y < minimumAllowedY || !player.IsGrounded)
        {
            GD.PushError(
                $"HITCH_SMOKE_SPAWN_FAILED tick={_simulation.State.Tick.Value} " +
                $"y={player.Position.Y:F4} expected>={minimumAllowedY:F4} " +
                $"grounded={player.IsGrounded}");
            GetTree().Quit(1);
            return true;
        }

        GD.Print(
            $"HITCH_SMOKE_SPAWN_STABLE tick={_simulation.State.Tick.Value} " +
            $"y={player.Position.Y:F4} grounded={player.IsGrounded}");
        GetTree().Quit(0);
        return true;
    }

    private static bool HasUserArgument(string expected)
    {
        foreach (var argument in OS.GetCmdlineUserArgs())
        {
            if (argument == expected)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector3 ToGodot(NumericsVector3 value) =>
        new(value.X, value.Y, value.Z);

    private void RefreshOverlay()
    {
        _overlay.Refresh(
            _simulation.State,
            _simulation.Config,
            _lastInput,
            _simulation.Telemetry,
            _input.IsMouseCaptured);
    }
}
