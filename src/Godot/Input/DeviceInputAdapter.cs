using Godot;
using Hitch.Simulation.Input;
using NumericsVector2 = System.Numerics.Vector2;

namespace Hitch.GodotIntegration.Input;

/// <summary>
/// Temporary Stage 3 keyboard/mouse adapter.
///
/// Raw device state stops here. The simulation only receives PlayerInput values.
/// Mouse pixels are converted to radians before crossing the simulation boundary.
/// </summary>
internal sealed class DeviceInputAdapter
{
    private const float MouseRadiansPerPixel = 0.0025f;

    private Godot.Vector2 _accumulatedMousePixels;
    private PlayerButtons _pendingButtons;

    public bool IsMouseCaptured => Godot.Input.MouseMode == Godot.Input.MouseModeEnum.Captured;

    public void CaptureMouse()
    {
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        _accumulatedMousePixels = Godot.Vector2.Zero;
    }

    public void ReleaseMouse()
    {
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        _accumulatedMousePixels = Godot.Vector2.Zero;
    }

    /// <summary>
    /// Returns true when the event was intentionally consumed by the adapter.
    /// </summary>
    public bool HandleEvent(InputEvent @event)
    {
        if (@event is InputEventKey key
            && key.Pressed
            && !key.Echo
            && key.Keycode == Key.Escape)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouse();
            }

            return true;
        }

        if (!IsMouseCaptured)
        {
            if (@event is InputEventMouseButton
                {
                    Pressed: true,
                    ButtonIndex: MouseButton.Left,
                })
            {
                // First click after releasing the cursor only returns to gameplay.
                // It must not also fire a weapon.
                CaptureMouse();
                return true;
            }

            return false;
        }

        if (@event is InputEventMouseMotion motion)
        {
            _accumulatedMousePixels += motion.Relative;
            return true;
        }

        if (@event is InputEventMouseButton mouseButton
            && mouseButton.Pressed
            && mouseButton.ButtonIndex == MouseButton.Right)
        {
            _pendingButtons |= PlayerButtons.GrapplePullPressed;
            return true;
        }

        if (@event is InputEventKey keyEvent
            && keyEvent.Pressed
            && !keyEvent.Echo)
        {
            switch (keyEvent.PhysicalKeycode)
            {
                case Key.Space:
                    _pendingButtons |= PlayerButtons.JumpPressed;
                    return true;
                case Key.F:
                    _pendingButtons |= PlayerButtons.MeleePressed;
                    return true;
            }
        }

        return false;
    }

    public PlayerInput ConsumePhysicsTickInput()
    {
        var moveX =
            (Godot.Input.IsPhysicalKeyPressed(Key.D) ? 1f : 0f)
            - (Godot.Input.IsPhysicalKeyPressed(Key.A) ? 1f : 0f);
        var moveY =
            (Godot.Input.IsPhysicalKeyPressed(Key.W) ? 1f : 0f)
            - (Godot.Input.IsPhysicalKeyPressed(Key.S) ? 1f : 0f);

        var move = new NumericsVector2(moveX, moveY);
        if (move.LengthSquared() > 1f)
        {
            move = NumericsVector2.Normalize(move);
        }

        // Reel-in/out is intentionally disabled in the current direct-pull playtest.
        var reelAxis = 0f;

        // Mouse right/down are positive pixels. HITCH! maps them to negative yaw/pitch radians
        // so Godot's -Z forward camera follows conventional FPS mouse direction.
        var lookDelta = IsMouseCaptured
            ? new NumericsVector2(
                -_accumulatedMousePixels.X * MouseRadiansPerPixel,
                -_accumulatedMousePixels.Y * MouseRadiansPerPixel)
            : NumericsVector2.Zero;

        var result = new PlayerInput(
            move,
            lookDelta,
            reelAxis,
            _pendingButtons);

        _accumulatedMousePixels = Godot.Vector2.Zero;
        _pendingButtons = PlayerButtons.None;

        return result;
    }
}
