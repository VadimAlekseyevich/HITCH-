# HITCH! — MVP Tuning

Runtime movement tuning lives in:

```text
config/mvp_tuning.json
```

The Godot runner loads and validates this file at startup.

## Stage 5 current dual-cable iteration

The active prototype is intentionally action-oriented and inspired by the feel of dual-cable traversal systems rather than by a fully realistic rope simulation.

Controls:

- LMB owns the left cable;
- RMB owns the right cable;
- clicking a side raycasts/replaces that side and immediately starts pull;
- both cables can pull simultaneously.

## Rope range

There is currently **no gameplay rope-length tuning parameter**.

The old `grappleRange` setting has been removed.

A large finite ray endpoint exists only because the world-query API requires one. It is not a gameplay cap.

## Pull values

Current shared per-cable values:

```text
pullInitialImpulse       = 24 m/s
pullRadialAcceleration   = 300 m/s²
pullTargetInwardSpeed    = 42 m/s
arrivalDistance          = 0.9 m
```

Each active cable independently tries to establish inward radial speed toward its own anchor.

With two cables active, both velocity corrections are computed symmetrically from the same base velocity and then summed.

## Momentum rule

During pull:

- tangential momentum is preserved for swinging;
- radial speed toward each anchor is aggressively increased;
- faster existing inward speed is not clamped down.

At arrival:

- one arrived cable clears independently if the other is still active;
- when the last active cable finishes, all player velocity is cleared;
- gravity and ordinary locomotion resume.

## Base locomotion

Current responsive prototype values:

```text
groundMaxSpeed      = 6 m/s
groundAcceleration  = 80 m/s²
groundBraking       = 100 m/s²
airAcceleration     = 5 m/s²
airControlMaxSpeed  = 5 m/s
```

## Enclosed movement room

The Stage 5 test lab is physically enclosed:

- floor: about 120 × 150 m;
- four continuous perimeter walls;
- ceiling above the tallest test structures;
- multiple vertical towers/spires and aerial targets.

Headless smoke verifies that all four walls and the ceiling are present.

## Tick rate

`tickRateHz` remains editable (currently 60) so 60/120 Hz can still be compared later.

## Run

```text
build_and_run.bat
```
