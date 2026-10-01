using Hitch.Simulation.State;

namespace Hitch.Simulation.Debug;

/// <summary>
/// Development-only in-memory movement metrics.
/// </summary>
public sealed class SimulationTelemetry
{
    private double _speedSampleSum;
    private ulong _speedSampleCount;
    private bool _leftHadTarget;
    private bool _rightHadTarget;
    private bool _leftWasPulling;
    private bool _rightWasPulling;

    public float PeakPlayerSpeed { get; private set; }

    public float PeakPullAcceleration { get; private set; }

    public int PeakActiveCables { get; private set; }

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
        var left = state.Winch.Left;
        var right = state.Winch.Right;

        PeakPlayerSpeed = MathF.Max(
            PeakPlayerSpeed,
            speed);
        PeakPullAcceleration = MathF.Max(
            PeakPullAcceleration,
            MathF.Max(
                left.LastPullAcceleration,
                right.LastPullAcceleration));
        PeakActiveCables = Math.Max(
            PeakActiveCables,
            state.Winch.ActiveCableCount);

        _speedSampleSum += speed;
        _speedSampleCount++;

        ObserveCable(
            left,
            ref _leftHadTarget,
            ref _leftWasPulling);
        ObserveCable(
            right,
            ref _rightHadTarget,
            ref _rightWasPulling);
    }

    public void Reset()
    {
        PeakPlayerSpeed = 0f;
        PeakPullAcceleration = 0f;
        PeakActiveCables = 0;
        TargetSelectionCount = 0;
        PullStartCount = 0;
        PullStopCount = 0;
        _speedSampleSum = 0d;
        _speedSampleCount = 0;
        _leftHadTarget = false;
        _rightHadTarget = false;
        _leftWasPulling = false;
        _rightWasPulling = false;
    }

    private void ObserveCable(
        in Hitch.Simulation.Winch.WinchCableState cable,
        ref bool hadTarget,
        ref bool wasPulling)
    {
        if (cable.HasTarget && !hadTarget)
        {
            TargetSelectionCount++;
        }

        if (cable.IsPulling && !wasPulling)
        {
            PullStartCount++;
        }
        else if (!cable.IsPulling && wasPulling)
        {
            PullStopCount++;
        }

        hadTarget = cable.HasTarget;
        wasPulling = cable.IsPulling;
    }
}
