# HITCH! — Temporary Development Controls

These bindings exist only for the current greybox movement prototype.

They are **not** the final control scheme.

| Input | Current prototype action |
|---|---|
| Mouse | Look |
| W / A / S / D | Ground/air movement intent |
| Space | Weak jump |
| Left mouse button | Select or replace grapple target point |
| Hold right mouse button | Pull directly toward selected point |
| Release right mouse button | Stop pull immediately; keep current momentum |
| F | Reserved melee intent for later combat work |
| Escape | Release mouse cursor |
| Left click while cursor is released | Recapture mouse only |

Q/E reel controls are intentionally disabled in the current Stage 5 iteration.

## Current pull loop

1. Aim at a valid surface and click LMB.
2. A target point is stored, but no pull happens yet.
3. Hold RMB to pull immediately toward that point.
4. Release RMB early to stop the force and continue with current momentum.
5. If the player reaches the target, the target is consumed, velocity is cleared, and gravity resumes.
6. Clicking LMB on another valid surface while RMB is held immediately retargets the pull.

## Architecture rule

Bindings live in the Godot-side input adapter.

Core simulation does **not** know about keyboard keys, mouse buttons, or mouse pixels. It receives hardware-independent `PlayerInput` actions.
