using Godot;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using SimulationTelemetry = Hitch.Simulation.Debug.SimulationTelemetry;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Lightweight developer-only overlay for dual-cable grapple feel testing.
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
        var speed = player.Velocity.Length();

        Text =
            "HITCH! Stage 5 dual-cable prototype\n" +
            "LMB left cable | RMB right cable | WASD move | Space jump | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz | Rope length: unlimited\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded} | Move: ({input.Move.X:F2}, {input.Move.Y:F2})\n" +
            CableLine("LEFT ", simulationState.Winch.Left) + "\n" +
            CableLine("RIGHT", simulationState.Winch.Right) + "\n" +
            $"Pull: impulse {config.Winch.PullInitialImpulse:F1} m/s | inward target {config.Winch.PullTargetInwardSpeed:F1} m/s | radial accel max {config.Winch.PullRadialAcceleration:F0} m/s²\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak pull accel: {telemetry.PeakPullAcceleration:F1} | peak cables: {telemetry.PeakActiveCables}\n" +
            $"Cable starts/stops: {telemetry.PullStartCount}/{telemetry.PullStopCount} | Mouse: {mouseCaptured}";
    }

    private static string CableLine(
        string label,
        in WinchCableState cable)
    {
        if (!cable.HasTarget)
        {
            return $"{label}: none";
        }

        var point = cable.Path.CurrentPullPoint;

        return
            $"{label}: PULL | d={cable.LastActualDistance:F2}m | " +
            $"a={cable.LastPullAcceleration:F0} m/s² | " +
            $"point=({point.X:F1},{point.Y:F1},{point.Z:F1})";
    }
}
