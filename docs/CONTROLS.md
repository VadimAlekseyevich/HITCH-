# HITCH! — Temporary Development Controls

These bindings exist only for the current greybox movement prototype.

They are **not** the final control scheme.

| Input | Current prototype action |
|---|---|
| Mouse | Look |
| W / A / S / D | Responsive ground movement + limited air steering |
| Space | Weak jump |
| Right mouse button | Shoot/replace the single grapple cable and immediately start pulling |
| F | Reserved melee intent for later combat work |
| Escape | Release mouse cursor |
| Left click while cursor is released | Recapture mouse only |

LMB has no grapple action in the active prototype.

Q/E reel controls remain disabled.

## Current grapple loop

1. Aim with the center crosshair.
2. Click RMB.
3. A hitscan grapple immediately selects the surface under the crosshair.
4. Any previous cable is replaced.
5. Pull begins in the same simulation tick.
6. No button hold is required.
7. Clicking RMB again immediately retargets the cable.
8. A miss clears the cable and preserves current momentum.

## Rope length

There is **no gameplay rope-length limit**.

The physics API still needs a finite ray endpoint internally, so the implementation uses a very large engine-only query distance. It must not be treated as gameplay range.

## Pull completion

During travel, tangential momentum is preserved so swinging/curved trajectories remain possible.

When the body actually reaches the grapple surface:

- pull ends;
- the cable clears;
- all residual velocity is cleared;
- ordinary gravity/locomotion resume.

Completion is **capsule-aware**. It accounts for the player's collision shape, so a ceiling/wall can end the pull even though the capsule center cannot physically reach the exact surface hit point.

This specifically prevents the rejected behavior where the player appeared fully reeled in but continued orbiting/rotating around the anchor.

## Architecture rule

Bindings live in the Godot-side input adapter.

Core simulation does **not** know about keyboard keys, mouse buttons, or mouse pixels. It receives hardware-independent `PlayerInput` actions.
