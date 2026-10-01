using Godot;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using SimulationTelemetry = Hitch.Simulation.Debug.SimulationTelemetry;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Lightweight developer-only overlay for inspecting simulation/input/direct-pull state.
/// </summary>
public partial class SimulationDebugOverlay : Label
{
    public void Refresh(
        in SimulationState simulationState,
        SimulationConfig config,
        in PlayerInput input,
        SimulationTelemetry telemetry,
        bool mouseCaptured)
    {
        var player = simulationState.Player;
        var winch = simulationState.Winch;
        var speed = player.Velocity.Length();

        var targetLine = winch.HasTarget
            ? $"Target: SELECTED | Distance: {winch.LastActualDistance:F2}m | Pulling: {winch.IsPulling}"
            : "Target: none";

        var pointLine = winch.HasTarget
            ? $"Point: ({winch.Path.CurrentPullPoint.X:F2}, {winch.Path.CurrentPullPoint.Y:F2}, {winch.Path.CurrentPullPoint.Z:F2})"
            : "Point: —";

        Text =
            "HITCH! Stage 5 direct-pull iteration\n" +
            "Controls: LMB select point | hold RMB pull | WASD move | Space jump | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded}\n" +
            $"Move: ({input.Move.X:F2}, {input.Move.Y:F2})\n" +
            $"Yaw: {player.ViewYawRadians:F2} | Pitch: {player.ViewPitchRadians:F2}\n" +
            targetLine + "\n" +
            pointLine + "\n" +
            $"Pull speed: {winch.LastPullSpeed:F2} m/s | Config pull: {config.Winch.PullSpeed:F2} m/s\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak pull: {telemetry.PeakPullSpeed:F2}\n" +
            $"Targets: {telemetry.TargetSelectionCount} | pull start/stop: {telemetry.PullStartCount}/{telemetry.PullStopCount} | Mouse captured: {mouseCaptured}";
    }
}
