namespace Hitch.Simulation.Input;

[Flags]
public enum PlayerButtons : ushort
{
    None = 0,
    JumpPressed = 1 << 0,
    GrapplePressed = 1 << 1,
    GrappleReleased = 1 << 2,
    FirePressed = 1 << 3,
    MeleePressed = 1 << 4,
}
