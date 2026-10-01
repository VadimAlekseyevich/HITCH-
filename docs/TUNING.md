# HITCH! — MVP Tuning

Runtime movement tuning lives in:

```text
config/mvp_tuning.json
```

The Godot runner loads and validates this file at startup.

## Stage 5 iteration 5

The active prototype now uses a single grapple input:

- RMB raycasts a fresh target;
- the previous cable is replaced immediately;
- pull begins in the same tick;
- there is no separate cable-placement button;
- repeated RMB clicks retarget and pull immediately;
- a missed RMB shot clears the previous cable;
- tangential momentum remains preserved.

Current winch fields:

- `grappleRange`;
- `grappleCollisionMask`;
- `pullInitialImpulse`;
- `pullRadialAcceleration`;
- `pullTargetInwardSpeed`;
- `arrivalDistance`.

Current values:

```text
grappleRange        = 72 m
pullInitialImpulse  = 30 m/s
pullRadialAcceleration = 420 m/s²
pullTargetInwardSpeed  = 55 m/s
arrivalDistance     = 0.9 m
```

Iteration 5 changes the force model, not just the coefficient:

- tangential velocity is preserved for swinging;
- the cable explicitly drives the radial component toward the anchor;
- it tries to establish at least 55 m/s inward radial speed;
- radial speed can change at up to 420 m/s²;
- already-faster inward radial speed is not clamped down.

This is meant to make cable length visibly contract even when the player has strong sideways or outward momentum.

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
