# Task 02 — Add Godot Device Input Adapter and First-Person Rig

**Status:** PLANNED  
**Depends on:** Task 01

## Goal

Translate keyboard/mouse events into `PlayerInput` and present simulation view state through a first-person Godot rig.

## Requirements

- raw device APIs stay in Godot-side code;
- use physical WASD polling for movement intent;
- mouse motion becomes radians before entering simulation;
- mouse capture toggles with Escape;
- first click recaptures the mouse without triggering gameplay;
- camera yaw/pitch is driven from simulation state;
- temporary bindings are clearly documented.

## Non-scope

- locomotion;
- remapping UI;
- gamepad;
- final controls.
