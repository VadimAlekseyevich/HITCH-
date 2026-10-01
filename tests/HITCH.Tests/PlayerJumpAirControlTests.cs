using System.Numerics;
using Hitch.Simulation;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class PlayerJumpAirControlTests
{
    [Fact]
    public void GroundedJumpStartsStrongUpwardVelocityAndLeavesGround()
    {
        var config = new SimulationConfig();
        var simulation = new GameSimulation(
            config,
            GroundedInitialState());

        var after = simulation.Step(
            new PlayerInput(
                Vector2.Zero,
                Vector2.Zero,
                0f,
                PlayerButtons.JumpPressed),
            new FlatFloorWorld());

        var expectedVelocityY =
            config.Locomotion.JumpSpeed
            - (config.Locomotion.Gravity / config.TickRateHz);

        Assert.InRange(
            Math.Abs(after.Player.Velocity.Y - expectedVelocityY),
            0f,
            1e-5f);
        Assert.True(after.Player.Velocity.Y > 0f);
        Assert.False(after.Player.IsGrounded);
    }

    [Fact]
    public void OneAirJumpIsAvailableAndThenConsumed()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                Velocity = new Vector3(0f, -3f, 0f),
                IsGrounded = false,
                AirJumpAvailable = true,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var airJump = simulation.Step(
            new PlayerInput(
                Vector2.Zero,
                Vector2.Zero,
                0f,
                PlayerButtons.JumpPressed),
            new NoHitWorld());

        var expectedAirJumpVelocity =
            config.Locomotion.AirJumpSpeed
            - (config.Locomotion.Gravity / config.TickRateHz);

        Assert.InRange(
            Math.Abs(
                airJump.Player.Velocity.Y
                - expectedAirJumpVelocity),
            0f,
            1e-5f);
        Assert.False(airJump.Player.AirJumpAvailable);

        var beforeThirdPress = airJump.Player.Velocity.Y;

        var thirdPress = simulation.Step(
            new PlayerInput(
                Vector2.Zero,
                Vector2.Zero,
                0f,
                PlayerButtons.JumpPressed),
            new NoHitWorld());

        Assert.True(
            thirdPress.Player.Velocity.Y
            < beforeThirdPress);
        Assert.False(thirdPress.Player.AirJumpAvailable);
    }

    [Fact]
    public void LandingRestoresAirJump()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
                Velocity = new Vector3(0f, -1f, 0f),
                IsGrounded = false,
                AirJumpAvailable = false,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            PlayerInput.Neutral,
            new FlatFloorWorld());

        Assert.True(after.Player.IsGrounded);
        Assert.True(after.Player.AirJumpAvailable);
    }

    [Fact]
    public void AirCorrectionIsMuchWeakerThanGroundAcceleration()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                IsGrounded = false,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            new PlayerInput(
                new Vector2(0f, 1f),
                Vector2.Zero,
                0f,
                PlayerButtons.None),
            new NoHitWorld());

        var horizontalSpeed = new Vector2(
            after.Player.Velocity.X,
            after.Player.Velocity.Z).Length();

        var expectedAirAddition =
            config.Locomotion.AirAcceleration / config.TickRateHz;
        var expectedGroundAddition =
            config.Locomotion.GroundAcceleration / config.TickRateHz;

        Assert.InRange(
            Math.Abs(horizontalSpeed - expectedAirAddition),
            0f,
            1e-5f);
        Assert.True(horizontalSpeed < expectedGroundAddition);
    }

    [Fact]
    public void AirControlDoesNotClampExistingHighForwardMomentum()
    {
        var config = new SimulationConfig();
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                Velocity = new Vector3(0f, 0f, -20f),
                IsGrounded = false,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            new PlayerInput(
                new Vector2(0f, 1f),
                Vector2.Zero,
                0f,
                PlayerButtons.None),
            new NoHitWorld());

        Assert.InRange(
            Math.Abs(after.Player.Velocity.Z + 20f),
            0f,
            1e-5f);
    }

    [Fact]
    public void AirCorrectionCanBeDisabled()
    {
        var config = new SimulationConfig
        {
            Locomotion = new Hitch.Simulation.Player.PlayerLocomotionConfig
            {
                AirAcceleration = 0f,
            },
        };
        var initial = SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 5f, 0f),
                IsGrounded = false,
            },
        };
        var simulation = new GameSimulation(config, initial);

        var after = simulation.Step(
            new PlayerInput(
                new Vector2(1f, 0f),
                Vector2.Zero,
                0f,
                PlayerButtons.None),
            new NoHitWorld());

        Assert.Equal(0f, after.Player.Velocity.X);
        Assert.Equal(0f, after.Player.Velocity.Z);
    }

    private static SimulationState GroundedInitialState() =>
        SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Position = new Vector3(0f, 0.92f, 0f),
                IsGrounded = true,
            },
        };

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

    private sealed class FlatFloorWorld : IWorldQuery
    {
        public bool TryRaycast(in RayQuery query, out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
        {
            if (query.Displacement.Y < 0f)
            {
                hit = new WorldHit(
                    query.StartCenter,
                    Vector3.UnitY,
                    0f,
                    1u);
                return true;
            }

            hit = default;
            return false;
        }
    }
}
