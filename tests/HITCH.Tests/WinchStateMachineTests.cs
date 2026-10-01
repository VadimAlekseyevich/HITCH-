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
    public void GrapplePressRaycastsFromEyeAlongSimulationViewAndAttaches()
    {
        var player = PlayerState.Initial with
        {
            Position = new Vector3(2f, 3f, 4f),
            ViewYawRadians = 0f,
            ViewPitchRadians = 0f,
        };
        var config = new WinchConfig
        {
            GrappleRange = 20f,
        };
        var locomotion = new PlayerLocomotionConfig
        {
            EyeOffsetFromCapsuleCenter = 0.65f,
        };
        var expectedHit = new Vector3(2f, 3.65f, -6f);
        var world = new RecordingRayWorld(expectedHit);

        var result = WinchSystem.Step(
            player,
            WinchState.Initial,
            ButtonInput(PlayerButtons.GrapplePressed),
            config,
            locomotion,
            world,
            1f / 60f);

        Assert.True(result.Winch.IsAttached);
        Assert.Equal(expectedHit, result.Winch.Path.WorldAnchor);
        Assert.Equal(expectedHit, result.Winch.Path.CurrentPullPoint);

        Assert.Equal(new Vector3(2f, 3.65f, 4f), world.LastRay.From);
        Assert.Equal(new Vector3(2f, 3.65f, -16f), world.LastRay.To);
        Assert.Equal(config.GrappleCollisionMask, world.LastRay.CollisionMask);
    }

    [Fact]
    public void GrappleReleaseDetachesWithoutChangingPlayerVelocity()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(12f, 5f, -7f),
        };
        var config = new WinchConfig();
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 10f);

        var result = WinchSystem.Step(
            player,
            attached,
            ButtonInput(PlayerButtons.GrappleReleased),
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.False(result.Winch.IsAttached);
        Assert.Equal(player.Velocity, result.Player.Velocity);
        Assert.Equal(config.ReattachCooldownSeconds, result.Winch.ReattachCooldownRemaining);
    }

    [Fact]
    public void ReattachIsBlockedUntilCooldownExpires()
    {
        var config = new WinchConfig
        {
            ReattachCooldownSeconds = 0.1f,
        };
        var locomotion = new PlayerLocomotionConfig();
        var player = PlayerState.Initial;
        var world = new RecordingRayWorld(new Vector3(0f, 0f, -5f));

        var detached = WinchSystem.Step(
            player,
            AttachedAt(new Vector3(0f, 0f, -5f), 5f),
            ButtonInput(PlayerButtons.GrappleReleased),
            config,
            locomotion,
            world,
            0.02f).Winch;

        var blocked = WinchSystem.Step(
            player,
            detached,
            ButtonInput(PlayerButtons.GrapplePressed),
            config,
            locomotion,
            world,
            0.02f).Winch;

        Assert.False(blocked.IsAttached);
        Assert.Equal(0, world.RaycastCount);

        var cooled = blocked;
        for (var i = 0; i < 4; i++)
        {
            cooled = WinchSystem.Step(
                player,
                cooled,
                PlayerInput.Neutral,
                config,
                locomotion,
                world,
                0.02f).Winch;
        }

        var attached = WinchSystem.Step(
            player,
            cooled,
            ButtonInput(PlayerButtons.GrapplePressed),
            config,
            locomotion,
            world,
            0.02f).Winch;

        Assert.True(attached.IsAttached);
        Assert.Equal(1, world.RaycastCount);
    }

    [Fact]
    public void ReleaseWinsWhenPressAndReleaseArriveSameTick()
    {
        var world = new RecordingRayWorld(new Vector3(0f, 0f, -5f));
        var input = new PlayerInput(
            Vector2.Zero,
            Vector2.Zero,
            0f,
            PlayerButtons.GrapplePressed | PlayerButtons.GrappleReleased);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            WinchState.Initial,
            input,
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            world,
            1f / 60f);

        Assert.False(result.Winch.IsAttached);
        Assert.Equal(0, world.RaycastCount);
    }

    private static PlayerInput ButtonInput(PlayerButtons button) =>
        new(Vector2.Zero, Vector2.Zero, 0f, button);

    private static WinchState AttachedAt(Vector3 anchor, float restLength) =>
        new(
            WinchAttachmentState.Attached,
            WinchPathState.AtWorldAnchor(anchor),
            restLength,
            0f,
            0f,
            Vector3.Distance(Vector3.Zero, anchor),
            0f);

    private sealed class RecordingRayWorld : IWorldQuery
    {
        private readonly Vector3 _hit;

        public RecordingRayWorld(Vector3 hit)
        {
            _hit = hit;
        }

        public int RaycastCount { get; private set; }

        public RayQuery LastRay { get; private set; }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            RaycastCount++;
            LastRay = query;
            hit = new WorldHit(
                _hit,
                Vector3.UnitZ,
                0.5f,
                1u);
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
