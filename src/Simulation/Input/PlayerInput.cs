using System.Numerics;

namespace Hitch.Simulation.Input;

/// <summary>
/// Gameplay intent for exactly one simulation tick.
///
/// Device adapters are responsible for translating keyboard/mouse/gamepad state into this value.
/// Button flags represent the intent for this tick; this type is not a raw hardware snapshot and
/// intentionally does not define a network wire format.
/// </summary>
public readonly record struct PlayerInput(
    Vector2 Move,
    Vector2 LookDelta,
    float ReelAxis,
    PlayerButtons Buttons)
{
    public static PlayerInput Neutral => new(
        Vector2.Zero,
        Vector2.Zero,
        0f,
        PlayerButtons.None);

    public bool Has(PlayerButtons button) => (Buttons & button) != 0;
}
