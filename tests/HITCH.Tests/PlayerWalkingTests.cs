using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class PlayerWalkingTests
{
    [Fact]
    public void ForwardInputAtZeroYawAcceleratesTowardNegativeZ()
    {
        var config = new SimulationConfig();
        var simulation = CreateGroundedSimulation(config);

        var after = simulation.Step(
            new PlayerInput(
                new Vector2(0f, 1f),
                Vector2.Zero,
                0f,
                PlayerButtons.None),
            new FlatFloorWorld());

        var expectedSpeed = config.Locomotion.GroundAcceleration / config.TickRateHz;

        Assert.InRange(Math.Abs(after.Player.Velocity.X), 0f, 1e-6f);
        Assert.InRange(
            Math.Abs(after.Player.Velocity.Z + expectedSpeed),
            0f,
            1e-5f);
    }

    [Fact]
    public void ForwardInputUsesViewYaw()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
                ViewYawRadians = -MathF.PI / 2f,
                IsGrounded = true,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            new PlayerInput(
                new Vector2(0f, 1f),
                Vector2.Zero,
                0f,
                PlayerButtons.None),
            new FlatFloorWorld());

        var expectedSpeed = config.Locomotion.GroundAcceleration / config.TickRateHz;

        Assert.InRange(
            Math.Abs(after.Player.Velocity.X - expectedSpeed),
            0f,
            1e-5f);
        Assert.InRange(Math.Abs(after.Player.Velocity.Z), 0f, 1e-5f);
    }

    [Fact]
    public void DiagonalInputDoesNotIncreaseAccelerationMagnitude()
    {
        var config = new SimulationConfig();
        var simulation = CreateGroundedSimulation(config);

        var after = simulation.Step(
            new PlayerInput(
                new Vector2(1f, 1f),
                Vector2.Zero,
                0f,
                PlayerButtons.None),
            new FlatFloorWorld());

        var horizontal = new Vector2(
            after.Player.Velocity.X,
            after.Player.Velocity.Z);
        var expectedSpeed = config.Locomotion.GroundAcceleration / config.TickRateHz;

        Assert.InRange(
            Math.Abs(horizontal.Length() - expectedSpeed),
            0f,
            1e-5f);
    }

    [Fact]
    public void NeutralWalkingDoesNotClampExistingHighMomentum()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
                Velocity = new Vector3(20f, 0f, 0f),
                IsGrounded = true,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new FlatFloorWorld());

        Assert.InRange(Math.Abs(after.Player.Velocity.X - 20f), 0f, 1e-5f);
    }

    private static GameSimulation CreateGroundedSimulation(SimulationConfig config)
    {
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
                IsGrounded = true,
            },
        };

        return new GameSimulation(config, initial);
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
