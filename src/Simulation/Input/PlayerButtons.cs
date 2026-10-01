namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,

    /// <summary>
    /// LMB: cancel any active pull and select/replace the grapple point.
    /// </summary>
    SelectGrapplePointPressed = 1 << 1,

    /// <summary>
    /// RMB click: start automatic pull toward the selected point.
    /// No hold/release state is required.
    /// </summary>
    PullPressed = 1 << 2,

    // Reserved for later combat stages.
    FirePressed = 1 << 3,
    MeleePressed = 1 << 4,
}
