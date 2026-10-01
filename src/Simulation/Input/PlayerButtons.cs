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

    /// <summary>
    /// Space while a grapple exists: fully detach while preserving current flight velocity.
    /// Device input may emit this together with JumpPressed; simulation suppresses the jump
    /// when the same Space press is consumed as a detach.
    /// </summary>
    GrappleDetachPressed = 1 << 2,

    // Reserved for later combat stages.
    FirePressed = 1 << 3,
    MeleePressed = 1 << 4,
}
