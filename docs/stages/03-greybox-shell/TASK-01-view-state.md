# Task 01 — Add Simulation-Owned First-Person View State

**Status:** DONE

## Goal

Represent gameplay-facing yaw/pitch explicitly in `PlayerState` and update it from per-tick `PlayerInput.LookDelta`.

## Requirements

- keep body orientation separate from view yaw/pitch;
- pitch limit is configurable/validated;
- yaw wraps safely;
- simulation input contains radians, not raw mouse pixels;
- tests cover yaw/pitch updates and pitch clamping.

## Non-scope

- movement direction;
- physical impact rotation behavior;
- camera shake;
- sensitivity/user settings.
