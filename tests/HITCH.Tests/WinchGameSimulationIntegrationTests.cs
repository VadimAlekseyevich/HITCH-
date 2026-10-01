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
    public void OneRmbClickShootsCableAndStronglyContractsDistance()
    {
        var config = new SimulationConfig
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
                ArrivalDistance = 0.5f,
            },
        };
        var target = new Vector3(0f, 5f, -30f);
        var simulation = CreateAirborneSimulation(config);
        var world = new FixedGrappleWorld(target);

        var started = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(started.Winch.HasTarget);
        Assert.True(started.Winch.IsPulling);
        Assert.True(started.Player.Velocity.Z < -28f);

        var initialDistance = started.Winch.LastActualDistance;

        for (var i = 0; i < 12; i++)
        {
            simulation.Step(PlayerInput.Neutral, world);
        }

        var later = simulation.State;

        Assert.True(later.Winch.IsPulling);
        Assert.True(later.Winch.LastActualDistance < initialDistance - 7f);
        Assert.True(later.Player.Velocity.Z <= -41f);
    }

    [Fact]
    public void StrongPullPreservesTangentialMomentum()
    {
        var config = new SimulationConfig
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 0.1f,
                AirAcceleration = 0f,
            },
            Winch = new WinchConfig
            {
                PullInitialImpulse = 24f,
                PullRadialAcceleration = 300f,
                PullTargetInwardSpeed = 42f,
                ArrivalDistance = 0.5f,
            },
        };
        var target = new Vector3(0f, 5f, -40f);
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                Velocity = new Vector3(18f, 0f, 0f),
                IsGrounded = false,
            },
        };
        var simulation = new GameSimulation(config, initial);
        var world = new FixedGrappleWorld(target);

        var after = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(after.Winch.IsPulling);
        Assert.InRange(
            Math.Abs(after.Player.Velocity.X - 18f),
            0f,
            0.05f);
        Assert.True(after.Player.Velocity.Z < -28f);
    }

    [Fact]
    public void SecondRmbRetargetsAndPullsInSameTick()
    {
        var config = new SimulationConfig
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 6f,
                AirAcceleration = 0f,
            },
            Winch = new WinchConfig
            {
                PullInitialImpulse = 24f,
                PullRadialAcceleration = 300f,
                PullTargetInwardSpeed = 42f,
                ArrivalDistance = 0.5f,
            },
        };
        var world = new MutableGrappleWorld(new Vector3(20f, 8f, -20f));
        var simulation = CreateAirborneSimulation(config);

        simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);
        var beforeRetarget = simulation.State;

        world.Anchor = new Vector3(-25f, 20f, 10f);
        var retargeted = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(retargeted.Winch.HasTarget);
        Assert.True(retargeted.Winch.IsPulling);
        Assert.Equal(world.Anchor, retargeted.Winch.Path.CurrentPullPoint);
        Assert.True(retargeted.Player.Velocity.X < beforeRetarget.Player.Velocity.X);
        Assert.True(retargeted.Player.Velocity.Y > beforeRetarget.Player.Velocity.Y);
    }

    [Fact]
    public void ArrivalStopsAutomaticPullAndGravityResumes()
    {
        var config = new SimulationConfig
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 12f,
                AirAcceleration = 0f,
            },
            Winch = new WinchConfig
            {
                PullInitialImpulse = 24f,
                PullRadialAcceleration = 300f,
                PullTargetInwardSpeed = 42f,
                ArrivalDistance = 0.9f,
            },
        };
        var target = new Vector3(0f, 5f, -0.5f);
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                Velocity = new Vector3(9f, 7f, -20f),
                IsGrounded = false,
            },
            Winch = new WinchState(
                WinchTargetState.Selected,
                WinchPathState.AtWorldAnchor(target),
                true,
                0.5f,
                config.Winch.PullRadialAcceleration),
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new FixedGrappleWorld(target));

        Assert.False(after.Winch.HasTarget);
        Assert.False(after.Winch.IsPulling);
        Assert.InRange(Math.Abs(after.Player.Velocity.X), 0f, 1e-5f);
        Assert.InRange(Math.Abs(after.Player.Velocity.Z), 0f, 1e-5f);

        // WinchSystem fully stops at arrival; GameSimulation then applies one gravity tick.
        Assert.True(after.Player.Velocity.Y < 0f);
        Assert.True(after.Player.Velocity.Y > -1f);
    }

    private static GameSimulation CreateAirborneSimulation(
        SimulationConfig config)
    {
        return new GameSimulation(
            config,
            SimulationState.Initial with
            {
                Player = SimulationState.Initial.Player with
                {
                    Position = new Vector3(0f, 5f, 0f),
                    IsGrounded = false,
                },
            });
    }

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
}
