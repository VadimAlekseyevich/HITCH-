using Hitch.Simulation;
using Hitch.Simulation.Winch;

namespace Hitch.Tests;

public sealed class WinchConfigTests
{
    [Fact]
    public void DefaultDirectPullConfigIsValid()
    {
        var config = new WinchConfig();

        config.Validate();

        Assert.True(config.GrappleRange > 0f);
        Assert.True(config.PullSpeed > 0f);
        Assert.True(config.ArrivalDistance > 0f);
    }

    [Fact]
    public void PullSpeedMustBePositive()
    {
        var config = new WinchConfig
        {
            PullSpeed = 0f,
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
                ArrivalDistance = float.NaN,
            },
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }
}
