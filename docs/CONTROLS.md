# HITCH! — Temporary Development Controls

These bindings exist only for the current greybox movement prototype.

They are **not** the final control scheme.

| Input | Current prototype action |
|---|---|
| Mouse | Look |
| W / A / S / D | Responsive ground movement + limited air steering |
| Space | Weak jump |
| Left mouse button | Throw/replace cable target; immediately cancels any active pull without removing momentum |
| Right mouse button click | Start automatic pull toward the currently selected cable point |
| F | Reserved melee intent for later combat work |
| Escape | Release mouse cursor |
| Left click while cursor is released | Recapture mouse only |

There is **no RMB hold/release gameplay state** in the current prototype.

Q/E reel controls remain disabled.

## Current pull loop

1. Aim using the small center crosshair and click LMB.
2. A cable/target point is created. It does not pull yet.
3. Click RMB once.
4. Pull starts immediately with a strong initial impulse and then continues automatically.
5. The player does **not** need to hold RMB.
6. Clicking LMB while pulling cancels the current pull immediately, preserves momentum, and throws a new idle cable.
7. If that LMB throw misses, the old cable is retracted/cleared and the player continues only by inertia.
8. The new cable does not pull until RMB is clicked again.
9. Reaching the target ends the pull automatically; inward motion into the anchor is removed and ordinary gravity/control resume.

## Current playtest range

The experimental grapple range is currently **72 m**, approximately 3× the previous 24 m value, to support the enlarged vertical movement lab.

## Architecture rule

Bindings live in the Godot-side input adapter.

Core simulation does **not** know about keyboard keys, mouse buttons, or mouse pixels. It receives hardware-independent `PlayerInput` actions.
