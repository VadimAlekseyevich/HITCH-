# HITCH! — MVP Tuning

Runtime movement tuning lives in:

```text
config/mvp_tuning.json
```

The Godot runner loads and validates this file at startup.

## Stage 5 current single-cable iteration

Current controls:

- RMB shoots/replaces the single cable;
- pull starts immediately;
- RMB does not need to be held;
- repeated RMB clicks retarget immediately.

## Rope range

There is currently **no gameplay rope-length tuning parameter**.

The old `grappleRange` setting is removed.

A large finite ray endpoint exists only because the world-query API requires one. It is not a gameplay cap.

## Pull values

```text
pullInitialImpulse        = 24 m/s
pullRadialAcceleration    = 300 m/s²
pullTargetInwardSpeed     = 42 m/s
arrivalContactTolerance   = 0.06 m
```

During pull:

- tangential velocity is preserved for swing;
- radial velocity toward the anchor is aggressively increased;
- faster existing inward speed is not clamped down.

## Capsule-aware completion

Do **not** use a fixed center-to-anchor arrival distance.

The exact center distance at physical surface contact depends on capsule geometry and approach direction.

Current completion distance is based on:

```text
capsule radius
+ projected capsule half-segment length
+ collision margin
+ arrivalContactTolerance
```

With current player dimensions this is approximately:

- horizontal wall: ~0.53 m from anchor;
- ceiling/floor direction: ~0.98 m from anchor.

This fixes the case where the capsule was already physically against a ceiling but the old fixed `0.9 m` threshold kept the cable active and produced residual orbiting/rotation.

At completed pull:

- velocity is fully cleared;
- cable ends;
- ordinary gravity then resumes.

## Base locomotion

```text
groundMaxSpeed      = 6 m/s
groundAcceleration  = 80 m/s²
groundBraking       = 100 m/s²
airAcceleration     = 5 m/s²
airControlMaxSpeed  = 5 m/s
```

## Enclosed movement room

The Stage 5 test lab is physically enclosed:

- floor about 120 × 150 m;
- four continuous perimeter walls;
- ceiling above the tallest test structures;
- multiple vertical towers/spires and aerial targets.

Headless smoke verifies all four walls and the ceiling.

## Tick rate

`tickRateHz` remains editable (currently 60) so 60/120 Hz can still be compared later.

## Run

```text
build_and_run.bat
```
