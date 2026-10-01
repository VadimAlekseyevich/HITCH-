namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,

    /// <summary>
    /// RMB click: raycast a new grapple point and immediately start pulling toward it.
    /// Replaces any previous cable/pull in the same simulation tick.
    /// </summary>
    GrapplePullPressed = 1 << 1,

    // Reserved for later combat stages.
    FirePressed = 1 << 2,
    MeleePressed = 1 << 3,
}
