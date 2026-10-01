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
    public void PullStartsAtConfiguredSpeedImmediately()
    {
        var config = new WinchConfig
        {
            PullSpeed = 18f,
            ArrivalDistance = 0.5f,
        };
        var state = SelectedAt(new Vector3(0f, 0f, -10f), pulling: false);

        var result = WinchSystem.Step(
            PlayerState.Initial,
            state,
            Input(PlayerButtons.PullPressed),
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.True(result.Winch.IsPulling);
        Assert.Equal(new Vector3(0f, 0f, -18f), result.Player.Velocity);
        Assert.Equal(18f, result.Winch.LastPullSpeed);
    }

    [Fact]
    public void PullDirectionIsRecomputedEveryTick()
    {
        var config = new WinchConfig
        {
            PullSpeed = 10f,
            ArrivalDistance = 0.5f,
        };
        var state = SelectedAt(new Vector3(10f, 10f, 0f), pulling: true);
        var player = PlayerState.Initial with
        {
            Position = new Vector3(0f, 0f, 0f),
        };

        var result = WinchSystem.Step(
            player,
            state,
            PlayerInput.Neutral,
            config,
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        var expected = Vector3.Normalize(new Vector3(10f, 10f, 0f)) * 10f;
        AssertVectorClose(expected, result.Player.Velocity);
    }

    [Fact]
    public void ReachingPointConsumesTargetStopsPullAndZeroesVelocity()
    {
        var config = new WinchConfig
        {
            ArrivalDistance = 0.75f,
        };
        var point = new Vector3(0f, 0f, -10f);
        var player = PlayerState.Initial with
        {
            Position = new Vector3(0f, 0f, -9.5f),
            Velocity = new Vector3(100f, 20f, -30f),
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
        Assert.Equal(Vector3.Zero, result.Player.Velocity);
    }

    [Fact]
    public void SelectedPointWithoutRmbDoesNotAlterVelocity()
    {
        var player = PlayerState.Initial with
        {
            Velocity = new Vector3(5f, 2f, -3f),
        };

        var result = WinchSystem.Step(
            player,
            SelectedAt(new Vector3(0f, 0f, -10f), pulling: false),
            PlayerInput.Neutral,
            new WinchConfig(),
            new PlayerLocomotionConfig(),
            new NoHitWorld(),
            1f / 60f);

        Assert.Equal(player.Velocity, result.Player.Velocity);
        Assert.Equal(0f, result.Winch.LastPullSpeed);
    }

    private static PlayerInput Input(PlayerButtons buttons) =>
        new(Vector2.Zero, Vector2.Zero, 0f, buttons);

    private static WinchState SelectedAt(Vector3 point, bool pulling) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            pulling,
            Vector3.Distance(Vector3.Zero, point),
            pulling ? 18f : 0f);

    private static void AssertVectorClose(Vector3 expected, Vector3 actual)
    {
        Assert.InRange(Math.Abs(actual.X - expected.X), 0f, 1e-5f);
        Assert.InRange(Math.Abs(actual.Y - expected.Y), 0f, 1e-5f);
        Assert.InRange(Math.Abs(actual.Z - expected.Z), 0f, 1e-5f);
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
