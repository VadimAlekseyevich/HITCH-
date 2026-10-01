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
    public void RmbShootsFiniteCableWithoutStartingReel()
    {
        var player = PlayerState.Initial with
        {
            Position = new Vector3(2f, 3f, 4f),
        };
        var hit = new Vector3(2f, 3.65f, -26f);
        var world = new RecordingRayWorld(hit);
        var config = new WinchConfig
        {
            MaxRopeLength = 100f,
        };

        var result = WinchSystem.Step(
            player,
            WinchState.Initial,
            Input(PlayerButtons.GrappleShootPressed),
            config,
            new PlayerLocomotionConfig
            {
                EyeOffsetFromCapsuleCenter = 0.65f,
            },
            world,
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.True(result.Winch.IsAttachedFree);
        Assert.Equal(hit, result.Winch.Path.WorldAnchor);
        Assert.Equal(Vector3.UnitZ, result.Winch.Path.WorldAnchorNormal);
        Assert.InRange(result.Winch.RopeLength, 30f, 30.02f);

        Assert.Equal(
            new Vector3(2f, 3.65f, 4f),
            world.FirstRay.From);
        Assert.Equal(
            new Vector3(2f, 3.65f, -96f),
            world.FirstRay.To);
        Assert.True(world.RayCount >= 2);
    }

    [Fact]
    public void LmbStartsReelOnlyAfterCableExists()
    {
        var attached = AttachedAt(
            new Vector3(0f, 10f, -20f));

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            Input(PlayerButtons.GrappleReelPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.True(result.Winch.IsPulling);
        Assert.True(result.Winch.PullElapsedSeconds > 0f);
        Assert.True(result.Winch.RopeLength < attached.RopeLength);
    }

    [Fact]
    public void RepeatedLmbDoesNotRestartExistingReelBurst()
    {
        var attached = AttachedAt(
            new Vector3(0f, 10f, -20f)) with
        {
            IsPulling = true,
            PullElapsedSeconds = 0.4f,
        };

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            Input(PlayerButtons.GrappleReelPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.IsPulling);
        Assert.True(result.Winch.PullElapsedSeconds > 0.4f);
    }

    [Fact]
    public void RmbMissKeepsExistingCable()
    {
        var attached = AttachedAt(
            new Vector3(0f, 10f, -20f));
        var velocity = new Vector3(8f, 3f, -6f);

        var result = WinchSystem.Step(
            PlayerState.Initial with { Velocity = velocity },
            attached,
            Input(PlayerButtons.GrappleShootPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.Equal(
            attached.Path.WorldAnchor,
            result.Winch.Path.WorldAnchor);
        Assert.False(result.Winch.IsPulling);
    }

    [Fact]
    public void SpaceDetachClearsCableAndPreservesExactVelocity()
    {
        var velocity = new Vector3(120f, 35f, -210f);
        var player = PlayerState.Initial with
        {
            Velocity = velocity,
            IsGrounded = false,
        };

        var result = WinchSystem.Step(
            player,
            AttachedAt(new Vector3(0f, 50f, -80f)),
            Input(PlayerButtons.GrappleDetachPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.False(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(velocity, result.Player.Velocity);
    }

    [Fact]
    public void SecondRmbReplacesCableButDoesNotAutoReel()
    {
        var newTarget = new Vector3(20f, 15f, 0f);
        var world = new RecordingRayWorld(newTarget);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            AttachedAt(new Vector3(0f, 10f, -20f)),
            Input(PlayerButtons.GrappleShootPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            world,
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(
            newTarget,
            result.Winch.Path.WorldAnchor);
    }

    [Fact]
    public void GrappleCannotAcquireBeyondFiniteRopeRange()
    {
        var config = new WinchConfig
        {
            MaxRopeLength = 60f,
        };
        var world = new RecordingRayWorld(
            new Vector3(0f, 0f, -80f));

        var result = WinchSystem.Step(
            PlayerState.Initial,
            WinchState.Initial,
            Input(PlayerButtons.GrappleShootPressed),
            config,
            new PlayerLocomotionConfig(),
            world,
            1f / 60f);

        Assert.False(result.Winch.HasTarget);
        Assert.InRange(
            Vector3.Distance(
                world.FirstRay.From,
                world.FirstRay.To),
            59.99f,
            60.01f);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchState AttachedAt(Vector3 point)
    {
        var length = Vector3.Distance(
            Vector3.Zero,
            point);

        return new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            false,
            length,
            0f)
        {
            RopeLength = length,
        };
    }

    private sealed class RecordingRayWorld : IWorldQuery
    {
        private readonly Vector3 _hit;

        public RecordingRayWorld(Vector3 hit)
        {
            _hit = hit;
        }

        public RayQuery FirstRay { get; private set; }

        public RayQuery LastRay { get; private set; }

        public int RayCount { get; private set; }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            if (RayCount == 0)
            {
                FirstRay = query;
            }

            LastRay = query;
            RayCount++;

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
