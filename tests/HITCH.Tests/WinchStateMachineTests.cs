using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchStateMachineTests
{
    [Fact]
    public void RmbRaycastsAndStartsPullInSameTick()
    {
        var player = PlayerState.Initial with
        {
            Position = new Vector3(2f, 3f, 4f),
        };
        var config = new WinchConfig
        {
            GrappleRange = 72f,
            PullInitialImpulse = 30f,
            PullRadialAcceleration = 420f,
            PullTargetInwardSpeed = 55f,
        };
        var locomotion = new PlayerLocomotionConfig
        {
            EyeOffsetFromCapsuleCenter = 0.65f,
        };
        var hit = new Vector3(2f, 3.65f, -26f);
        var world = new RecordingRayWorld(hit);

        var result = WinchSystem.Step(
            player,
            WinchState.Initial,
            Input(PlayerButtons.GrapplePullPressed),
            config,
            locomotion,
            world,
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.True(result.Winch.IsPulling);
        Assert.Equal(hit, result.Winch.Path.CurrentPullPoint);
        Assert.True(result.Player.Velocity.Z < -29f);

        Assert.Equal(new Vector3(2f, 3.65f, 4f), world.LastRay.From);
        Assert.Equal(new Vector3(2f, 3.65f, -68f), world.LastRay.To);
    }

    [Fact]
    public void RmbMissClearsExistingCableAndPreservesMomentum()
    {
        var velocity = new Vector3(8f, 3f, -6f);

        var result = WinchSystem.Step(
            PlayerState.Initial with { Velocity = velocity },
            ActiveAt(new Vector3(0f, 10f, -20f)),
            Input(PlayerButtons.GrapplePullPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.False(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(velocity, result.Player.Velocity);
    }

    [Fact]
    public void SecondRmbReplacesCableAndImmediatelyPullsTowardNewPoint()
    {
        var velocity = new Vector3(11f, 4f, -8f);
        var newTarget = new Vector3(20f, 15f, 0f);
        var world = new RecordingRayWorld(newTarget);

        var result = WinchSystem.Step(
            PlayerState.Initial with { Velocity = velocity },
            ActiveAt(new Vector3(0f, 10f, -20f)),
            Input(PlayerButtons.GrapplePullPressed),
            new WinchConfig
            {
                PullInitialImpulse = 30f,
                PullRadialAcceleration = 420f,
            PullTargetInwardSpeed = 55f,
            },
            new PlayerLocomotionConfig(),
            world,
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.True(result.Winch.IsPulling);
        Assert.Equal(newTarget, result.Winch.Path.CurrentPullPoint);
        Assert.True(result.Player.Velocity.X > velocity.X);
        Assert.True(result.Player.Velocity.Y > velocity.Y);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchState ActiveAt(Vector3 point) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
            Vector3.Distance(Vector3.Zero, point),
            420f);

    private sealed class RecordingRayWorld : IWorldQuery
    {
        private readonly Vector3 _hit;

        public RecordingRayWorld(Vector3 hit)
        {
            _hit = hit;
        }

        public RayQuery LastRay { get; private set; }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            LastRay = query;
            hit = new WorldHit(_hit, Vector3.UnitZ, 0.5f, 1u);
            return true;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
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

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }
}
