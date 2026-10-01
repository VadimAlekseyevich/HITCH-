using System.Numerics;
using Hitch.Simulation.Debug;
using Hitch.Simulation.State;
using Hitch.Simulation.Winch;

namespace Hitch.Tests;

public sealed class SimulationTelemetryTests
{
    [Fact]
    public void TelemetryTracksPeaksAverageAndAttachmentTransitions()
    {
        var telemetry = new SimulationTelemetry();

        telemetry.Observe(State(
            speed: 3f,
            tension: 10f,
            attached: false));
        telemetry.Observe(State(
            speed: 5f,
            tension: 20f,
            attached: true));
        telemetry.Observe(State(
            speed: 4f,
            tension: 15f,
            attached: true));
        telemetry.Observe(State(
            speed: 2f,
            tension: 0f,
            attached: false));

        Assert.Equal(5f, telemetry.PeakPlayerSpeed);
        Assert.Equal(20f, telemetry.PeakWinchTensionAcceleration);
        Assert.Equal(3.5f, telemetry.AveragePlayerSpeed);
        Assert.Equal(1UL, telemetry.AttachCount);
        Assert.Equal(1UL, telemetry.DetachCount);
    }

    [Fact]
    public void ResetClearsDevelopmentMetrics()
    {
        var telemetry = new SimulationTelemetry();
        telemetry.Observe(State(10f, 30f, attached: true));

        telemetry.Reset();

        Assert.Equal(0f, telemetry.PeakPlayerSpeed);
        Assert.Equal(0f, telemetry.PeakWinchTensionAcceleration);
        Assert.Equal(0f, telemetry.AveragePlayerSpeed);
        Assert.Equal(0UL, telemetry.AttachCount);
        Assert.Equal(0UL, telemetry.DetachCount);
    }

    private static SimulationState State(
        float speed,
        float tension,
        bool attached)
    {
        var winch = attached
            ? new WinchState(
                WinchAttachmentState.Attached,
                WinchPathState.AtWorldAnchor(new Vector3(0f, 0f, -5f)),
                5f,
                0f,
                0f,
                5f,
                tension)
            : WinchState.Initial with
            {
                LastTensionAcceleration = tension,
            };

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
