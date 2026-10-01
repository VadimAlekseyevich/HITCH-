using Hitch.Simulation;
using Hitch.Simulation.Winch;

namespace Hitch.Tests;

public sealed class WinchConfigTests
{
    [Fact]
    public void DefaultWinchConfigIsValid()
    {
        var config = new WinchConfig();

        config.Validate();

        Assert.True(config.GrappleRange > 0f);
        Assert.True(config.ReelMaxSpeed > 0f);
        Assert.True(config.SpringAccelerationPerMeter > 0f);
        Assert.InRange(config.MinimumReelInMultiplier, 0.0001f, 1f);
    }

    [Fact]
    public void FalloffEndMustExceedStart()
    {
        var config = new WinchConfig
        {
            ReelFalloffStartSpeed = 20f,
            ReelFalloffEndSpeed = 20f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }

    [Fact]
    public void SimulationConfigValidatesNestedWinchConfig()
    {
        var config = new SimulationConfig
        {
            Winch = new WinchConfig
            {
                GrappleRange = float.NaN,
            },
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }
}
