using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class SimulationContractTests
{
    [Fact]
    public void DefaultConfigUsesTemporarySixtyHertzBaseline()
    {
        var config = new SimulationConfig();
        config.Validate();

        Assert.Equal(60, config.TickRateHz);
        Assert.InRange(Math.Abs(config.FixedDeltaSeconds - (1.0 / 60.0)), 0.0, 1e-12);
    }

    [Fact]
    public void ConfigRejectsNonPositiveTickRate()
    {
        var config = new SimulationConfig { TickRateHz = 0 };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }

    [Fact]
    public void SimulationStateIsSnapshotFriendlyValueData()
    {
        var original = SimulationState.Initial;
        var changed = original with
        {
            Player = original.Player with
            {
                Position = new Vector3(1f, 2f, 3f),
            },
        };

        Assert.Equal(Vector3.Zero, original.Player.Position);
        Assert.Equal(new Vector3(1f, 2f, 3f), changed.Player.Position);
    }

    [Fact]
    public void NeutralInputContainsNoGameplayIntent()
    {
        var input = PlayerInput.Neutral;

        Assert.Equal(Vector2.Zero, input.Move);
        Assert.Equal(Vector2.Zero, input.LookDelta);
        Assert.Equal(0f, input.ReelAxis);
        Assert.Equal(PlayerButtons.None, input.Buttons);
    }

    [Fact]
    public void CapsuleQueryRejectsInvalidRadius()
    {
        var query = new CapsuleSweepQuery(
            Vector3.Zero,
            Vector3.UnitX,
            0f,
            0.5f,
            0.02f,
            uint.MaxValue);

        Assert.Throws<ArgumentOutOfRangeException>(query.Validate);
    }

    [Fact]
    public void OneStepAdvancesExactlyOneTickAndDoesNotMutateOldSnapshot()
    {
        var initial = SimulationState.Initial;
        var simulation = new GameSimulation(new SimulationConfig(), initial);
        var world = new NoHitWorldQuery();

        var after = simulation.Step(PlayerInput.Neutral, world);

        Assert.Equal(0UL, initial.Tick.Value);
        Assert.Equal(1UL, after.Tick.Value);
        Assert.Equal(after, simulation.State);
        Assert.Equal(initial.Player, after.Player);
    }

    [Fact]
    public void MultipleStepsAdvanceMonotonically()
    {
        var simulation = new GameSimulation(new SimulationConfig(), SimulationState.Initial);
        var world = new NoHitWorldQuery();

        simulation.Step(PlayerInput.Neutral, world);
        simulation.Step(PlayerInput.Neutral, world);
        var third = simulation.Step(PlayerInput.Neutral, world);

        Assert.Equal(3UL, third.Tick.Value);
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
