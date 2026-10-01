using Hitch.Simulation;
using Hitch.Simulation.Winch;

namespace Hitch.Tests;

public sealed class WinchConfigTests
{
    [Fact]
    public void DefaultSingleCableConfigIsValid()
    {
        var config = new WinchConfig();

        config.Validate();

        Assert.True(config.PullInitialImpulse > 0f);
        Assert.True(config.PullRadialAcceleration > 0f);
        Assert.True(config.PullTargetInwardSpeed > 0f);
        Assert.True(config.ArrivalContactTolerance >= 0f);
    }

    [Fact]
    public void ArrivalContactToleranceMayBeZero()
    {
        var config = new WinchConfig
        {
            ArrivalContactTolerance = 0f,
        };

        config.Validate();
    }

    [Fact]
    public void SimulationConfigRejectsInvalidWinchValues()
    {
        var config = new SimulationConfig
        {
            Winch = new WinchConfig
            {
                PullRadialAcceleration = float.NaN,
            },
        };

        Assert.Throws<ArgumentOutOfRangeException>(config.Validate);
    }
}
