using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchGameSimulationIntegrationTests
{
    [Fact]
    public void RmbCreatesSingleCableAndContractsDistance()
    {
        var config = TestSimulationConfig();
        var target = new Vector3(0f, 5f, -30f);
        var world = new FixedGrappleWorld(target);
        var simulation = CreateAirborneSimulation(config);

        var started = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(started.Winch.IsPulling);
        Assert.True(started.Player.Velocity.Z < -28f);

        var initialDistance = started.Winch.LastActualDistance;

        for (var i = 0; i < 8; i++)
        {
            simulation.Step(PlayerInput.Neutral, world);
        }

        Assert.True(
            simulation.State.Winch.LastActualDistance
            < initialDistance);
    }

    [Fact]
    public void CeilingContactEndsPullWithoutResidualHorizontalMotion()
    {
        var config = TestSimulationConfig() with
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 12f,
                AirAcceleration = 0f,
            },
        };

        var playerPosition = new Vector3(0f, 5f, 0f);
        var ceilingPoint = playerPosition + new Vector3(0f, 0.95f, 0f);

        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = playerPosition,
                Velocity = new Vector3(9f, 7f, 6f),
                IsGrounded = false,
            },
            Winch = new WinchState(
                WinchTargetState.Selected,
                WinchPathState.AtWorldAnchor(ceilingPoint),
                true,
                0.95f,
                config.Winch.PullRadialAcceleration),
        };

        var simulation = new GameSimulation(
            config,
            initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        Assert.False(after.Winch.HasTarget);
        Assert.False(after.Winch.IsPulling);

        // Full reel-in clears orbital/tangential motion.
        Assert.InRange(
            Math.Abs(after.Player.Velocity.X),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(after.Player.Velocity.Z),
            0f,
            1e-5f);

        // Then one ordinary gravity tick is applied.
        Assert.True(after.Player.Velocity.Y < 0f);
        Assert.True(after.Player.Velocity.Y > -1f);
    }

    [Fact]
    public void RetargetPreservesExistingMomentumAndStartsNewPull()
    {
        var config = TestSimulationConfig();
        var world = new MutableGrappleWorld(
            new Vector3(20f, 8f, -20f));
        var simulation = CreateAirborneSimulation(config);

        simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        var before = simulation.State.Player.Velocity;

        world.Anchor = new Vector3(-25f, 20f, 10f);
        var retargeted = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(retargeted.Winch.IsPulling);
        Assert.Equal(
            world.Anchor,
            retargeted.Winch.Path.CurrentPullPoint);
        Assert.NotEqual(
            Vector3.Zero,
            retargeted.Player.Velocity);
        Assert.NotEqual(
            before,
            retargeted.Player.Velocity);
    }

    private static SimulationConfig TestSimulationConfig() =>
        new()
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 1f,
                AirAcceleration = 0f,
            },
            Winch = new WinchConfig
            {
                PullInitialImpulse = 24f,
                PullRadialAcceleration = 300f,
                PullTargetInwardSpeed = 42f,
                ArrivalContactTolerance = 0.06f,
            },
        };

    private static GameSimulation CreateAirborneSimulation(
        SimulationConfig config) =>
        new(
            config,
            SimulationState.Initial with
            {
                Player = SimulationState.Initial.Player with
                {
                    Position = new Vector3(0f, 5f, 0f),
                    IsGrounded = false,
                },
            });

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private class FixedGrappleWorld : IWorldQuery
    {
        protected Vector3 CurrentAnchor;

        public FixedGrappleWorld(Vector3 anchor)
        {
            CurrentAnchor = anchor;
        }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = new WorldHit(
                CurrentAnchor,
                Vector3.UnitY,
                0.5f,
                1u);
            return true;
        }

        public bool TrySweepCapsule(
            in CapsuleSweepQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }

    private sealed class MutableGrappleWorld : FixedGrappleWorld
    {
        public MutableGrappleWorld(Vector3 anchor)
            : base(anchor)
        {
        }

        public Vector3 Anchor
        {
            get => CurrentAnchor;
            set => CurrentAnchor = value;
        }
    }

    private sealed class NoHitWorld : IWorldQuery
    {
        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(
            in CapsuleSweepQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }
}
