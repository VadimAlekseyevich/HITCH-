using Godot;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using SimulationTelemetry = Hitch.Simulation.Debug.SimulationTelemetry;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Lightweight developer-only overlay for current grapple feel testing.
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
            ? $"Cable: SET | Distance: {winch.LastActualDistance:F2}m | Pulling: {winch.IsPulling}"
            : "Cable: none";

        var pointLine = winch.HasTarget
            ? $"Point: ({winch.Path.CurrentPullPoint.X:F2}, {winch.Path.CurrentPullPoint.Y:F2}, {winch.Path.CurrentPullPoint.Z:F2})"
            : "Point: —";

        Text =
            "HITCH! Stage 5 automatic-pull iteration\n" +
            "LMB throw/replace cable (cancels pull) | RMB click start pull | WASD move | Space jump | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz | Range: {config.Winch.GrappleRange:F0}m\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded} | Move: ({input.Move.X:F2}, {input.Move.Y:F2})\n" +
            targetLine + "\n" +
            pointLine + "\n" +
            $"Pull: initial impulse {config.Winch.PullInitialImpulse:F1} m/s | accel {winch.LastPullAcceleration:F1}/{config.Winch.PullAcceleration:F1} m/s²\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak pull accel: {telemetry.PeakPullAcceleration:F1}\n" +
            $"Targets: {telemetry.TargetSelectionCount} | pull start/stop: {telemetry.PullStartCount}/{telemetry.PullStopCount} | Mouse: {mouseCaptured}";
    }
}
