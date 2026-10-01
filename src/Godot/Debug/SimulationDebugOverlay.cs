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

        var cableLine = "Cable: none";
        if (winch.HasTarget)
        {
            var toAnchor =
                winch.Path.CurrentPullPoint - player.Position;
            var distance = toAnchor.Length();
            var radialSpeed = 0f;
            var tangentialSpeed = speed;

            if (distance > 1e-5f)
            {
                var direction = toAnchor / distance;
                radialSpeed =
                    System.Numerics.Vector3.Dot(
                        player.Velocity,
                        direction);
                var tangent =
                    player.Velocity
                    - (direction * radialSpeed);
                tangentialSpeed = tangent.Length();
            }

            var mode = winch.IsLatched ? "LATCHED" : "PULL";
            cableLine =
                $"Cable: {mode} | d={winch.LastActualDistance:F2}m | bends={winch.Path.ContactCount} | radial={radialSpeed:F1} | tangent={tangentialSpeed:F1} m/s";
        }

        Text =
            "HITCH! Stage 5 single-cable prototype\n" +
            "RMB grapple/retarget | Space jump or detach | WASD move | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz | Rope length: unlimited\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded} | Move: ({input.Move.X:F2}, {input.Move.Y:F2})\n" +
            cableLine + "\n" +
            $"Pull: sustained {config.Winch.PullTargetInwardSpeed:F1}-{config.Winch.PullLongRangeInwardSpeed:F1} m/s | burst {config.Winch.PullLaunchInitialMultiplier:F2}x->{config.Winch.PullLaunchPeakMultiplier:F2}x | gravity ON while pulling\n" +
            $"Arrival contact tolerance: {config.Winch.ArrivalContactTolerance:F2}m\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak pull accel: {telemetry.PeakPullAcceleration:F1}\n" +
            $"Cable starts/stops: {telemetry.PullStartCount}/{telemetry.PullStopCount} | Mouse: {mouseCaptured}";
    }
}
