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
    public void LmbShootsOnlyLeftCableAndStartsPull()
    {
        var player = PlayerState.Initial with
        {
            Position = new Vector3(2f, 3f, 4f),
        };
        var hit = new Vector3(2f, 3.65f, -26f);
        var world = new RecordingRayWorld(hit);

        var result = WinchSystem.Step(
            player,
            WinchState.Initial,
            Input(PlayerButtons.LeftGrapplePressed),
            new WinchConfig(),
            new PlayerLocomotionConfig
            {
                EyeOffsetFromCapsuleCenter = 0.65f,
            },
            world,
            1f / 60f);

        Assert.True(result.Winch.Left.IsPulling);
        Assert.False(result.Winch.Right.IsPulling);
        Assert.Equal(hit, result.Winch.Left.Path.CurrentPullPoint);

        Assert.Equal(
            new Vector3(2f, 3.65f, 4f),
            world.LastRay.From);

        // 10 km is an engine-query distance, not gameplay rope range.
        Assert.Equal(
            new Vector3(2f, 3.65f, -9996f),
            world.LastRay.To);
    }

    [Fact]
    public void RmbShootsOnlyRightCableAndStartsPull()
    {
        var hit = new Vector3(15f, 8f, -20f);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            WinchState.Initial,
            Input(PlayerButtons.RightGrapplePressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new RecordingRayWorld(hit),
            1f / 60f);

        Assert.False(result.Winch.Left.IsPulling);
        Assert.True(result.Winch.Right.IsPulling);
        Assert.Equal(hit, result.Winch.Right.Path.CurrentPullPoint);
    }

    [Fact]
    public void RetargetingLeftDoesNotReplaceRightCable()
    {
        var rightPoint = new Vector3(20f, 10f, -20f);
        var newLeftPoint = new Vector3(-15f, 12f, -25f);
        var initial = new WinchState(
            WinchCableState.Initial,
            ActiveCable(rightPoint));

        var result = WinchSystem.Step(
            PlayerState.Initial,
            initial,
            Input(PlayerButtons.LeftGrapplePressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new RecordingRayWorld(newLeftPoint),
            1f / 60f);

        Assert.Equal(
            newLeftPoint,
            result.Winch.Left.Path.CurrentPullPoint);
        Assert.Equal(
            rightPoint,
            result.Winch.Right.Path.CurrentPullPoint);
        Assert.True(result.Winch.Left.IsPulling);
        Assert.True(result.Winch.Right.IsPulling);
    }

    [Fact]
    public void LeftMissClearsOnlyLeftCable()
    {
        var velocity = new Vector3(8f, 3f, -6f);
        var rightPoint = new Vector3(20f, 10f, -20f);
        var initial = new WinchState(
            ActiveCable(new Vector3(-20f, 10f, -20f)),
            ActiveCable(rightPoint));

        var result = WinchSystem.Step(
            PlayerState.Initial with { Velocity = velocity },
            initial,
            Input(PlayerButtons.LeftGrapplePressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.False(result.Winch.Left.HasTarget);
        Assert.True(result.Winch.Right.HasTarget);
        Assert.Equal(
            rightPoint,
            result.Winch.Right.Path.CurrentPullPoint);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchCableState ActiveCable(Vector3 point) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
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
            hit = new WorldHit(
                _hit,
                Vector3.UnitZ,
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
