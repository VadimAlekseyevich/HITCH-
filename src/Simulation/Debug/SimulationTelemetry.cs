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

        PeakPlayerSpeed = MathF.Max(
            PeakPlayerSpeed,
            speed);
        PeakPullAcceleration = MathF.Max(
            PeakPullAcceleration,
            MathF.Max(
                state.Winch.LastPullAcceleration,
                state.SecondaryWinch.LastPullAcceleration));

        _speedSampleSum += speed;
        _speedSampleCount++;

        var hasAnyTarget =
            state.Winch.HasTarget
            || state.SecondaryWinch.HasTarget;
        var anyPulling =
            state.Winch.IsPulling
            || state.SecondaryWinch.IsPulling;

        if (hasAnyTarget && !_hadTarget)
        {
            TargetSelectionCount++;
        }

        if (anyPulling && !_wasPulling)
        {
            PullStartCount++;
        }
        else if (!anyPulling && _wasPulling)
        {
            PullStopCount++;
        }

        _hadTarget = hasAnyTarget;
        _wasPulling = anyPulling;
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
