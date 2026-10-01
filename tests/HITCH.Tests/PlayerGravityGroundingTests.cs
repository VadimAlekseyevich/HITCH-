using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class PlayerGravityGroundingTests
{
    [Fact]
    public void FreeFallAppliesGravityAndMovesDuringSameTick()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 10f, 0f),
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        var expectedVelocityY = -config.Locomotion.Gravity / config.TickRateHz;
        var expectedPositionY = 10f + (expectedVelocityY / config.TickRateHz);

        Assert.InRange(
            Math.Abs(after.Player.Velocity.Y - expectedVelocityY),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(after.Player.Position.Y - expectedPositionY),
            0f,
            1e-5f);
        Assert.False(after.Player.IsGrounded);
    }

    [Fact]
    public void UpwardFloorContactCancelsDownwardVelocityAndMarksGrounded()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new FlatFloorWorld());

        Assert.True(after.Player.IsGrounded);
        Assert.Equal(0f, after.Player.Velocity.Y);
        Assert.Equal(initial.Player.Position, after.Player.Position);
    }

    [Fact]
    public void SteepContactIsNotGround()
    {
        var config = new SimulationConfig();
        var state = PlayerState.Initial with
        {
            Position = new Vector3(0f, 1f, 0f),
            Velocity = -Vector3.UnitY,
        };

        var grounded = Hitch.Simulation.Player.GroundDetector.IsGrounded(
            state,
            config.Locomotion,
            new FixedNormalWorld(Vector3.UnitX));

        Assert.False(grounded);
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

    private sealed class FixedNormalWorld : IWorldQuery
    {
        private readonly Vector3 _normal;

        public FixedNormalWorld(Vector3 normal)
        {
            _normal = normal;
        }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            hit = new WorldHit(
                query.StartCenter,
                _normal,
                0f,
                1u);
            return true;
        }
    }
}
