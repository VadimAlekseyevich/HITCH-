using Godot;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
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
        var speed = player.Velocity.Length();
        var nextSide =
            simulationState.NextGrappleSlot == 0
                ? "L"
                : "R";

        Text =
            "HITCH! Stage 5 dual-ODM prototype\n" +
            "RMB L/R hook | LMB reel | Space jump/detach | WASD | F2 tuning | Esc cursor\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz | Next RMB: {nextSide}\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded} | Move: ({input.Move.X:F2}, {input.Move.Y:F2})\n" +
            BuildCableLine("L", simulationState.Winch, player) + "\n" +
            BuildCableLine("R", simulationState.SecondaryWinch, player) + "\n" +
            $"ODM: reel {config.Winch.PullTargetInwardSpeed:F1}-{config.Winch.PullLongRangeInwardSpeed:F1} m/s | burst {config.Winch.PullLaunchInitialMultiplier:F2}x->{config.Winch.PullLaunchPeakMultiplier:F2}x | gas {config.Winch.GasAcceleration:F1} m/s²\n" +
            $"Rope max: {config.Winch.MaxRopeLength:F0}m | dual motor/cable: {config.Winch.DualCableMotorScale:F2}x\n" +
            $"Telemetry — peak speed: {telemetry.PeakPlayerSpeed:F2} | avg speed: {telemetry.AveragePlayerSpeed:F2} | peak pull accel: {telemetry.PeakPullAcceleration:F1}\n" +
            $"Cable starts/stops: {telemetry.PullStartCount}/{telemetry.PullStopCount} | Mouse: {mouseCaptured}";
    }

    private static string BuildCableLine(
        string side,
        in WinchState winch,
        in PlayerState player)
    {
        if (!winch.HasTarget)
        {
            return $"{side}: none";
        }

        var toAnchor =
            winch.Path.CurrentPullPoint - player.Position;
        var distance = toAnchor.Length();
        var radialSpeed = 0f;
        var tangentialSpeed =
            player.Velocity.Length();

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

        var mode = winch.IsLatched
            ? "LATCHED"
            : winch.IsPulling
                ? "REEL"
                : "ATTACHED";

        return
            $"{side}: {mode} | rope={winch.RopeLength:F1}m | path={winch.LastActualDistance:F1}m | bends={winch.Path.ContactCount} | radial={radialSpeed:F1} | tangent={tangentialSpeed:F1}";
    }
}
