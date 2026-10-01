# HITCH! — Temporary Development Controls

These bindings exist only for the current greybox prototype.

They are **not** a final control scheme and there is no remapping UI yet.

| Input | Temporary action |
|---|---|
| Mouse | Look |
| W / A / S / D | Movement intent |
| Space | Jump intent |
| Left mouse button | Fire intent |
| Right mouse button | Grapple press/release intent |
| F | Melee intent |
| E | Reel in (+1 axis) |
| Q | Reel out (-1 axis) |
| Escape | Release mouse cursor |
| Left click while cursor is released | Recapture mouse only; does not fire |

## Architecture rule

The bindings above live in the Godot-side device adapter.

Core simulation does **not** know about keyboard keys, mouse buttons, or mouse pixels. It receives hardware-independent `PlayerInput` values, with mouse motion already converted to radians.

When bindings change, update this file and the Godot adapter. Do not introduce raw device APIs into the simulation assembly.
