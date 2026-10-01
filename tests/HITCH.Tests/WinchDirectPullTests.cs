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
    public void LeftCableImmediatelyEstablishesStrongInwardSpeed()
    {
        var result = WinchSystem.Step(
            PlayerState.Initial,
            WinchState.Initial,
            Input(PlayerButtons.LeftGrapplePressed),
            TestConfig(),
            new PlayerLocomotionConfig(),
            new FixedHitWorld(new Vector3(0f, 0f, -20f)),
            0.1f);

        Assert.True(result.Winch.Left.IsPulling);
        Assert.False(result.Winch.Right.IsPulling);
        AssertVectorClose(
            new Vector3(0f, 0f, -42f),
            result.Player.Velocity);
    }

    [Fact]
    public void TwoSymmetricCablesCombinePullWithoutSideBias()
    {
        var state = new WinchState(
            ActiveCable(new Vector3(-20f, 0f, -20f)),
            ActiveCable(new Vector3(20f, 0f, -20f)));

        var result = WinchSystem.Step(
            PlayerState.Initial,
            state,
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.2f);

        Assert.Equal(2, result.Winch.ActiveCableCount);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.X),
            0f,
            1e-4f);
        Assert.True(result.Player.Velocity.Z < -59f);
    }

    [Fact]
    public void TwoCablesPreserveExistingTangentialMomentum()
    {
        var state = new WinchState(
            ActiveCable(new Vector3(-20f, 0f, -20f)),
            ActiveCable(new Vector3(20f, 0f, -20f)));
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(0f, 15f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            state,
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.2f);

        Assert.InRange(
            Math.Abs(result.Player.Velocity.Y - 15f),
            0f,
            1e-5f);
        Assert.True(result.Player.Velocity.Z < -59f);
    }

    [Fact]
    public void OneCableArrivalDoesNotStopWhileOtherCableStillPulls()
    {
        var config = TestConfig() with
        {
            ArrivalDistance = 0.75f,
        };
        var state = new WinchState(
            ActiveCable(new Vector3(0f, 0f, -0.5f)),
            ActiveCable(new Vector3(0f, 0f, -20f)));
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(8f, 3f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            state,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.False(result.Winch.Left.HasTarget);
        Assert.True(result.Winch.Right.IsPulling);
        Assert.NotEqual(Vector3.Zero, result.Player.Velocity);
        Assert.True(result.Player.Velocity.Z < 0f);
    }

    [Fact]
    public void LastCableArrivalStopsAllVelocity()
    {
        var config = TestConfig() with
        {
            ArrivalDistance = 0.75f,
        };
        var state = new WinchState(
            ActiveCable(new Vector3(0f, 0f, -0.5f)),
            WinchCableState.Initial);
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(8f, 3f, -20f),
        };

        var result = WinchSystem.Step(
            player,
            state,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.Equal(0, result.Winch.ActiveCableCount);
        Assert.Equal(Vector3.Zero, result.Player.Velocity);
    }

    [Fact]
    public void FasterExistingInwardSpeedIsNotClampedDown()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(8f, 0f, -80f),
        };
        var state = new WinchState(
            ActiveCable(new Vector3(0f, 0f, -20f)),
            WinchCableState.Initial);

        var result = WinchSystem.Step(
            player,
            state,
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.InRange(
            Math.Abs(result.Player.Velocity.X - 8f),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z + 80f),
            0f,
            1e-5f);
        Assert.Equal(
            0f,
            result.Winch.Left.LastPullAcceleration);
    }

    private static WinchConfig TestConfig() =>
        new()
        {
            PullInitialImpulse = 24f,
            PullRadialAcceleration = 300f,
            PullTargetInwardSpeed = 42f,
            ArrivalDistance = 0.5f,
        };

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchCableState ActiveCable(Vector3 point) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
            Vector3.Distance(Vector3.Zero, point),
            0f);

    private static void AssertVectorClose(
        Vector3 expected,
        Vector3 actual)
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
