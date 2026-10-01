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
    public void PullPreservesTangentialMomentumAndAppliesGravity()
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
            Math.Abs(result.Player.Velocity.Y + 3.6f),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z + 42f),
            0f,
            1e-5f);
    }

    [Fact]
    public void PullReversesAwayMotionWithinRadialAccelerationLimit()
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

        // Initial radial speed is -30 m/s (away). At 300 m/s² for 0.2 s,
        // the solver may add at most 60 m/s inward this tick, reaching +30 m/s.
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z + 30f),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.X),
            0f,
            1e-5f);
        Assert.True(result.Player.Velocity.Y < 0f);
    }

    [Fact]
    public void GravityBendsTrajectoryWhileGrappleRemainsActive()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(12f, 8f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            ActiveAt(new Vector3(0f, 0f, -50f)),
            PlayerInput.Neutral,
            TestConfig(),
            new PlayerLocomotionConfig
            {
                Gravity = 18f,
            },
            new NoHitWorld(),
            0.25f);

        Assert.True(result.Winch.IsPulling);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.X - 12f),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Y - 3.5f),
            0f,
            1e-5f);
        Assert.InRange(
            Math.Abs(result.Player.Velocity.Z + 42f),
            0f,
            1e-5f);
    }

    [Fact]
    public void LongGrappleUsesExtremeTraversalSpeed()
    {
        var config = new WinchConfig
        {
            PullTargetInwardSpeed = 90f,
            PullLongRangeInwardSpeed = 160f,
            PullLongRangeDistance = 250f,
        };

        var speed = WinchSystem.ComputeDirectPullSpeed(
            300f,
            config);

        Assert.InRange(speed, 159.99f, 160.01f);
    }

    [Fact]
    public void GrappleSpeedBuildsWithAnchorDistance()
    {
        var config = new WinchConfig
        {
            PullTargetInwardSpeed = 90f,
            PullLongRangeInwardSpeed = 160f,
            PullLongRangeDistance = 250f,
        };

        var shortSpeed = WinchSystem.ComputeDirectPullSpeed(20f, config);
        var mediumSpeed = WinchSystem.ComputeDirectPullSpeed(125f, config);
        var longSpeed = WinchSystem.ComputeDirectPullSpeed(250f, config);

        Assert.True(shortSpeed >= 90f);
        Assert.True(mediumSpeed > shortSpeed);
        Assert.True(longSpeed > mediumSpeed);
        Assert.InRange(longSpeed, 159.99f, 160.01f);
    }

    [Fact]
    public void LongRangeStepRampsTowardTargetInsteadOfSnappingToIt()
    {
        var config = new WinchConfig
        {
            PullTargetInwardSpeed = 90f,
            PullLongRangeInwardSpeed = 160f,
            PullLongRangeDistance = 250f,
            PullLaunchInitialMultiplier = 1f,
            PullLaunchPeakMultiplier = 1f,
            PullLaunchPeakSeconds = 0.10f,
        };

        var result = WinchSystem.Step(
            PlayerState.Initial,
            ActiveAt(new Vector3(0f, 0f, -300f)),
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.IsPulling);
        var maximumFirstTickRadialChange =
            config.PullRadialAcceleration / 60f;
        Assert.InRange(
            result.Player.Velocity.Length(),
            maximumFirstTickRadialChange - 0.02f,
            maximumFirstTickRadialChange + 0.02f);
        Assert.True(result.Player.Velocity.Z < 0f);
        Assert.True(result.Player.Velocity.Length() < 160f);
    }

    [Fact]
    public void FreshGrappleLaunchesHardThenSurgesHigherBeforeDecaying()
    {
        var config = new WinchConfig
        {
            PullTargetInwardSpeed = 100f,
            PullLongRangeInwardSpeed = 100f,
            PullLongRangeDistance = 250f,
            PullLaunchInitialMultiplier = 1.75f,
            PullLaunchPeakMultiplier = 2.75f,
            PullLaunchPeakSeconds = 0.10f,
            PullLaunchDecaySeconds = 0.70f,
        };

        var immediate = WinchSystem.ComputeDirectPullSpeed(
            100f,
            0f,
            config);
        var rising = WinchSystem.ComputeDirectPullSpeed(
            100f,
            0.05f,
            config);
        var peak = WinchSystem.ComputeDirectPullSpeed(
            100f,
            0.10f,
            config);
        var decaying = WinchSystem.ComputeDirectPullSpeed(
            100f,
            0.45f,
            config);
        var settled = WinchSystem.ComputeDirectPullSpeed(
            100f,
            0.80f,
            config);

        Assert.InRange(immediate, 174.99f, 175.01f);
        Assert.InRange(rising, 224.99f, 225.01f);
        Assert.InRange(peak, 274.99f, 275.01f);
        Assert.True(decaying < peak);
        Assert.True(decaying > settled);
        Assert.InRange(settled, 99.99f, 100.01f);
    }

    [Fact]
    public void PullStateAdvancesIntoSecondStagePeak()
    {
        var config = new WinchConfig
        {
            PullTargetInwardSpeed = 100f,
            PullLongRangeInwardSpeed = 100f,
            PullLongRangeDistance = 250f,
            PullLaunchInitialMultiplier = 1.75f,
            PullLaunchPeakMultiplier = 2.75f,
            PullLaunchPeakSeconds = 0.10f,
            PullLaunchDecaySeconds = 0.70f,
        };
        var player = PlayerState.Initial;
        var winch = ActiveAt(new Vector3(0f, 0f, -100f));

        var first = WinchSystem.Step(
            player,
            winch,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.05f);

        var second = WinchSystem.Step(
            first.Player,
            first.Winch,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.05f);

        var third = WinchSystem.Step(
            second.Player,
            second.Winch,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.05f);

        Assert.InRange(first.Winch.PullElapsedSeconds, 0.049f, 0.051f);
        Assert.InRange(second.Winch.PullElapsedSeconds, 0.099f, 0.101f);
        Assert.InRange(third.Winch.PullElapsedSeconds, 0.149f, 0.151f);
        Assert.True(
            second.Player.Velocity.Length()
            > first.Player.Velocity.Length());
        Assert.True(
            third.Player.Velocity.Length()
            > second.Player.Velocity.Length());
    }

    [Fact]
    public void WrappedRopeCannotCompleteAtIntermediateBend()
    {
        var config = TestConfig();
        var locomotion = new PlayerLocomotionConfig();
        var path = WinchPathState
            .AtWorldAnchor(
                new Vector3(10f, 0f, 0f),
                -Vector3.UnitX)
            .PushContact(
                new Vector3(2f, 0f, 0f));
        var winch = new WinchState(
            WinchTargetState.Selected,
            path,
            true,
            10f,
            0f);
        var player = PlayerState.Initial with
        {
            Position = new Vector3(1.5f, 0f, 0f),
        };

        Assert.False(
            WinchSystem.HasReachedAnchor(
                player,
                winch,
                config,
                locomotion));
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
            PullLongRangeInwardSpeed = 42f,
            PullLongRangeDistance = 250f,
            PullLaunchInitialMultiplier = 1f,
            PullLaunchPeakMultiplier = 1f,
            PullLaunchPeakSeconds = 0.10f,
            PullLaunchDecaySeconds = 0.75f,
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
