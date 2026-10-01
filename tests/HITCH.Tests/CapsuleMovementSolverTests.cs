using System.Numerics;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class CapsuleMovementSolverTests
{
    [Fact]
    public void FreeMotionUsesVelocityTimesFixedDelta()
    {
        var config = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(2f, 3f, -4f),
        };

        var result = CapsuleMovementSolver.Move(
            player,
            config,
            new NoHitWorld(),
            0.5f);

        Assert.Equal(new Vector3(1f, 1.5f, -2f), result.Position);
        Assert.Equal(player.Velocity, result.Velocity);
    }

    [Fact]
    public void WallCollisionRemovesOnlyVelocityIntoSurface()
    {
        var config = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(10f, 0f, 10f),
        };
        var world = new FirstSweepHitWorld(
            0.5f,
            -Vector3.UnitX);

        var result = CapsuleMovementSolver.Move(
            player,
            config,
            world,
            1f);

        Assert.Equal(new Vector3(5f, 0f, 10f), result.Position);
        Assert.Equal(new Vector3(0f, 0f, 10f), result.Velocity);
        Assert.Equal(2, world.SweepCount);
    }

    [Fact]
    public void PureHeadOnWallImpactStopsOnlyBecauseNoTangentExists()
    {
        var config = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(10f, 0f, 0f),
        };
        var world = new FirstSweepHitWorld(
            0.5f,
            -Vector3.UnitX);

        var result = CapsuleMovementSolver.Move(
            player,
            config,
            world,
            1f);

        Assert.Equal(new Vector3(5f, 0f, 0f), result.Position);
        Assert.Equal(Vector3.Zero, result.Velocity);
        Assert.Equal(1, world.SweepCount);
    }

    [Fact]
    public void GroundCollisionStillPreservesHorizontalLocomotion()
    {
        var config = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(8f, -4f, 6f),
        };
        var world = new FirstSweepHitWorld(
            0.25f,
            Vector3.UnitY);

        var result = CapsuleMovementSolver.Move(
            player,
            config,
            world,
            1f);

        Assert.Equal(new Vector3(8f, 0f, 6f), result.Velocity);
        Assert.True(result.Position.X > 0f);
        Assert.True(result.Position.Z > 0f);
        Assert.Equal(2, world.SweepCount);
    }

    [Fact]
    public void HighSpeedSweepHonorsSafeTravelFraction()
    {
        var config = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(1000f, 0f, 0f),
        };
        var world = new FirstSweepHitWorld(
            0.1f,
            -Vector3.UnitX);

        var result = CapsuleMovementSolver.Move(
            player,
            config,
            world,
            1f);

        Assert.InRange(Math.Abs(result.Position.X - 100f), 0f, 1e-4f);
        Assert.Equal(0f, result.Velocity.X);
        Assert.Equal(1, world.SweepCount);
    }

    [Fact]
    public void PathologicalZeroDistanceHitsAreBoundedByIterationLimit()
    {
        var config = new PlayerLocomotionConfig
        {
            MaxSlideIterations = 3,
        };
        var player = PlayerState.Initial with
        {
            Velocity = Vector3.UnitX * 5f,
        };
        var world = new RepeatingHitWorld(
            0f,
            Vector3.UnitY);

        var result = CapsuleMovementSolver.Move(
            player,
            config,
            world,
            1f);

        Assert.Equal(3, world.SweepCount);
        Assert.Equal(Vector3.Zero, result.Position);
        Assert.Equal(player.Velocity, result.Velocity);
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

    private sealed class FirstSweepHitWorld : IWorldQuery
    {
        private readonly float _fraction;
        private readonly Vector3 _normal;

        public FirstSweepHitWorld(float fraction, Vector3 normal)
        {
            _fraction = fraction;
            _normal = normal;
        }

        public int SweepCount { get; private set; }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            SweepCount++;

            if (SweepCount == 1)
            {
                hit = new WorldHit(
                    query.StartCenter + (query.Displacement * _fraction),
                    _normal,
                    _fraction,
                    1u);
                return true;
            }

            hit = default;
            return false;
        }
    }

    private sealed class RepeatingHitWorld : IWorldQuery
    {
        private readonly float _fraction;
        private readonly Vector3 _normal;

        public RepeatingHitWorld(float fraction, Vector3 normal)
        {
            _fraction = fraction;
            _normal = normal;
        }

        public int SweepCount { get; private set; }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            SweepCount++;
            hit = new WorldHit(
                query.StartCenter,
                _normal,
                _fraction,
                1u);
            return true;
        }
    }
}
