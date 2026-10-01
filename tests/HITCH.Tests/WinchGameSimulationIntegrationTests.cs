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

        Assert.True(after.Winch.HasTarget);
        Assert.False(after.Winch.IsPulling);
        Assert.True(after.Winch.IsLatched);

        // Full reel-in clears orbital/tangential motion.
        Assert.InRange(
            Math.Abs(after.Player.Velocity.X),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(after.Player.Velocity.Z),
            0f,
            1e-5f);

        // Completion itself is now a hard settle: locomotion/gravity resumes next tick.
        Assert.InRange(
            Math.Abs(after.Player.Velocity.Y),
            0f,
            1e-5f);
    }

    [Fact]
    public void HighSpeedArrivalDuringMovementSettlesInSameTick()
    {
        var config = TestSimulationConfig() with
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 1f,
                AirAcceleration = 0f,
            },
        };

        var playerPosition = new Vector3(0f, 5f, 0f);
        var target = playerPosition + new Vector3(0f, 0f, -1.15f);

        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = playerPosition,
                Velocity = new Vector3(8f, 0f, -42f),
                IsGrounded = false,
            },
            Winch = new WinchState(
                WinchTargetState.Selected,
                WinchPathState.AtWorldAnchor(target),
                true,
                1.15f,
                config.Winch.PullRadialAcceleration),
        };

        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        Assert.True(after.Winch.HasTarget);
        Assert.False(after.Winch.IsPulling);
        Assert.True(after.Winch.IsLatched);
        Assert.Equal(Vector3.Zero, after.Player.Velocity);
    }

    [Fact]
    public void ActiveGrappleAppliesGravityAndCreatesSwingTangent()
    {
        var config = TestSimulationConfig() with
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 18f,
                AirAcceleration = 0f,
            },
        };
        var target = new Vector3(0f, 30f, -40f);
        var world = new FixedGrappleWorld(target);
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 10f, 0f),
                Velocity = new Vector3(12f, 0f, 0f),
                IsGrounded = false,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(after.Winch.IsPulling);
        Assert.True(after.Player.Velocity.X > 10f);

        var toAnchor = Vector3.Normalize(
            target - after.Player.Position);
        var radial =
            Vector3.Dot(after.Player.Velocity, toAnchor);
        var tangent =
            after.Player.Velocity - (toAnchor * radial);

        Assert.True(radial > 0f);
        Assert.True(tangent.Length() > 1f);
    }

    [Fact]
    public void CompletedGrappleRemainsStoppedUntilRmbReleasesOrRetargets()
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

        var simulation = new GameSimulation(config, initial);

        var settled = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        Assert.True(settled.Winch.IsLatched);
        Assert.Equal(Vector3.Zero, settled.Player.Velocity);

        for (var i = 0; i < 10; i++)
        {
            var held = simulation.Step(
                PlayerInput.Neutral,
                new NoHitWorld());

            Assert.True(held.Winch.IsLatched);
            Assert.Equal(settled.Player.Position, held.Player.Position);
            Assert.Equal(Vector3.Zero, held.Player.Velocity);
        }

        var released = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            new NoHitWorld());

        Assert.False(released.Winch.HasTarget);
        Assert.True(released.Player.Velocity.Y < 0f);
    }

    [Fact]
    public void SpaceDetachPreservesFlightMomentumWithoutJumping()
    {
        var config = TestSimulationConfig();
        var world = new MutableGrappleWorld(
            new Vector3(0f, 5f, -200f));
        var simulation = CreateAirborneSimulation(config);

        simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        var beforeDetach =
            simulation.State.Player.Velocity;

        var detached = simulation.Step(
            Input(
                PlayerButtons.JumpPressed
                | PlayerButtons.GrappleDetachPressed),
            new NoHitWorld());

        Assert.False(detached.Winch.HasTarget);
        Assert.False(detached.Winch.IsPulling);

        // Horizontal flight momentum survives the detach exactly.
        Assert.InRange(
            Math.Abs(detached.Player.Velocity.X - beforeDetach.X),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(detached.Player.Velocity.Z - beforeDetach.Z),
            0f,
            1e-5f);

        // Space was consumed as detach, not as an upward jump.
        Assert.True(detached.Player.Velocity.Y <= 0f);
        Assert.True(detached.Player.Velocity.Z < -40f);
    }

    [Fact]
    public void RetargetRestartsLaunchBurst()
    {
        var config = TestSimulationConfig() with
        {
            Winch = new WinchConfig
            {
                PullTargetInwardSpeed = 100f,
                PullLongRangeInwardSpeed = 100f,
                PullLongRangeDistance = 250f,
                PullLaunchInitialMultiplier = 1.75f,
                PullLaunchPeakMultiplier = 2.75f,
                PullLaunchPeakSeconds = 0.10f,
                PullLaunchDecaySeconds = 0.70f,
                ArrivalContactTolerance = 0.06f,
            },
        };
        var world = new MutableGrappleWorld(
            new Vector3(0f, 5f, -300f));
        var simulation = CreateAirborneSimulation(config);

        var launched = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        for (var i = 0; i < 50; i++)
        {
            simulation.Step(PlayerInput.Neutral, world);
        }

        var beforeRetargetSpeed =
            simulation.State.Player.Velocity.Length();

        world.Anchor = new Vector3(300f, 5f, 0f);
        var retargeted = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(retargeted.Winch.IsPulling);
        Assert.True(
            retargeted.Player.Velocity.Length()
            > beforeRetargetSpeed);
        Assert.True(
            retargeted.Player.Velocity.Length()
            > 140f);
        Assert.True(
            launched.Player.Velocity.Length()
            > 140f);
    }

    [Fact]
    public void RetargetReplacesRadialMotionButKeepsSwingTangent()
    {
        var config = TestSimulationConfig();
        var world = new MutableGrappleWorld(
            new Vector3(20f, 8f, -20f));
        var simulation = CreateAirborneSimulation(config);

        simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        world.Anchor = new Vector3(-25f, 20f, 10f);
        var retargeted = simulation.Step(
            Input(PlayerButtons.GrapplePullPressed),
            world);

        Assert.True(retargeted.Winch.IsPulling);
        Assert.Equal(
            world.Anchor,
            retargeted.Winch.Path.CurrentPullPoint);

        var direction = Vector3.Normalize(
            world.Anchor - retargeted.Player.Position);
        var inwardSpeed =
            Vector3.Dot(retargeted.Player.Velocity, direction);
        var tangent =
            retargeted.Player.Velocity
            - (direction * inwardSpeed);

        Assert.True(inwardSpeed > 35f);
        Assert.True(tangent.Length() > 1f);
        Assert.True(retargeted.Player.Velocity.Y < 10f);
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
                PullLongRangeInwardSpeed = 42f,
                PullLongRangeDistance = 250f,
                PullLaunchInitialMultiplier = 1f,
                PullLaunchPeakMultiplier = 1f,
                PullLaunchPeakSeconds = 0.10f,
                PullLaunchDecaySeconds = 0.70f,
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
