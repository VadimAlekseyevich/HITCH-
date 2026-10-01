using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class PlayerImpulseAndTimestepTests
{
    [Fact]
    public void ExternalImpulseComposesWithExistingVelocity()
    {
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Velocity = new Vector3(3f, 4f, 5f),
            },
        };
        var simulation = new GameSimulation(new SimulationConfig(), initial);

        simulation.ApplyPlayerVelocityImpulse(new Vector3(10f, -2f, 7f));

        Assert.Equal(new Vector3(13f, 2f, 12f), simulation.State.Player.Velocity);
    }

    [Fact]
    public void ExternalImpulseRejectsNonFiniteComponents()
    {
        var simulation = new GameSimulation(
            new SimulationConfig(),
            SimulationState.Initial);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => simulation.ApplyPlayerVelocityImpulse(
                new Vector3(float.PositiveInfinity, 0f, 0f)));
    }

    [Fact]
    public void HighHorizontalImpulseIsNotClampedByOrdinaryLocomotion()
    {
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
            },
        };
        var simulation = new GameSimulation(new SimulationConfig(), initial);
        simulation.ApplyPlayerVelocityImpulse(new Vector3(100f, 0f, 0f));

        var after = simulation.Step(PlayerInput.Neutral, new NoHitWorld());

        Assert.InRange(Math.Abs(after.Player.Velocity.X - 100f), 0f, 1e-5f);
        Assert.True(after.Player.Position.X > 1f);
    }

    [Fact]
    public void OneSecondFreeFallEndsWithSameVelocityAtSixtyAndOneTwentyHertz()
    {
        var sixty = SimulateFreeFall(tickRateHz: 60, durationSeconds: 1);
        var oneTwenty = SimulateFreeFall(tickRateHz: 120, durationSeconds: 1);

        Assert.InRange(
            Math.Abs(sixty.Player.Velocity.Y - oneTwenty.Player.Velocity.Y),
            0f,
            1e-4f);
        var expectedGravity =
            new SimulationConfig().Locomotion.Gravity;
        Assert.InRange(
            Math.Abs(sixty.Player.Velocity.Y + expectedGravity),
            0f,
            1e-4f);

        // Semi-implicit integration has a small, expected timestep-dependent position difference.
        Assert.InRange(
            Math.Abs(sixty.Player.Position.Y - oneTwenty.Player.Position.Y),
            0f,
            0.1f);
    }

    [Fact]
    public void OneSecondGroundAccelerationConvergesToSameWalkSpeedAtSixtyAndOneTwentyHertz()
    {
        var sixty = SimulateGroundForward(tickRateHz: 60, durationSeconds: 1);
        var oneTwenty = SimulateGroundForward(tickRateHz: 120, durationSeconds: 1);

        var sixtySpeed = new Vector2(
            sixty.Player.Velocity.X,
            sixty.Player.Velocity.Z).Length();
        var oneTwentySpeed = new Vector2(
            oneTwenty.Player.Velocity.X,
            oneTwenty.Player.Velocity.Z).Length();

        var expectedWalkSpeed =
            new SimulationConfig().Locomotion.GroundMaxSpeed;

        Assert.InRange(
            Math.Abs(sixtySpeed - expectedWalkSpeed),
            0f,
            1e-4f);
        Assert.InRange(
            Math.Abs(oneTwentySpeed - expectedWalkSpeed),
            0f,
            1e-4f);
        Assert.InRange(Math.Abs(sixtySpeed - oneTwentySpeed), 0f, 1e-4f);
        Assert.InRange(
            Math.Abs(sixty.Player.Position.Z - oneTwenty.Player.Position.Z),
            0f,
            0.08f);
    }

    private static SimulationState SimulateFreeFall(
        int tickRateHz,
        int durationSeconds)
    {
        var config = new SimulationConfig
        {
            TickRateHz = tickRateHz,
        };
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 100f, 0f),
            },
        };
        var simulation = new GameSimulation(config, initial);
        var world = new NoHitWorld();

        for (var tick = 0; tick < tickRateHz * durationSeconds; tick++)
        {
            simulation.Step(PlayerInput.Neutral, world);
        }

        return simulation.State;
    }

    private static SimulationState SimulateGroundForward(
        int tickRateHz,
        int durationSeconds)
    {
        var config = new SimulationConfig
        {
            TickRateHz = tickRateHz,
        };
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
                IsGrounded = true,
            },
        };
        var simulation = new GameSimulation(config, initial);
        var world = new FlatFloorWorld();
        var input = new PlayerInput(
            new Vector2(0f, 1f),
            Vector2.Zero,
            0f,
            PlayerButtons.None);

        for (var tick = 0; tick < tickRateHz * durationSeconds; tick++)
        {
            simulation.Step(input, world);
        }

        return simulation.State;
    }

    private sealed class NoHitWorld : IWorldQuery
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

    private sealed class FlatFloorWorld : IWorldQuery
    {
        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            if (query.Displacement.Y < 0f)
            {
                hit = new WorldHit(
                    query.StartCenter,
                    Vector3.UnitY,
                    0f,
                    1u);
                return true;
            }

            hit = default;
            return false;
        }
    }
}
