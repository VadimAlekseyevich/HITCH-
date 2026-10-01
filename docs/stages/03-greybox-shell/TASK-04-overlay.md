# Task 04 — Add Debug Overlay

**Status:** PLANNED  
**Depends on:** Tasks 01–03

## Goal

Make simulation/input state visible while iterating.

## Show at minimum

- simulation tick;
- tick rate;
- position;
- velocity;
- scalar speed;
- grounded flag;
- move input;
- reel input;
- current yaw/pitch;
- mouse capture state.

This is developer UI, not final HUD.

Avoid updating expensive strings every render frame when a lower refresh rate is sufficient.
