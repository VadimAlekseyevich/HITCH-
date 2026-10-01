namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,

    /// <summary>
    /// Current movement prototype: left click selects/replaces the grapple target point.
    /// </summary>
    SelectGrapplePointPressed = 1 << 1,

    /// <summary>
    /// Current movement prototype: right mouse button started being held.
    /// </summary>
    PullPressed = 1 << 2,

    /// <summary>
    /// Current movement prototype: right mouse button was released.
    /// </summary>
    PullReleased = 1 << 3,

    // Reserved for the later combat stage. No mouse binding currently emits this.
    FirePressed = 1 << 4,

    MeleePressed = 1 << 5,
}
