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

        Assert.True(config.MaxRopeLength > 0f);
        Assert.True(config.PullInitialImpulse > 0f);
        Assert.True(config.PullRadialAcceleration > 0f);
        Assert.True(config.PullTargetInwardSpeed > 0f);
        Assert.True(config.PullLongRangeInwardSpeed >= config.PullTargetInwardSpeed);
        Assert.True(config.PullLongRangeDistance > 0f);
        Assert.True(config.PullLaunchInitialMultiplier >= 1f);
        Assert.True(config.PullLaunchPeakMultiplier >= config.PullLaunchInitialMultiplier);
        Assert.True(config.PullLaunchPeakSeconds > 0f);
        Assert.True(config.PullLaunchDecaySeconds > 0f);
        Assert.True(config.ArrivalContactTolerance >= 0f);
        Assert.True(config.ArrivalSurfaceCaptureRadius > 0f);
        Assert.True(config.RopeContactSurfaceOffset > 0f);
        Assert.True(config.RopeEndpointTolerance > 0f);
        Assert.True(config.RopeMinimumContactSpacing > 0f);
        Assert.True(config.RopeContactEdgeSearchDistance > 0f);
        Assert.True(config.RopeConstraintCorrectionSpeed > 0f);
        Assert.True(config.RopeTautTolerance >= 0f);
        Assert.True(config.GasAcceleration > 0f);
        Assert.True(config.GasFullAccelerationSpeed >= 0f);
        Assert.True(config.GasCutoffSpeed > config.GasFullAccelerationSpeed);
        Assert.InRange(config.DualCableMotorScale, 0f, 1f);
    }

    [Fact]
    public void DefaultDualOdmReelUsesControlledLaunchAssist()
    {
        var config = new WinchConfig();

        Assert.InRange(
            config.PullLaunchInitialMultiplier,
            1.20f,
            1.30f);
        Assert.InRange(
            config.PullLaunchPeakMultiplier,
            1.60f,
            1.70f);
        Assert.InRange(
            config.PullTargetInwardSpeed,
            18f,
            22f);
        Assert.InRange(
            config.PullLongRangeInwardSpeed,
            30f,
            34f);
        Assert.InRange(
            config.MaxRopeLength,
            140f,
            160f);
        Assert.InRange(
            config.DualCableMotorScale,
            0.65f,
            0.80f);
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
    public void LongRangeSpeedCannotBeSlowerThanBaseSpeed()
    {
        var config = new WinchConfig
        {
            PullTargetInwardSpeed = 100f,
            PullLongRangeInwardSpeed = 90f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            config.Validate);
    }

    [Fact]
    public void LaunchInitialMultiplierCannotReduceInitialSpeed()
    {
        var config = new WinchConfig
        {
            PullLaunchInitialMultiplier = 0.99f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            config.Validate);
    }

    [Fact]
    public void LaunchPeakCannotBeBelowImmediateLaunch()
    {
        var config = new WinchConfig
        {
            PullLaunchInitialMultiplier = 2f,
            PullLaunchPeakMultiplier = 1.9f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            config.Validate);
    }

    [Fact]
    public void MaximumRopeLengthMustBePositive()
    {
        var config = new WinchConfig
        {
            MaxRopeLength = 0f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            config.Validate);
    }

    [Fact]
    public void RopeTautToleranceMayBeZero()
    {
        var config = new WinchConfig
        {
            RopeTautTolerance = 0f,
        };

        config.Validate();
    }

    [Fact]
    public void GasCutoffMustExceedFullAccelerationSpeed()
    {
        var config = new WinchConfig
        {
            GasFullAccelerationSpeed = 20f,
            GasCutoffSpeed = 20f,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            config.Validate);
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
