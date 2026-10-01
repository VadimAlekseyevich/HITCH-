# HITCH! — MVP Tuning

Current local movement/winch tuning lives in:

```text
config/mvp_tuning.json
```

The Godot runner loads this file on startup, deserializes it into typed C# configuration, validates it, and uses the same values in simulation.

## Tick rate

```json
"tickRateHz": 60
```

Change this to `120` to compare the same prototype at 120 simulation/physics ticks per second.

The runner also sets Godot's physics tick rate to this value so engine queries and gameplay simulation remain aligned.

## Locomotion

Important fields:

- `gravity`
- `groundAcceleration`
- `groundMaxSpeed`
- `groundBraking`
- `jumpSpeed`
- `airAcceleration`
- `airControlMaxSpeed`

Base locomotion is intentionally weak. Do not tune it into a conventional fast arena-FPS controller unless the design is explicitly changed.

## Winch

Important Stage 5 experiment fields:

- `grappleRange`
- `reattachCooldownSeconds`
- `minimumRopeLength`
- `reelMaxSpeed`
- `reelAcceleration`
- `reelDeceleration`
- `springAccelerationPerMeter`
- `pretensionDistance`
- `outwardDampingPerSecond`
- `slackTakeUpSpeed`
- `reelFalloffStartSpeed`
- `reelFalloffEndSpeed`
- `minimumReelInMultiplier`

These values are **experimental tuning**, not final design truth.

## Current v1 spring interpretation

The Stage 5 model is intentionally simple:

- it is a one-sided spring;
- it may pull the player toward the anchor;
- it never pushes the player away from the anchor;
- extension increases inward acceleration;
- moving radially away increases damping tension;
- a small pretension distance keeps the line active close to rest length;
- obvious neutral/inward slack is automatically taken up;
- explicit reel-out may temporarily create controlled slack.

If human testing shows this model is wrong, change the model rather than endlessly tuning coefficients.

## One-click test

After editing the JSON:

```text
build_and_run.bat
```

No rebuild is technically required for JSON-only changes, but the one-click path remains the canonical way to verify and launch the current repository state.
