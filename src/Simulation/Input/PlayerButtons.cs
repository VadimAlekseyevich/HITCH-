namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,

    /// <summary>
    /// RMB click: shoot or replace the single grapple cable. Does not start reel-in.
    /// </summary>
    GrappleShootPressed = 1 << 1,

    /// <summary>
    /// Space while a grapple exists: fully detach while preserving current flight velocity.
    /// Device input may emit this together with JumpPressed; simulation suppresses the jump
    /// when the same Space press is consumed as a detach.
    /// </summary>
    GrappleDetachPressed = 1 << 2,

    /// <summary>
    /// LMB click: start automatic reel-in for the already attached cable.
    /// Repeated clicks while already reeling do not restart the launch envelope.
    /// </summary>
    GrappleReelPressed = 1 << 3,

    // Reserved for later combat stages.
    FirePressed = 1 << 4,
    MeleePressed = 1 << 5,
}
