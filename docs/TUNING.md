# HITCH! — MVP Tuning

Runtime movement tuning lives in:

```text
config/mvp_tuning.json
```

The Godot runner loads and validates this file at startup.

## Stage 5 iteration 3

Iteration 2 misunderstood the requested controls by requiring RMB to be held.

The current active prototype is:

- LMB throws/selects a cable point and cancels any previous pull;
- RMB is a **single click** that starts automatic pull;
- pull starts with an immediate velocity impulse;
- continuous pull acceleration then bends the existing trajectory toward the anchor;
- tangential momentum is preserved instead of replacing the full velocity vector;
- LMB during pull stops the force and preserves inertia before placing a new idle cable;
- arrival stops the pull automatically.

Current winch fields:

- `grappleRange` — **72 m** in the current playtest;
- `grappleCollisionMask`;
- `pullInitialImpulse` — immediate delta-velocity on RMB click;
- `pullAcceleration` — continuous acceleration while automatic pull is active;
- `arrivalDistance`.

Current values:

```text
grappleRange        = 72 m
pullInitialImpulse  = 18 m/s
pullAcceleration    = 32 m/s²
arrivalDistance     = 0.9 m
```

These are playtest values, not final game balance.

## Locomotion

The first playtests reported slow/floaty/ice-like ordinary control.

Current values intentionally favor responsiveness:

```text
groundMaxSpeed      = 6 m/s
groundAcceleration  = 80 m/s²
groundBraking       = 100 m/s²
airAcceleration     = 5 m/s²
airControlMaxSpeed  = 5 m/s
```

The winch remains much more powerful than walking, but ordinary WASD should no longer feel sluggish.

## Movement lab

The engineering arena is temporarily larger and much more vertical:

- 120 × 150 m floor;
- several 44–72 m towers/spires;
- bridges and sky bars at multiple heights;
- small aerial stepping targets;
- long-range thin grapple targets.

This is test geometry, not the final MVP arena.

## Tick rate

`tickRateHz` remains editable (currently 60) so 60/120 Hz can still be compared later.

## Run

```text
build_and_run.bat
```
