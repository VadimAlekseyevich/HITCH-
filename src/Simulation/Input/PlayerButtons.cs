namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,

    /// <summary>
    /// LMB click: raycast/replace the left cable and immediately start pulling.
    /// </summary>
    LeftGrapplePressed = 1 << 1,

    /// <summary>
    /// RMB click: raycast/replace the right cable and immediately start pulling.
    /// </summary>
    RightGrapplePressed = 1 << 2,

    // Reserved for later combat stages.
    FirePressed = 1 << 3,
    MeleePressed = 1 << 4,
}
