using System.Numerics;
using Hitch.Simulation.Debug;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;

namespace Hitch.Tests;

public sealed class SimulationTelemetryTests
{
    [Fact]
    public void TelemetryTracksSpeedTargetAndAutomaticPullTransitions()
    {
        var telemetry = new SimulationTelemetry();

        telemetry.Observe(State(3f, hasTarget: false, pulling: false, pullAcceleration: 0f));
        telemetry.Observe(State(5f, hasTarget: true, pulling: false, pullAcceleration: 0f));
        telemetry.Observe(State(8f, hasTarget: true, pulling: true, pullAcceleration: 32f));
        telemetry.Observe(State(4f, hasTarget: true, pulling: false, pullAcceleration: 0f));

        Assert.Equal(8f, telemetry.PeakPlayerSpeed);
        Assert.Equal(32f, telemetry.PeakPullAcceleration);
        Assert.Equal(5f, telemetry.AveragePlayerSpeed);
        Assert.Equal(1UL, telemetry.TargetSelectionCount);
        Assert.Equal(1UL, telemetry.PullStartCount);
        Assert.Equal(1UL, telemetry.PullStopCount);
    }

    [Fact]
    public void ResetClearsDevelopmentMetrics()
    {
        var telemetry = new SimulationTelemetry();
        telemetry.Observe(State(10f, true, true, 32f));

        telemetry.Reset();

        Assert.Equal(0f, telemetry.PeakPlayerSpeed);
        Assert.Equal(0f, telemetry.PeakPullAcceleration);
        Assert.Equal(0f, telemetry.AveragePlayerSpeed);
        Assert.Equal(0UL, telemetry.TargetSelectionCount);
        Assert.Equal(0UL, telemetry.PullStartCount);
        Assert.Equal(0UL, telemetry.PullStopCount);
    }

    private static SimulationState State(
        float speed,
        bool hasTarget,
        bool pulling,
        float pullAcceleration)
    {
        var winch = hasTarget
            ? new WinchState(
                WinchTargetState.Selected,
                WinchPathState.AtWorldAnchor(new Vector3(0f, 0f, -5f)),
                pulling,
                5f,
                pullAcceleration)
            : WinchState.Initial;

        return SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Velocity = Vector3.UnitX * speed,
            },
            Winch = winch,
        };
    }
}
