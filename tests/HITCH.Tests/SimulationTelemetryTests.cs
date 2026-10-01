using System.Numerics;
using Hitch.Simulation.Debug;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;

namespace Hitch.Tests;

public sealed class SimulationTelemetryTests
{
    [Fact]
    public void TelemetryTracksDualCableTransitionsAndPeaks()
    {
        var telemetry = new SimulationTelemetry();

        telemetry.Observe(State(
            speed: 3f,
            left: WinchCableState.Initial,
            right: WinchCableState.Initial));

        telemetry.Observe(State(
            speed: 5f,
            left: ActiveCable(new Vector3(-5f, 0f, -5f), 20f),
            right: WinchCableState.Initial));

        telemetry.Observe(State(
            speed: 8f,
            left: ActiveCable(new Vector3(-5f, 0f, -5f), 20f),
            right: ActiveCable(new Vector3(5f, 0f, -5f), 32f)));

        telemetry.Observe(State(
            speed: 4f,
            left: WinchCableState.Initial,
            right: ActiveCable(new Vector3(5f, 0f, -5f), 12f)));

        Assert.Equal(8f, telemetry.PeakPlayerSpeed);
        Assert.Equal(32f, telemetry.PeakPullAcceleration);
        Assert.Equal(2, telemetry.PeakActiveCables);
        Assert.Equal(5f, telemetry.AveragePlayerSpeed);
        Assert.Equal(2UL, telemetry.TargetSelectionCount);
        Assert.Equal(2UL, telemetry.PullStartCount);
        Assert.Equal(1UL, telemetry.PullStopCount);
    }

    [Fact]
    public void ResetClearsDevelopmentMetrics()
    {
        var telemetry = new SimulationTelemetry();
        telemetry.Observe(State(
            10f,
            ActiveCable(new Vector3(-5f, 0f, -5f), 30f),
            ActiveCable(new Vector3(5f, 0f, -5f), 40f)));

        telemetry.Reset();

        Assert.Equal(0f, telemetry.PeakPlayerSpeed);
        Assert.Equal(0f, telemetry.PeakPullAcceleration);
        Assert.Equal(0, telemetry.PeakActiveCables);
        Assert.Equal(0f, telemetry.AveragePlayerSpeed);
        Assert.Equal(0UL, telemetry.TargetSelectionCount);
        Assert.Equal(0UL, telemetry.PullStartCount);
        Assert.Equal(0UL, telemetry.PullStopCount);
    }

    private static SimulationState State(
        float speed,
        WinchCableState left,
        WinchCableState right) =>
        SimulationState.Initial with
        {
            Player = SimulationState.Initial.Player with
            {
                Velocity = Vector3.UnitX * speed,
            },
            Winch = new WinchState(left, right),
        };

    private static WinchCableState ActiveCable(
        Vector3 point,
        float pullAcceleration) =>
        new(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(point),
            true,
            Vector3.Distance(Vector3.Zero, point),
            pullAcceleration);
}
