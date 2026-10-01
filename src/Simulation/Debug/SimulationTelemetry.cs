using Hitch.Simulation.State;

namespace Hitch.Simulation.Debug;

/// <summary>
/// Development-only in-memory metrics for local movement tuning.
///
/// This is intentionally not part of authoritative SimulationState and is not a production
/// analytics system.
/// </summary>
public sealed class SimulationTelemetry
{
    private double _speedSampleSum;
    private ulong _speedSampleCount;
    private bool _wasAttached;

    public float PeakPlayerSpeed { get; private set; }

    public float PeakWinchTensionAcceleration { get; private set; }

    public ulong AttachCount { get; private set; }

    public ulong DetachCount { get; private set; }

    public float AveragePlayerSpeed =>
        _speedSampleCount == 0
            ? 0f
            : (float)(_speedSampleSum / _speedSampleCount);

    public void Observe(in SimulationState state)
    {
        var speed = state.Player.Velocity.Length();

        PeakPlayerSpeed = MathF.Max(PeakPlayerSpeed, speed);
        PeakWinchTensionAcceleration = MathF.Max(
            PeakWinchTensionAcceleration,
            state.Winch.LastTensionAcceleration);

        _speedSampleSum += speed;
        _speedSampleCount++;

        var attached = state.Winch.IsAttached;
        if (attached && !_wasAttached)
        {
            AttachCount++;
        }
        else if (!attached && _wasAttached)
        {
            DetachCount++;
        }

        _wasAttached = attached;
    }

    public void Reset()
    {
        PeakPlayerSpeed = 0f;
        PeakWinchTensionAcceleration = 0f;
        AttachCount = 0;
        DetachCount = 0;
        _speedSampleSum = 0d;
        _speedSampleCount = 0;
        _wasAttached = false;
    }
}
