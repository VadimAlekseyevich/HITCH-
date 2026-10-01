using Godot;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using SimulationTelemetry = Hitch.Simulation.Debug.SimulationTelemetry;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Lightweight developer-only overlay for inspecting simulation/input/winch state.
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

        var winchLine = winch.IsAttached
            ? $"Winch: ATTACHED | Rest: {winch.RestLength:F2}m | Actual: {winch.LastActualDistance:F2}m"
            : $"Winch: detached | Cooldown: {winch.ReattachCooldownRemaining:F3}s";

        var anchorLine = winch.IsAttached
            ? $"Anchor: ({winch.Path.CurrentPullPoint.X:F2}, {winch.Path.CurrentPullPoint.Y:F2}, {winch.Path.CurrentPullPoint.Z:F2})"
            : "Anchor: —";

        Text =
            "HITCH! Stage 5 winch debug\n" +
            "Controls: hold RMB grapple | Q reel out | E reel in | WASD move | Space jump | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded}\n" +
            $"Move: ({input.Move.X:F2}, {input.Move.Y:F2}) | Reel input: {input.ReelAxis:F2}\n" +
            $"Yaw: {player.ViewYawRadians:F2} | Pitch: {player.ViewPitchRadians:F2}\n" +
            winchLine + "\n" +
            anchorLine + "\n" +
            $"Reel velocity: {winch.ReelVelocity:F2} m/s | Tension accel: {winch.LastTensionAcceleration:F2} m/s²\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak tension: {telemetry.PeakWinchTensionAcceleration:F2}\n" +
            $"Attach/detach: {telemetry.AttachCount}/{telemetry.DetachCount} | Mouse captured: {mouseCaptured}";
    }
}
