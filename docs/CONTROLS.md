# HITCH! — Temporary Development Controls

These bindings exist only for the current greybox movement prototype.

They are **not** the final control scheme.

| Input | Current prototype action |
|---|---|
| Mouse | Look |
| W / A / S / D | Responsive ground movement + limited air steering |
| Space | Weak jump |
| Left mouse button | Shoot/replace the **left** cable and immediately start its pull |
| Right mouse button | Shoot/replace the **right** cable and immediately start its pull |
| F | Reserved melee intent for later combat work |
| Escape | Release mouse cursor |
| Left click while cursor is released | Recapture mouse only; does not fire a cable |

Q/E reel controls remain disabled.

## Dual-cable loop

Each mouse button owns one independent cable.

### LMB

- raycasts a new left anchor;
- replaces only the previous left cable;
- starts left-cable pull in the same simulation tick;
- a miss clears only the left cable.

### RMB

- raycasts a new right anchor;
- replaces only the previous right cable;
- starts right-cable pull in the same simulation tick;
- a miss clears only the right cable.

Both cables may be active simultaneously.

When both are active:

- both pull contributions are applied in the same fixed tick;
- left/right processing is symmetric;
- tangential momentum remains available for swinging;
- the result is intentionally more action-oriented than a realistic rope simulation.

## Rope length

There is **no gameplay rope-length limit** in the current prototype.

The simulation ray query uses a very large finite endpoint internally because the physics API requires one. That number is an engine implementation detail, not a gameplay range.

Any visible valid surface in the enclosed Stage 5 movement room should be reachable.

## Arrival behavior

Each cable ends independently when its anchor is reached.

- If one cable finishes while the other still pulls, only that cable clears.
- When the **last active cable** finishes, player velocity is cleared completely.
- Ordinary gravity/locomotion then resume.

This avoids endless orbiting after a completed pull while still allowing two-cable traversal.

## Architecture rule

Bindings live in the Godot-side input adapter.

Core simulation does **not** know about keyboard keys, mouse buttons, or mouse pixels. It receives hardware-independent `PlayerInput` actions.
