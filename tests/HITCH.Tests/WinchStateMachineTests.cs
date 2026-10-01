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
    public void LeftClickSelectsPointWithoutStartingPull()
    {
        var player = PlayerState.Initial with
        {
            Position = new Vector3(2f, 3f, 4f),
        };
        var config = new WinchConfig
        {
            GrappleRange = 20f,
        };
        var locomotion = new PlayerLocomotionConfig
        {
            EyeOffsetFromCapsuleCenter = 0.65f,
        };
        var hit = new Vector3(2f, 3.65f, -6f);
        var world = new RecordingRayWorld(hit);

        var result = WinchSystem.Step(
            player,
            WinchState.Initial,
            Input(PlayerButtons.SelectGrapplePointPressed),
            config,
            locomotion,
            world,
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(hit, result.Winch.Path.CurrentPullPoint);
        Assert.Equal(player.Velocity, result.Player.Velocity);

        Assert.Equal(new Vector3(2f, 3.65f, 4f), world.LastRay.From);
        Assert.Equal(new Vector3(2f, 3.65f, -16f), world.LastRay.To);
    }

    [Fact]
    public void PullPressWithoutTargetDoesNothing()
    {
        var result = WinchSystem.Step(
            PlayerState.Initial,
            WinchState.Initial,
            Input(PlayerButtons.PullPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.False(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(Vector3.Zero, result.Player.Velocity);
    }

    [Fact]
    public void PullReleaseStopsForceButKeepsSelectedTargetAndVelocity()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(12f, 5f, -7f),
        };
        var state = SelectedAt(
            new Vector3(0f, 0f, -20f),
            pulling: true);

        var result = WinchSystem.Step(
            player,
            state,
            Input(PlayerButtons.PullReleased),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(player.Velocity, result.Player.Velocity);
    }

    [Fact]
    public void LeftClickWhilePullingReplacesTargetImmediately()
    {
        var oldTarget = new Vector3(0f, 0f, -20f);
        var newTarget = new Vector3(20f, 0f, 0f);
        var config = new WinchConfig { PullSpeed = 10f };
        var world = new RecordingRayWorld(newTarget);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            SelectedAt(oldTarget, pulling: true),
            Input(PlayerButtons.SelectGrapplePointPressed),
            config,
            new PlayerLocomotionConfig(),
            world,
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.True(result.Winch.IsPulling);
        Assert.Equal(newTarget, result.Winch.Path.CurrentPullPoint);
        Assert.Equal(new Vector3(10f, 0f, 0f), result.Player.Velocity);
    }

    [Fact]
    public void MissedSelectionKeepsExistingTarget()
    {
        var target = new Vector3(0f, 0f, -10f);
        var state = SelectedAt(target, pulling: false);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            state,
            Input(PlayerButtons.SelectGrapplePointPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.Equal(target, result.Winch.Path.CurrentPullPoint);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchState SelectedAt(Vector3 point, bool pulling) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            pulling,
            Vector3.Distance(Vector3.Zero, point),
            0f);

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
