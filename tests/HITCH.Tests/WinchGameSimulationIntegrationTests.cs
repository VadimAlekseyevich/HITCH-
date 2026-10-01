using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchGameSimulationIntegrationTests
{
    [Fact]
    public void SelectThenPullMovesImmediatelyTowardPoint()
    {
        var config = new SimulationConfig
        {
            Winch = new Hitch.Simulation.Winch.WinchConfig
            {
                PullSpeed = 12f,
                ArrivalDistance = 0.5f,
            },
        };
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
            },
        };
        var world = new FixedGrappleWorld(new Vector3(0f, 5f, -20f));
        var simulation = new GameSimulation(config, initial);

        var selected = simulation.Step(
            Input(PlayerButtons.SelectGrapplePointPressed),
            world);

        Assert.True(selected.Winch.HasTarget);
        Assert.False(selected.Winch.IsPulling);

        var pulling = simulation.Step(
            Input(PlayerButtons.PullPressed),
            world);

        Assert.True(pulling.Winch.IsPulling);
        Assert.InRange(
            Math.Abs(pulling.Player.Velocity.Length() - 12f),
            0f,
            1e-5f);

        var towardTarget = Vector3.Normalize(
            selected.Winch.Path.CurrentPullPoint - selected.Player.Position);
        var actualDirection = Vector3.Normalize(pulling.Player.Velocity);

        Assert.True(Vector3.Dot(towardTarget, actualDirection) > 0.9999f);
        Assert.True(pulling.Player.Position.Z < selected.Player.Position.Z);
    }

    [Fact]
    public void ReleasingPullPreservesHorizontalMomentumAndGravityResumes()
    {
        var config = new SimulationConfig
        {
            Winch = new Hitch.Simulation.Winch.WinchConfig
            {
                PullSpeed = 12f,
                ArrivalDistance = 0.5f,
            },
        };
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
            },
        };
        var world = new FixedGrappleWorld(new Vector3(12f, 5f, 0f));
        var simulation = new GameSimulation(config, initial);

        simulation.Step(Input(PlayerButtons.SelectGrapplePointPressed), world);
        simulation.Step(Input(PlayerButtons.PullPressed), world);
        var beforeRelease = simulation.State;

        var afterRelease = simulation.Step(
            Input(PlayerButtons.PullReleased),
            world);

        Assert.True(afterRelease.Winch.HasTarget);
        Assert.False(afterRelease.Winch.IsPulling);
        Assert.InRange(
            Math.Abs(afterRelease.Player.Velocity.X - beforeRelease.Player.Velocity.X),
            0f,
            1e-5f);
        Assert.True(afterRelease.Player.Velocity.Y < beforeRelease.Player.Velocity.Y);
    }

    [Fact]
    public void RetargetWhilePullingChangesDirectionInSameTick()
    {
        var config = new SimulationConfig
        {
            Winch = new Hitch.Simulation.Winch.WinchConfig
            {
                PullSpeed = 10f,
                ArrivalDistance = 0.5f,
            },
        };
        var world = new MutableGrappleWorld(new Vector3(0f, 5f, -20f));
        var simulation = new GameSimulation(
            config,
            SimulationState.Initial with
            {
                Player = SimulationState.Initial.Player with
                {
                    Position = new Vector3(0f, 5f, 0f),
                },
            });

        simulation.Step(Input(PlayerButtons.SelectGrapplePointPressed), world);
        simulation.Step(Input(PlayerButtons.PullPressed), world);

        world.Anchor = new Vector3(20f, 5f, 0f);
        var retargeted = simulation.Step(
            Input(PlayerButtons.SelectGrapplePointPressed),
            world);

        Assert.True(retargeted.Winch.IsPulling);
        Assert.True(retargeted.Player.Velocity.X > 9f);
        Assert.True(Math.Abs(retargeted.Player.Velocity.Z) < 2f);
    }


    [Fact]
    public void ReachingTargetConsumesPointAndGravityImmediatelyResumes()
    {
        var config = new SimulationConfig
        {
            Winch = new Hitch.Simulation.Winch.WinchConfig
            {
                PullSpeed = 12f,
                ArrivalDistance = 0.75f,
            },
        };
        var player = SimulationState.Initial.Player with
        {
            Position = new Vector3(0f, 5f, 0f),
            Velocity = new Vector3(50f, 20f, -10f),
            IsGrounded = false,
        };
        var target = new Vector3(0f, 5f, -0.5f);
        var initial = SimulationState.Initial with
        {
            Player = player,
            Winch = new Hitch.Simulation.Winch.WinchState(
                Hitch.Simulation.Winch.WinchTargetState.Selected,
                Hitch.Simulation.Winch.WinchPathState.AtWorldAnchor(target),
                true,
                0.5f,
                12f),
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new FixedGrappleWorld(target));

        Assert.False(after.Winch.HasTarget);
        Assert.False(after.Winch.IsPulling);
        Assert.Equal(0f, after.Player.Velocity.X);
        Assert.Equal(0f, after.Player.Velocity.Z);
        Assert.True(after.Player.Velocity.Y < 0f);
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
            hit = new WorldHit(CurrentAnchor, Vector3.UnitY, 0.5f, 1u);
            return true;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
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
