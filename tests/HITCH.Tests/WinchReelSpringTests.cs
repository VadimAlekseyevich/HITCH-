using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchReelSpringTests
{
    [Fact]
    public void ReelInAcceleratesMotorAndShortensRestLength()
    {
        var config = new WinchConfig
        {
            ReelAcceleration = 60f,
            ReelMaxSpeed = 20f,
            PretensionDistance = 0f,
            SpringAccelerationPerMeter = 0f,
            SlackTakeUpSpeed = 0f,
        };
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 10f);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            ReelInput(+1f),
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.InRange(Math.Abs(result.Winch.ReelVelocity - 6f), 0f, 1e-5f);
        Assert.InRange(Math.Abs(result.Winch.RestLength - 9.4f), 0f, 1e-5f);
    }

    [Fact]
    public void NeutralInputDeceleratesReelMotorTowardZero()
    {
        var config = new WinchConfig
        {
            ReelDeceleration = 20f,
            PretensionDistance = 0f,
            SpringAccelerationPerMeter = 0f,
            SlackTakeUpSpeed = 0f,
        };
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 10f,
            reelVelocity: 5f);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.InRange(Math.Abs(result.Winch.ReelVelocity - 3f), 0f, 1e-5f);
    }

    [Fact]
    public void ReelDirectionCanReverseThroughAcceleration()
    {
        var config = new WinchConfig
        {
            ReelAcceleration = 20f,
            PretensionDistance = 0f,
            SpringAccelerationPerMeter = 0f,
            SlackTakeUpSpeed = 0f,
        };
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 10f,
            reelVelocity: 2f);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            ReelInput(-1f),
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.2f);

        Assert.InRange(Math.Abs(result.Winch.ReelVelocity + 2f), 0f, 1e-5f);
        Assert.True(result.Winch.RestLength > 10f);
    }

    [Fact]
    public void SpringPullsInwardWhenPlayerIsOutsideRestLength()
    {
        var config = new WinchConfig
        {
            SpringAccelerationPerMeter = 10f,
            PretensionDistance = 0f,
            OutwardDampingPerSecond = 0f,
            SlackTakeUpSpeed = 0f,
        };
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 5f);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.True(result.Winch.LastTensionAcceleration > 0f);
        Assert.True(result.Player.Velocity.Z < 0f);
    }

    [Fact]
    public void SpringNeverPushesOutwardWhenInsideRestLength()
    {
        var config = new WinchConfig
        {
            SpringAccelerationPerMeter = 100f,
            PretensionDistance = 0f,
            OutwardDampingPerSecond = 100f,
            SlackTakeUpSpeed = 0f,
        };
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 20f);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.Equal(0f, result.Winch.LastTensionAcceleration);
        Assert.Equal(Vector3.Zero, result.Player.Velocity);
    }

    [Fact]
    public void OutwardRadialVelocityAddsDampingTension()
    {
        var config = new WinchConfig
        {
            SpringAccelerationPerMeter = 10f,
            PretensionDistance = 0f,
            OutwardDampingPerSecond = 5f,
            SlackTakeUpSpeed = 0f,
        };
        var attached = AttachedAt(
            new Vector3(0f, 0f, -10f),
            restLength: 5f);

        var stationary = WinchSystem.Step(
            PlayerState.Initial,
            attached,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        var movingAway = WinchSystem.Step(
            PlayerState.Initial with
            {
                Velocity = new Vector3(0f, 0f, 4f),
            },
            attached,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.True(
            movingAway.Winch.LastTensionAcceleration
            > stationary.Winch.LastTensionAcceleration);
    }

    [Fact]
    public void ReelInFalloffMatchesConfiguredEndpoints()
    {
        var config = new WinchConfig
        {
            ReelFalloffStartSpeed = 20f,
            ReelFalloffEndSpeed = 60f,
            MinimumReelInMultiplier = 0.25f,
        };

        Assert.Equal(1f, WinchSystem.GetReelInMultiplier(10f, config));
        Assert.Equal(1f, WinchSystem.GetReelInMultiplier(20f, config));
        Assert.Equal(0.25f, WinchSystem.GetReelInMultiplier(60f, config));
        Assert.Equal(0.25f, WinchSystem.GetReelInMultiplier(100f, config));

        var midpoint = WinchSystem.GetReelInMultiplier(40f, config);
        Assert.InRange(Math.Abs(midpoint - 0.625f), 0f, 1e-5f);
    }

    [Fact]
    public void ReelInFalloffDoesNotClampExistingPlayerVelocity()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(100f, 0f, 0f),
        };
        var config = new WinchConfig
        {
            SpringAccelerationPerMeter = 0f,
            PretensionDistance = 0f,
            SlackTakeUpSpeed = 0f,
        };

        var result = WinchSystem.Step(
            player,
            AttachedAt(new Vector3(0f, 0f, -10f), 10f),
            ReelInput(+1f),
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.Equal(100f, result.Player.Velocity.X);
    }

    private static PlayerInput ReelInput(float axis) =>
        new(Vector2.Zero, Vector2.Zero, axis, PlayerButtons.None);

    private static WinchState AttachedAt(
        Vector3 anchor,
        float restLength,
        float reelVelocity = 0f) =>
        new(
            WinchAttachmentState.Attached,
            WinchPathState.AtWorldAnchor(anchor),
            restLength,
            reelVelocity,
            0f,
            Vector3.Distance(Vector3.Zero, anchor),
            0f);

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
