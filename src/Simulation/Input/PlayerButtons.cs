namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,

    /// <summary>
    /// RMB click: raycast/replace the single grapple cable and immediately start pulling.
    /// </summary>
    GrapplePullPressed = 1 << 1,

    // Reserved for later combat stages.
    FirePressed = 1 << 2,
    MeleePressed = 1 << 3,
}
