using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchGameSimulationIntegrationTests
{
    [Fact]
    public void AttachReelAndDetachPreserveEarnedHorizontalMomentum()
    {
        var config = new SimulationConfig
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                Gravity = 18f,
                AirAcceleration = 0f,
            },
            Winch = new Hitch.Simulation.Winch.WinchConfig
            {
                PretensionDistance = 0.5f,
                SpringAccelerationPerMeter = 60f,
                ReelAcceleration = 80f,
                ReelMaxSpeed = 15f,
            },
        };

        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 10f, 0f),
            },
        };

        var world = new FixedGrappleWorld(
            new Vector3(10f, 10f, -10f));
        var simulation = new GameSimulation(config, initial);

        var attached = simulation.Step(
            new PlayerInput(
                Vector2.Zero,
                Vector2.Zero,
                0f,
                PlayerButtons.GrapplePressed),
            world);

        Assert.True(attached.Winch.IsAttached);
        Assert.True(attached.Winch.LastTensionAcceleration > 0f);

        for (var i = 0; i < 20; i++)
        {
            simulation.Step(
                new PlayerInput(
                    Vector2.Zero,
                    Vector2.Zero,
                    1f,
                    PlayerButtons.None),
                world);
        }

        var beforeDetach = simulation.State;
        var horizontalBefore = new Vector2(
            beforeDetach.Player.Velocity.X,
            beforeDetach.Player.Velocity.Z).Length();

        Assert.True(horizontalBefore > 0.1f);
        Assert.True(
            beforeDetach.Winch.RestLength
            < attached.Winch.RestLength);

        var afterDetach = simulation.Step(
            new PlayerInput(
                Vector2.Zero,
                Vector2.Zero,
                0f,
                PlayerButtons.GrappleReleased),
            world);

        var horizontalAfter = new Vector2(
            afterDetach.Player.Velocity.X,
            afterDetach.Player.Velocity.Z).Length();

        Assert.False(afterDetach.Winch.IsAttached);

        // Ordinary gravity still runs on the detach tick, but horizontal momentum must not reset.
        Assert.InRange(
            Math.Abs(horizontalAfter - horizontalBefore),
            0f,
            1e-4f);
    }

    private sealed class FixedGrappleWorld : IWorldQuery
    {
        private readonly Vector3 _anchor;

        public FixedGrappleWorld(Vector3 anchor)
        {
            _anchor = anchor;
        }

        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = new WorldHit(
                _anchor,
                Vector3.UnitY,
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
}
