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
    public void PullPreservesTangentialMomentumBeforeArrival()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(17f, 0f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 0f, -20f)),
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.2f);

        Assert.True(result.Winch.IsPulling);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.X - 17f),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z + 42f),
            0f,
            1e-5f);
    }

    [Fact]
    public void PullRapidlyReversesMotionAwayFromAnchor()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(0f, 0f, 30f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 0f, -20f)),
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.2f);

        Assert.True(result.Player.Velocity.Z < 0f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z + 30f),
            0f,
            1e-5f);
    }

    [Fact]
    public void HorizontalWallArrivalUsesCapsuleRadiusInsteadOfOldFixedDistance()
    {
        var config = TestConfig();
        var locomotion = new PlayerLocomotionConfig();
        var direction = Vector3.UnitX;

        var arrival = WinchSystem.ComputeCapsuleAwareArrivalDistance(
            direction,
            config,
            locomotion);

        Assert.InRange(arrival, 0.52f, 0.54f);
    }

    [Fact]
    public void CeilingArrivalAccountsForCapsuleHalfHeight()
    {
        var config = TestConfig();
        var locomotion = new PlayerLocomotionConfig();
        var direction = Vector3.UnitY;

        var arrival = WinchSystem.ComputeCapsuleAwareArrivalDistance(
            direction,
            config,
            locomotion);

        Assert.InRange(arrival, 0.97f, 0.99f);
    }

    [Fact]
    public void CeilingContactCompletesPullAndZeroesVelocity()
    {
        var point = new Vector3(0f, 0.95f, 0f);
        var player = PlayerState.Initial with
        {
            Position = Vector3.Zero,
            Velocity = new Vector3(6f, 8f, 3f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(point),
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.True(result.Winch.IsLatched);
        Assert.Equal(Vector3.Zero, result.Player.Velocity);
    }

    [Fact]
    public void WallSurfaceContactCompletesDespiteTangentialSlidePastExactAnchorPoint()
    {
        var config = TestConfig() with
        {
            ArrivalSurfaceCaptureRadius = 1.75f,
        };
        var locomotion = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Position = new Vector3(0.50f, 0f, 1.20f),
            Velocity = new Vector3(-20f, 0f, 14f),
        };
        var winch = ActiveAtSurface(
            Vector3.Zero,
            Vector3.UnitX);

        Assert.True(
            WinchSystem.HasReachedAnchor(
                player,
                winch,
                config,
                locomotion));
    }

    [Fact]
    public void TouchingSameWallFarFromSelectedAnchorDoesNotCompletePull()
    {
        var config = TestConfig() with
        {
            ArrivalSurfaceCaptureRadius = 1.75f,
        };
        var locomotion = new PlayerLocomotionConfig();
        var player = PlayerState.Initial with
        {
            Position = new Vector3(0.50f, 0f, 2.25f),
        };
        var winch = ActiveAtSurface(
            Vector3.Zero,
            Vector3.UnitX);

        Assert.False(
            WinchSystem.HasReachedAnchor(
                player,
                winch,
                config,
                locomotion));
    }

    [Fact]
    public void HorizontalCableDoesNotFinishTooEarlyAtOldPointNineThreshold()
    {
        var point = new Vector3(0.80f, 0f, 0f);
        var player = PlayerState.Initial with
        {
            Position = Vector3.Zero,
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(point),
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.IsPulling);
        Assert.True(result.Player.Velocity.X > 0f);
    }

    private static WinchConfig TestConfig() =>
        new()
        {
            PullInitialImpulse = 24f,
            PullRadialAcceleration = 300f,
            PullTargetInwardSpeed = 42f,
            ArrivalContactTolerance = 0.06f,
        };

    private static WinchState ActiveAt(Vector3 point) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
            Vector3.Distance(Vector3.Zero, point),
            0f);

    private static WinchState ActiveAtSurface(
        Vector3 point,
        Vector3 normal) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point, normal),
            true,
            Vector3.Distance(Vector3.Zero, point),
            0f);

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
