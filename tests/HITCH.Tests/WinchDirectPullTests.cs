using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchDirectPullTests
{
    [Fact]
    public void RmbShotImmediatelyEstablishesStrongInwardSpeed()
    {
        var config = new WinchConfig
        {
            PullInitialImpulse = 30f,
            PullRadialAcceleration = 420f,
            PullTargetInwardSpeed = 55f,
            ArrivalDistance = 0.5f,
        };

        var result = WinchSystem.Step(
            PlayerState.Initial,
            WinchState.Initial,
            Input(PlayerButtons.GrapplePullPressed),
            config,
            new PlayerLocomotionConfig(),
            new FixedHitWorld(new Vector3(0f, 0f, -20f)),
            0.1f);

        Assert.True(result.Winch.IsPulling);
        AssertVectorClose(
            new Vector3(0f, 0f, -55f),
            result.Player.Velocity);
        Assert.True(result.Winch.LastPullAcceleration > 0f);
    }

    [Fact]
    public void PullPreservesTangentialSwingWhileForcingInwardRadialSpeed()
    {
        var config = new WinchConfig
        {
            PullInitialImpulse = 30f,
            PullRadialAcceleration = 420f,
            PullTargetInwardSpeed = 55f,
            ArrivalDistance = 0.5f,
        };
        var player = PlayerState.Initial with
        {
            // X is tangential to a target straight down -Z.
            Velocity = new Vector3(17f, 0f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 0f, -20f), 0f),
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.2f);

        Assert.True(result.Winch.IsPulling);
        Assert.InRange(Math.Abs(result.Player.Velocity.X - 17f), 0f, 1e-5f);
        Assert.InRange(Math.Abs(result.Player.Velocity.Z + 55f), 0f, 1e-5f);
    }

    [Fact]
    public void PullRapidlyReversesVelocityMovingAwayFromAnchor()
    {
        var config = new WinchConfig
        {
            PullRadialAcceleration = 420f,
            PullTargetInwardSpeed = 55f,
            ArrivalDistance = 0.5f,
        };
        var player = PlayerState.Initial with
        {
            // Target is -Z, so +Z is moving directly away from the anchor.
            Velocity = new Vector3(0f, 0f, 30f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 0f, -20f), 0f),
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.True(result.Player.Velocity.Z < 0f);
        Assert.InRange(Math.Abs(result.Player.Velocity.Z + 12f), 0f, 1e-5f);
    }

    [Fact]
    public void ExistingFasterInwardSpeedIsNotClampedDown()
    {
        var config = new WinchConfig
        {
            PullRadialAcceleration = 420f,
            PullTargetInwardSpeed = 55f,
        };
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(8f, 0f, -80f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 0f, -20f), 0f),
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.InRange(Math.Abs(result.Player.Velocity.X - 8f), 0f, 1e-5f);
        Assert.InRange(Math.Abs(result.Player.Velocity.Z + 80f), 0f, 1e-5f);
        Assert.Equal(0f, result.Winch.LastPullAcceleration);
    }

    [Fact]
    public void NewRmbShotPreservesOldMomentumThenAddsNewPull()
    {
        var velocity = new Vector3(14f, 3f, -9f);
        var player = PlayerState.Initial with { Velocity = velocity };
        var newTarget = new Vector3(20f, 0f, 0f);

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 10f, -20f), 0f),
            Input(PlayerButtons.GrapplePullPressed),
            new WinchConfig
            {
                PullInitialImpulse = 30f,
                PullRadialAcceleration = 420f,
                PullTargetInwardSpeed = 55f,
            },
            new PlayerLocomotionConfig(),
            new FixedHitWorld(newTarget),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.True(result.Winch.IsPulling);
        Assert.Equal(newTarget, result.Winch.Path.CurrentPullPoint);
        Assert.True(result.Player.Velocity.X > velocity.X + 29f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z - velocity.Z),
            0f,
            1e-5f);
    }

    [Fact]
    public void MissedRmbShotClearsCableWithoutChangingVelocity()
    {
        var velocity = new Vector3(12f, 4f, -7f);
        var player = PlayerState.Initial with { Velocity = velocity };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 10f, -20f), 0f),
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
    public void ArrivalStopsPullButKeepsTangentialVelocity()
    {
        var config = new WinchConfig
        {
            ArrivalDistance = 0.75f,
        };
        var point = new Vector3(0f, 0f, -10f);
        var player = PlayerState.Initial with
        {
            Position = new Vector3(0f, 0f, -9.5f),
            Velocity = new Vector3(4f, -2f, -30f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(point, 0f),
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.False(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        AssertVectorClose(
            new Vector3(4f, -2f, 0f),
            result.Player.Velocity);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchState ActiveAt(
        Vector3 point,
        float pullAcceleration) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
            Vector3.Distance(Vector3.Zero, point),
            pullAcceleration);

    private static void AssertVectorClose(Vector3 expected, Vector3 actual)
    {
        Assert.InRange(Math.Abs(actual.X - expected.X), 0f, 1e-5f);
        Assert.InRange(Math.Abs(actual.Y - expected.Y), 0f, 1e-5f);
        Assert.InRange(Math.Abs(actual.Z - expected.Z), 0f, 1e-5f);
    }

    private sealed class FixedHitWorld : IWorldQuery
    {
        private readonly Vector3 _point;

        public FixedHitWorld(Vector3 point)
        {
            _point = point;
        }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = new WorldHit(_point, Vector3.UnitY, 0.5f, 1u);
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
