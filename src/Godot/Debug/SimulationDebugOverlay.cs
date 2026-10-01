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

        var cableLine = winch.HasTarget
            ? $"Cable: PULL | d={winch.LastActualDistance:F2}m | a={winch.LastPullAcceleration:F0} m/s²"
            : "Cable: none";

        Text =
            "HITCH! Stage 5 single-cable prototype\n" +
            "RMB grapple + immediate pull | WASD move | Space jump | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz | Rope length: unlimited\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded} | Move: ({input.Move.X:F2}, {input.Move.Y:F2})\n" +
            cableLine + "\n" +
            $"Pull: impulse {config.Winch.PullInitialImpulse:F1} m/s | inward target {config.Winch.PullTargetInwardSpeed:F1} m/s | radial accel max {config.Winch.PullRadialAcceleration:F0} m/s²\n" +
            $"Arrival contact tolerance: {config.Winch.ArrivalContactTolerance:F2}m\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak pull accel: {telemetry.PeakPullAcceleration:F1}\n" +
            $"Cable starts/stops: {telemetry.PullStartCount}/{telemetry.PullStopCount} | Mouse: {mouseCaptured}";
    }
}
