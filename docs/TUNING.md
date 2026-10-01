# HITCH! — MVP Tuning

Runtime movement tuning lives in:

```text
config/mvp_tuning.json
```

The Godot runner loads and validates this file at startup.

## Current Stage 5 iteration

The previous elastic spring + reel motor experiment failed the first human movement gate.

The active prototype is intentionally simpler:

- LMB selects a point;
- RMB directly pulls toward it;
- pull speed is immediate rather than accelerated;
- reaching the point consumes it and drops the player;
- releasing RMB early preserves current momentum;
- retargeting with LMB works while pulling.

Current winch fields:

- `grappleRange`
- `grappleCollisionMask`
- `pullSpeed`
- `arrivalDistance`

The removed spring/reel parameters are not part of the active prototype.

## Ground locomotion

Important fields:

- `groundAcceleration`
- `groundMaxSpeed`
- `groundBraking`
- `gravity`
- `jumpSpeed`
- `airAcceleration`
- `airControlMaxSpeed`

Ground control now moves horizontal velocity toward the desired movement velocity and brakes excess velocity on the ground. This was changed after the first playtest reported an ice-like/slippery feel.

## Tick rate

`tickRateHz` remains editable (currently 60) so 60/120 Hz can still be compared later.

## Run

```text
build_and_run.bat
```
