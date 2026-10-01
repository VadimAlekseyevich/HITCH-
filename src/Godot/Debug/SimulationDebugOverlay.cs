using Godot;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Lightweight developer-only overlay for inspecting simulation/input state.
/// </summary>
public partial class SimulationDebugOverlay : Label
{
    public void Refresh(
        in SimulationState simulationState,
        SimulationConfig config,
        in PlayerInput input,
        bool mouseCaptured)
    {
        var player = simulationState.Player;
        var speed = player.Velocity.Length();

        Text =
            "HITCH! Stage 3 debug\n" +
            $"Tick: {simulationState.Tick.Value} | Rate: {config.TickRateHz} Hz\n" +
            $"Position: ({player.Position.X:F2}, {player.Position.Y:F2}, {player.Position.Z:F2})\n" +
            $"Velocity: ({player.Velocity.X:F2}, {player.Velocity.Y:F2}, {player.Velocity.Z:F2}) | Speed: {speed:F2}\n" +
            $"Grounded: {player.IsGrounded}\n" +
            $"Move: ({input.Move.X:F2}, {input.Move.Y:F2}) | Reel: {input.ReelAxis:F2}\n" +
            $"Yaw: {player.ViewYawRadians:F2} | Pitch: {player.ViewPitchRadians:F2}\n" +
            $"Mouse captured: {mouseCaptured}";
    }
}
