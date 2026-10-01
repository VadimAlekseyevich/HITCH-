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
    public void LeftThenRightClicksCreateTwoIndependentActiveCables()
    {
        var config = TestSimulationConfig();
        var world = new MutableGrappleWorld(
            new Vector3(-20f, 12f, -30f));
        var simulation = CreateAirborneSimulation(config);

        var left = simulation.Step(
            Input(PlayerButtons.LeftGrapplePressed),
            world);

        Assert.True(left.Winch.Left.IsPulling);
        Assert.False(left.Winch.Right.IsPulling);

        var leftPoint = left.Winch.Left.Path.CurrentPullPoint;

        world.Anchor = new Vector3(20f, 16f, -30f);
        var dual = simulation.Step(
            Input(PlayerButtons.RightGrapplePressed),
            world);

        Assert.Equal(2, dual.Winch.ActiveCableCount);
        Assert.Equal(
            leftPoint,
            dual.Winch.Left.Path.CurrentPullPoint);
        Assert.Equal(
            world.Anchor,
            dual.Winch.Right.Path.CurrentPullPoint);
    }

    [Fact]
    public void DualCablesContinuePullingWithoutFurtherMouseInput()
    {
        var config = TestSimulationConfig();
        var world = new MutableGrappleWorld(
            new Vector3(-20f, 10f, -40f));
        var simulation = CreateAirborneSimulation(config);

        simulation.Step(
            Input(PlayerButtons.LeftGrapplePressed),
            world);

        world.Anchor = new Vector3(20f, 10f, -40f);
        simulation.Step(
            Input(PlayerButtons.RightGrapplePressed),
            world);

        var before = simulation.State;
        var leftBefore = before.Winch.Left.LastActualDistance;
        var rightBefore = before.Winch.Right.LastActualDistance;

        for (var i = 0; i < 6; i++)
        {
            simulation.Step(PlayerInput.Neutral, world);
        }

        var after = simulation.State;

        Assert.Equal(2, after.Winch.ActiveCableCount);
        Assert.True(
            after.Winch.Left.LastActualDistance < leftBefore);
        Assert.True(
            after.Winch.Right.LastActualDistance < rightBefore);
    }

    [Fact]
    public void OneArrivedCableClearsButOtherCableKeepsMovingPlayer()
    {
        var config = TestSimulationConfig() with
        {
            Winch = TestWinchConfig() with
            {
                ArrivalDistance = 0.9f,
            },
        };

        var leftPoint = new Vector3(0f, 5f, -0.5f);
        var rightPoint = new Vector3(0f, 5f, -30f);
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                Velocity = new Vector3(7f, 4f, 0f),
                IsGrounded = false,
            },
            Winch = new WinchState(
                ActiveCable(leftPoint),
                ActiveCable(rightPoint)),
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        Assert.False(after.Winch.Left.HasTarget);
        Assert.True(after.Winch.Right.IsPulling);
        Assert.NotEqual(Vector3.Zero, after.Player.Velocity);
        Assert.True(after.Player.Velocity.Z < 0f);
    }

    [Fact]
    public void LastArrivedCableFullyStopsThenGravityResumes()
    {
        var config = TestSimulationConfig() with
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 12f,
                AirAcceleration = 0f,
            },
            Winch = TestWinchConfig() with
            {
                ArrivalDistance = 0.9f,
            },
        };

        var point = new Vector3(0f, 5f, -0.5f);
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                Velocity = new Vector3(9f, 7f, -20f),
                IsGrounded = false,
            },
            Winch = new WinchState(
                ActiveCable(point),
                WinchCableState.Initial),
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new NoHitWorld());

        Assert.Equal(0, after.Winch.ActiveCableCount);
        Assert.InRange(
            Math.Abs(after.Player.Velocity.X),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(after.Player.Velocity.Z),
            0f,
            1e-5f);

        // Winch stops completely, then locomotion applies one gravity tick.
        Assert.True(after.Player.Velocity.Y < 0f);
        Assert.True(after.Player.Velocity.Y > -1f);
    }

    private static SimulationConfig TestSimulationConfig() =>
        new()
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 1f,
                AirAcceleration = 0f,
            },
            Winch = TestWinchConfig(),
        };

    private static WinchConfig TestWinchConfig() =>
        new()
        {
            PullInitialImpulse = 24f,
            PullRadialAcceleration = 300f,
            PullTargetInwardSpeed = 42f,
            ArrivalDistance = 0.5f,
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

    private static WinchCableState ActiveCable(
        Vector3 point) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
            Vector3.Distance(
                new Vector3(0f, 5f, 0f),
                point),
            0f);

    private sealed class MutableGrappleWorld : IWorldQuery
    {
        public MutableGrappleWorld(Vector3 anchor)
        {
            Anchor = anchor;
        }

        public Vector3 Anchor { get; set; }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
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
