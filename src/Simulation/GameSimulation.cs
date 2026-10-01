using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation;

/// <summary>
/// Owns authoritative gameplay state for one simulation instance.
///
/// One call to Step advances exactly one fixed simulation tick.
/// The kernel intentionally has no movement behavior yet.
/// </summary>
public sealed class GameSimulation
{
    public GameSimulation(
        SimulationConfig config,
        SimulationState initialState)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();

        Config = config;
        State = initialState;
    }

    public SimulationConfig Config { get; }

    public SimulationState State { get; private set; }

    public double FixedDeltaSeconds => Config.FixedDeltaSeconds;

    public SimulationState Step(
        in PlayerInput input,
        IWorldQuery world)
    {
        ArgumentNullException.ThrowIfNull(world);

        // View orientation is gameplay state because aiming/grapple direction will depend on it.
        // Input adapters provide LookDelta in radians; raw mouse pixels never enter simulation.
        var player = State.Player;
        var yaw = WrapRadians(player.ViewYawRadians + input.LookDelta.X);
        var pitch = Math.Clamp(
            player.ViewPitchRadians + input.LookDelta.Y,
            -Config.ViewPitchLimitRadians,
            Config.ViewPitchLimitRadians);

        State = State with
        {
            Tick = State.Tick.Next(),
            Player = player with
            {
                ViewYawRadians = yaw,
                ViewPitchRadians = pitch,
            },
        };

        return State;
    }

    private static float WrapRadians(float angle)
    {
        var wrapped = (angle + MathF.PI) % MathF.Tau;

        if (wrapped < 0f)
        {
            wrapped += MathF.Tau;
        }

        return wrapped - MathF.PI;
    }
}
