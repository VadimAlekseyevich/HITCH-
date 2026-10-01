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
    public void RmbClickStartsPullWithImmediateImpulse()
    {
        var config = new WinchConfig
        {
            PullInitialImpulse = 18f,
            PullAcceleration = 30f,
            ArrivalDistance = 0.5f,
        };

        var result = WinchSystem.Step(
            PlayerState.Initial,
            SelectedAt(new Vector3(0f, 0f, -20f), pulling: false),
            Input(PlayerButtons.PullPressed),
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.True(result.Winch.IsPulling);
        AssertVectorClose(
            new Vector3(0f, 0f, -21f),
            result.Player.Velocity);
        Assert.Equal(30f, result.Winch.LastPullAcceleration);
    }

    [Fact]
    public void AutomaticPullContinuesOnNeutralTicksWithoutHoldingRmb()
    {
        var config = new WinchConfig
        {
            PullInitialImpulse = 18f,
            PullAcceleration = 20f,
            ArrivalDistance = 0.5f,
        };
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(5f, 0f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            SelectedAt(new Vector3(0f, 0f, -20f), pulling: true),
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            0.1f);

        Assert.True(result.Winch.IsPulling);
        Assert.InRange(Math.Abs(result.Player.Velocity.X - 5f), 0f, 1e-5f);
        Assert.InRange(Math.Abs(result.Player.Velocity.Z + 2f), 0f, 1e-5f);
    }

    [Fact]
    public void LmbRetargetCancelsPullAndPreservesMomentum()
    {
        var velocity = new Vector3(14f, 3f, -9f);
        var player = PlayerState.Initial with { Velocity = velocity };
        var newTarget = new Vector3(20f, 5f, 0f);

        var result = WinchSystem.Step(
            player,
            SelectedAt(new Vector3(0f, 10f, -20f), pulling: true),
            Input(PlayerButtons.SelectGrapplePointPressed),
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new FixedHitWorld(newTarget),
            1f / 60f);

        Assert.True(result.Winch.HasTarget);
        Assert.False(result.Winch.IsPulling);
        Assert.Equal(newTarget, result.Winch.Path.CurrentPullPoint);
        Assert.Equal(velocity, result.Player.Velocity);
    }

    [Fact]
    public void LmbMissRetractsCableAndPreservesMomentum()
    {
        var velocity = new Vector3(12f, 4f, -7f);
        var player = PlayerState.Initial with { Velocity = velocity };

        var result = WinchSystem.Step(
            player,
            SelectedAt(new Vector3(0f, 10f, -20f), pulling: true),
            Input(PlayerButtons.SelectGrapplePointPressed),
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
            SelectedAt(point, pulling: true),
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

    [Fact]
    public void SelectedCableWithoutRmbDoesNotApplyForce()
    {
        var velocity = new Vector3(5f, 2f, -3f);
        var player = PlayerState.Initial with { Velocity = velocity };

        var result = WinchSystem.Step(
            player,
            SelectedAt(new Vector3(0f, 0f, -10f), pulling: false),
            PlayerInput.Neutral,
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.Equal(velocity, result.Player.Velocity);
        Assert.Equal(0f, result.Winch.LastPullAcceleration);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchState SelectedAt(Vector3 point, bool pulling) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            pulling,
            Vector3.Distance(Vector3.Zero, point),
            pulling ? 1f : 0f);

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
