namespace Hitch.Simulation.Winch;

/// <summary>
/// Snapshot-friendly gameplay state for the world-anchor winch.
/// </summary>
public readonly record struct WinchState(
    WinchAttachmentState Attachment,
    WinchPathState Path,
    float RestLength,
    float ReelVelocity,
    float ReattachCooldownRemaining,
    float LastActualDistance,
    float LastTensionAcceleration)
{
    public bool IsAttached => Attachment == WinchAttachmentState.Attached;

    public static WinchState Initial => new(
        WinchAttachmentState.Detached,
        default,
        0f,
        0f,
        0f,
        0f,
        0f);
}
