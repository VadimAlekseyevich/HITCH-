using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class ViewStateTests
{
    [Fact]
    public void LookDeltaUpdatesYawAndPitchInRadians()
    {
        var simulation = new GameSimulation(new SimulationConfig(), SimulationState.Initial);
        var world = new NoHitWorldQuery();
        var input = PlayerInput.Neutral with
        {
            LookDelta = new Vector2(0.25f, -0.1f),
        };

        var state = simulation.Step(input, world);

        Assert.InRange(MathF.Abs(state.Player.ViewYawRadians - 0.25f), 0f, 1e-6f);
        Assert.InRange(MathF.Abs(state.Player.ViewPitchRadians - (-0.1f)), 0f, 1e-6f);
    }

    [Fact]
    public void PitchIsClampedByConfiguration()
    {
        var config = new SimulationConfig { ViewPitchLimitRadians = 0.5f };
        var simulation = new GameSimulation(config, SimulationState.Initial);
        var world = new NoHitWorldQuery();

        var state = simulation.Step(
            PlayerInput.Neutral with { LookDelta = new Vector2(0f, 5f) },
            world);

        Assert.InRange(MathF.Abs(state.Player.ViewPitchRadians - 0.5f), 0f, 1e-6f);
    }

    [Fact]
    public void YawWrapsToSignedPiRange()
    {
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                ViewYawRadians = MathF.PI - 0.05f,
            },
        };

        var simulation = new GameSimulation(new SimulationConfig(), initial);
        var world = new NoHitWorldQuery();

        var state = simulation.Step(
            PlayerInput.Neutral with { LookDelta = new Vector2(0.1f, 0f) },
            world);

        Assert.InRange(state.Player.ViewYawRadians, -MathF.PI, MathF.PI);
        Assert.True(state.Player.ViewYawRadians < 0f);
    }

    [Fact]
    public void BodyOrientationDoesNotChangeWhenLookingAround()
    {
        var initial = SimulationState.Initial;
        var simulation = new GameSimulation(new SimulationConfig(), initial);
        var world = new NoHitWorldQuery();

        var state = simulation.Step(
            PlayerInput.Neutral with { LookDelta = new Vector2(0.5f, 0.25f) },
            world);

        Assert.Equal(initial.Player.BodyOrientation, state.Player.BodyOrientation);
    }

    private sealed class NoHitWorldQuery : IWorldQuery
    {
        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }
}
