namespace Hitch.Simulation.Winch;

/// <summary>
/// Snapshot-friendly dual-cable grapple state.
///
/// Left and right cables are fully independent gameplay channels.
/// </summary>
public readonly record struct WinchState(
    WinchCableState Left,
    WinchCableState Right)
{
    public bool HasAnyTarget =>
        Left.HasTarget || Right.HasTarget;

    public bool IsPulling =>
        Left.IsPulling || Right.IsPulling;

    public int ActiveCableCount =>
        (Left.IsPulling ? 1 : 0)
        + (Right.IsPulling ? 1 : 0);

    public static WinchState Initial => new(
        WinchCableState.Initial,
        WinchCableState.Initial);
}
