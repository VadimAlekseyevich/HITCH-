using Hitch.Simulation;
using Hitch.Simulation.Player;

namespace Hitch.Tests;

public sealed class PlayerLocomotionConfigTests
{
    [Fact]
    public void DefaultsDescribeWeakCapsuleLocomotion()
    {
        var config = new PlayerLocomotionConfig();

        config.Validate();

        Assert.Equal(0.45f, config.CapsuleRadius);
        Assert.Equal(1.80f, config.CapsuleHeight);
        Assert.Equal(0.45f, config.CapsuleHalfSegmentLength, 5);
        Assert.True(config.GroundMaxSpeed < 5f);
        Assert.True(config.AirAcceleration < config.GroundAcceleration);
    }

    [Fact]
    public void CapsuleHeightMustExceedDiameter()
    {
        var config = new PlayerLocomotionConfig
        {
            CapsuleRadius = 0.5f,
            CapsuleHeight = 1.0f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }

    [Fact]
    public void SlideIterationCountIsBounded()
    {
        var config = new PlayerLocomotionConfig
        {
            MaxSlideIterations = 0,
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }

    [Fact]
    public void SimulationConfigValidatesNestedLocomotionConfig()
    {
        var config = new SimulationConfig
        {
            Locomotion = new PlayerLocomotionConfig
            {
                Gravity = float.NaN,
            },
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }
}
