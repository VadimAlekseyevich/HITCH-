# HITCH! — Temporary Development Controls

These bindings exist only for the current greybox movement prototype.

They are **not** the final control scheme.

| Input | Current prototype action |
|---|---|
| Mouse | Look |
| W / A / S / D | Responsive ground movement + limited air steering |
| Space | Weak jump |
| Right mouse button click | Raycast a new grapple point and immediately start pulling toward it |
| F | Reserved melee intent for later combat work |
| Escape | Release mouse cursor |
| Left click while cursor is released | Recapture mouse only |

LMB has no grapple action in the active Stage 5 iteration.

Q/E reel controls remain disabled.

## Current pull loop

1. Aim using the small center crosshair.
2. Click RMB once.
3. The game raycasts immediately.
4. On a valid hit, the old cable is replaced, a new cable is created, and pull starts in the same simulation tick.
5. Pull begins with a strong immediate impulse and then continues automatically with acceleration toward the anchor.
6. The player does not hold RMB.
7. Clicking RMB again while already pulling immediately shoots/replaces the cable and starts pulling toward the new hit point.
8. If RMB misses, the old cable is cleared and the player continues with existing momentum.
9. Reaching the anchor ends the pull automatically; inward motion into the surface is removed while tangential momentum remains.

## Current playtest tuning

- grapple range: **72 m**
- initial pull impulse: **30 m/s**
- continuous pull acceleration: **60 m/s²**

These values are experimental.

## Architecture rule

Bindings live in the Godot-side input adapter.

Core simulation does **not** know about keyboard keys, mouse buttons, or mouse pixels. It receives hardware-independent `PlayerInput` actions.
