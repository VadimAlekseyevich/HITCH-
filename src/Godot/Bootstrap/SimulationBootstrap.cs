using Godot;
using Hitch.GodotIntegration.Debug;
using Hitch.GodotIntegration.Input;
using Hitch.GodotIntegration.World;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;
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
    private Camera3D _camera = null!;
    private SimulationDebugOverlay _overlay = null!;
    private DebugLineDrawer3D _debugLines = null!;
    private PlayerInput _lastInput = PlayerInput.Neutral;
    private bool _spawnSmokeValidationEnabled;
    private float _expectedSpawnCenterY;

    private const ulong SpawnSmokeValidationTick = 45UL;
    private const float SpawnSmokeMaximumDropMeters = 0.10f;

    private const float BaseCameraFovDegrees = 80f;
    private const float MaximumSpeedCameraFovDegrees = 96f;
    private const float SpeedFovStartMetersPerSecond = 12f;
    private const float SpeedFovMaximumMetersPerSecond = 36f;
    private const float SpeedFovResponsePerTick = 0.18f;

    public override void _Ready()
    {
        var spawnMarker = GetNode<Marker3D>("Spawn");
        _playerRoot = GetNode<Node3D>("PlayerView");
        _yawPivot = GetNode<Node3D>("PlayerView/Yaw");
        _pitchPivot = GetNode<Node3D>("PlayerView/Yaw/Pitch");
        _camera = GetNode<Camera3D>("PlayerView/Yaw/Pitch/Camera3D");
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

        _camera.Fov = BaseCameraFovDegrees;

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

        var speed = player.Velocity.Length();
        var normalizedSpeed = Mathf.Clamp(
            (speed - SpeedFovStartMetersPerSecond)
            / (SpeedFovMaximumMetersPerSecond - SpeedFovStartMetersPerSecond),
            0f,
            1f);
        var easedSpeed = normalizedSpeed
            * normalizedSpeed
            * (3f - (2f * normalizedSpeed));
        var targetFov = Mathf.Lerp(
            BaseCameraFovDegrees,
            MaximumSpeedCameraFovDegrees,
            easedSpeed);

        _camera.Fov = Mathf.Lerp(
            _camera.Fov,
            targetFov,
            SpeedFovResponsePerTick);
    }

    private void DrawDebugVectors()
    {
        var state = _simulation.State;
        var player = state.Player;
        var origin = _yawPivot.GlobalPosition;
        var velocity = ToGodot(player.Velocity);
        _debugLines.BeginFrame();

        _debugLines.DrawLine(
            origin,
            origin + velocity,
            Colors.Orange);

        if (state.Winch.HasTarget)
        {
            DrawCableDebug(
                state.Winch,
                ToGodot(player.Position),
                Colors.Yellow);
        }

        if (state.SecondaryWinch.HasTarget)
        {
            DrawCableDebug(
                state.SecondaryWinch,
                ToGodot(player.Position),
                Colors.Magenta);
        }

        _debugLines.Commit();
    }

    private void DrawCableDebug(
        in WinchState cable,
        Vector3 playerPosition,
        Color cableColor)
    {
        if (!cable.HasTarget)
        {
            return;
        }

        var from = playerPosition;

        for (var i = 0; i <= cable.Path.ContactCount; i++)
        {
            var point = ToGodot(
                cable.Path.GetPathPointFromPlayer(i));

            _debugLines.DrawLine(
                from,
                point,
                cableColor);

            if (i < cable.Path.ContactCount)
            {
                DrawCableMarker(
                    point,
                    Colors.Cyan,
                    0.22f);
            }

            from = point;
        }

        DrawCableMarker(
            ToGodot(cable.Path.WorldAnchor),
            cableColor,
            0.35f);
    }

    private void DrawCableMarker(
        Vector3 point,
        Color color,
        float markerSize)
    {
        _debugLines.DrawLine(
            point - (Vector3.Right * markerSize),
            point + (Vector3.Right * markerSize),
            color);
        _debugLines.DrawLine(
            point - (Vector3.Up * markerSize),
            point + (Vector3.Up * markerSize),
            color);
        _debugLines.DrawLine(
            point - (Vector3.Back * markerSize),
            point + (Vector3.Back * markerSize),
            color);
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

        if (!ValidateClosedRoomBounds())
        {
            GetTree().Quit(1);
            return true;
        }

        GD.Print(
            $"HITCH_SMOKE_SPAWN_STABLE tick={_simulation.State.Tick.Value} " +
            $"y={player.Position.Y:F4} grounded={player.IsGrounded}");
        GD.Print("HITCH_SMOKE_ROOM_CLOSED west/east/back/front/ceiling=True");
        GetTree().Quit(0);
        return true;
    }

    private bool ValidateClosedRoomBounds()
    {
        const uint collisionMask = 0b11u;
        var probeY = MovementLabBuilder.RoomHeight - 20f;
        var origin = new NumericsVector3(0f, probeY, 0f);

        var checks = new (string Name, NumericsVector3 End)[]
        {
            ("west", new NumericsVector3(
                -MovementLabBuilder.RoomHalfWidth - 15f,
                probeY,
                0f)),
            ("east", new NumericsVector3(
                MovementLabBuilder.RoomHalfWidth + 15f,
                probeY,
                0f)),
            ("back", new NumericsVector3(
                0f,
                probeY,
                -MovementLabBuilder.RoomHalfDepth - 15f)),
            ("front", new NumericsVector3(
                0f,
                probeY,
                MovementLabBuilder.RoomHalfDepth + 15f)),
            ("ceiling", new NumericsVector3(
                0f,
                MovementLabBuilder.RoomHeight + 15f,
                0f)),
        };

        foreach (var check in checks)
        {
            var query = new RayQuery(
                origin,
                check.End,
                collisionMask);

            if (_world.TryRaycast(query, out _))
            {
                continue;
            }

            GD.PushError(
                $"HITCH_SMOKE_ROOM_OPEN missing={check.Name}");
            return false;
        }

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
