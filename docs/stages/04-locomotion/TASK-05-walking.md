# Task 05 — Add Slow View-Relative Walking

**Status:** DONE  
**Depends on:** Task 04

## Goal

Translate `PlayerInput.Move` into intentionally weak ground locomotion.

## Requirements

- input direction is relative to view yaw, not pitch;
- diagonal input remains normalized;
- configurable acceleration;
- configurable low maximum ground speed;
- no instant velocity replacement;
- no render-FPS dependency;
- tests cover direction and acceleration behavior.

## Non-scope

- sprint;
- crouch;
- acceleration curves for final game feel.
