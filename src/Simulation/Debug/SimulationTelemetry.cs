using Hitch.Simulation.State;

namespace Hitch.Simulation.Debug;

/// <summary>
/// Development-only in-memory movement metrics.
/// </summary>
public sealed class SimulationTelemetry
{
    private double _speedSampleSum;
    private ulong _speedSampleCount;
    private bool _hadTarget;
    private bool _wasPulling;

    public float PeakPlayerSpeed { get; private set; }

    public float PeakPullAcceleration { get; private set; }

    public ulong TargetSelectionCount { get; private set; }

    public ulong PullStartCount { get; private set; }

    public ulong PullStopCount { get; private set; }

    public float AveragePlayerSpeed =>
        _speedSampleCount == 0
            ? 0f
            : (float)(_speedSampleSum / _speedSampleCount);

    public void Observe(in SimulationState state)
    {
        var speed = state.Player.Velocity.Length();

        PeakPlayerSpeed = MathF.Max(PeakPlayerSpeed, speed);
        PeakPullAcceleration = MathF.Max(
            PeakPullAcceleration,
            state.Winch.LastPullAcceleration);

        _speedSampleSum += speed;
        _speedSampleCount++;

        if (state.Winch.HasTarget && !_hadTarget)
        {
            TargetSelectionCount++;
        }

        if (state.Winch.IsPulling && !_wasPulling)
        {
            PullStartCount++;
        }
        else if (!state.Winch.IsPulling && _wasPulling)
        {
            PullStopCount++;
        }

        _hadTarget = state.Winch.HasTarget;
        _wasPulling = state.Winch.IsPulling;
    }

    public void Reset()
    {
        PeakPlayerSpeed = 0f;
        PeakPullAcceleration = 0f;
        TargetSelectionCount = 0;
        PullStartCount = 0;
        PullStopCount = 0;
        _speedSampleSum = 0d;
        _speedSampleCount = 0;
        _hadTarget = false;
        _wasPulling = false;
    }
}
