using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class DualWinchGameSimulationTests
{
    [Fact]
    public void FirstAndSecondRmbAttachDifferentCableSlots()
    {
        var config = TestConfig();
        var world = new MutableAnchorWorld(
            new Vector3(-20f, 20f, -30f));
        var simulation = CreateAirborne(config);

        var first = simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);

        Assert.True(first.Winch.HasTarget);
        Assert.False(first.SecondaryWinch.HasTarget);
        Assert.Equal((byte)1, first.NextGrappleSlot);

        var leftAnchor = first.Winch.Path.WorldAnchor;
        world.Anchor = new Vector3(25f, 24f, -35f);

        var second = simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);

        Assert.True(second.Winch.HasTarget);
        Assert.True(second.SecondaryWinch.HasTarget);
        Assert.Equal(leftAnchor, second.Winch.Path.WorldAnchor);
        Assert.Equal(world.Anchor, second.SecondaryWinch.Path.WorldAnchor);
        Assert.Equal((byte)0, second.NextGrappleSlot);
    }

    [Fact]
    public void ThirdRmbRetargetsLeftAndKeepsRight()
    {
        var config = TestConfig();
        var world = new MutableAnchorWorld(
            new Vector3(-20f, 20f, -30f));
        var simulation = CreateAirborne(config);

        simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);

        world.Anchor = new Vector3(20f, 22f, -32f);
        var second = simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);
        var rightAnchor =
            second.SecondaryWinch.Path.WorldAnchor;

        world.Anchor = new Vector3(-35f, 30f, -45f);
        var third = simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);

        Assert.Equal(world.Anchor, third.Winch.Path.WorldAnchor);
        Assert.Equal(rightAnchor, third.SecondaryWinch.Path.WorldAnchor);
        Assert.Equal((byte)1, third.NextGrappleSlot);
    }

    [Fact]
    public void LmbStartsReelOnBothAttachedCables()
    {
        var config = TestConfig();
        var world = new MutableAnchorWorld(
            new Vector3(-20f, 18f, -30f));
        var simulation = CreateAirborne(config);

        simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);
        world.Anchor = new Vector3(20f, 18f, -30f);
        var attached = simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);

        var leftLength = attached.Winch.RopeLength;
        var rightLength = attached.SecondaryWinch.RopeLength;

        var reeling = simulation.Step(
            Input(PlayerButtons.GrappleReelPressed),
            world);

        Assert.True(reeling.Winch.IsPulling);
        Assert.True(reeling.SecondaryWinch.IsPulling);
        Assert.True(reeling.Winch.RopeLength < leftLength);
        Assert.True(reeling.SecondaryWinch.RopeLength < rightLength);
    }

    [Fact]
    public void SpaceDetachesBothCables()
    {
        var config = TestConfig();
        var world = new MutableAnchorWorld(
            new Vector3(-20f, 18f, -30f));
        var simulation = CreateAirborne(config);

        simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);
        world.Anchor = new Vector3(20f, 18f, -30f);
        simulation.Step(
            Input(PlayerButtons.GrappleShootPressed),
            world);

        var detached = simulation.Step(
            Input(PlayerButtons.GrappleDetachPressed),
            world);

        Assert.False(detached.Winch.HasTarget);
        Assert.False(detached.SecondaryWinch.HasTarget);
    }

    [Fact]
    public void TwoIdleCablesApplyGravityOnlyOnce()
    {
        var config = TestConfig() with
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 12f,
                AirAcceleration = 0f,
            },
            Winch = TestConfig().Winch with
            {
                GasAcceleration = 0f,
            },
        };
        var position = new Vector3(0f, 10f, 0f);
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = position,
                IsGrounded = false,
            },
            Winch = AttachedAt(
                position,
                new Vector3(-20f, 10f, -20f)),
            SecondaryWinch = AttachedAt(
                position,
                new Vector3(20f, 10f, -20f)),
        };
        var simulation = new GameSimulation(
            config,
            initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        var expectedY =
            -config.Locomotion.Gravity / config.TickRateHz;

        Assert.InRange(
            Math.Abs(after.Player.Velocity.Y - expectedY),
            0f,
            1e-5f);
    }

    private static SimulationConfig TestConfig() =>
        new()
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 1f,
                AirAcceleration = 0f,
            },
            Winch = new WinchConfig
            {
                MaxRopeLength = 150f,
                PullRadialAcceleration = 110f,
                PullTargetInwardSpeed = 20f,
                PullLongRangeInwardSpeed = 32f,
                PullLongRangeDistance = 110f,
                PullLaunchInitialMultiplier = 1.25f,
                PullLaunchPeakMultiplier = 1.65f,
                PullLaunchPeakSeconds = 0.11f,
                PullLaunchDecaySeconds = 0.48f,
                GasAcceleration = 0f,
                DualCableMotorScale = 0.72f,
            },
        };

    private static GameSimulation CreateAirborne(
        SimulationConfig config) =>
        new(
            config,
            SimulationState.Initial with
            {
                Player = SimulationState.Initial.Player with
                {
                    Position = new Vector3(0f, 8f, 0f),
                    IsGrounded = false,
                },
            });

    private static WinchState AttachedAt(
        Vector3 playerPosition,
        Vector3 anchor)
    {
        var length = Vector3.Distance(
            playerPosition,
            anchor);

        return new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(anchor),
            false,
            length,
            0f)
        {
            RopeLength = length,
        };
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(
            Vector2.Zero,
            Vector2.Zero,
            0f,
            buttons);

    private sealed class MutableAnchorWorld : IWorldQuery
    {
        public MutableAnchorWorld(Vector3 anchor)
        {
            Anchor = anchor;
        }

        public Vector3 Anchor { get; set; }

        public bool TryRaycast(
            in RayQuery query,
            out WorldHit hit)
        {
            hit = new WorldHit(
                Anchor,
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

    private sealed class NoHitWorld : IWorldQuery
    {
        public bool TryRaycast(
            in RayQuery query,
            out WorldHit hit)
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
