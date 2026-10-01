# Stage 5 — Iteration 3: Click-to-Start Automatic Pull

**Status: IMPLEMENTED — awaiting human playtest**

## Human correction

Iteration 2 incorrectly interpreted RMB as a hold action.

The requested prototype is:

- **LMB:** throw/place/replace cable point;
- **RMB click once:** start automatic pull;
- no RMB hold/release gameplay state;
- **LMB during pull:** cancel current pull immediately and preserve momentum;
- newly placed cable is idle until RMB is clicked again.

## Pull model

Starting automatic pull:

1. adds an immediate `pullInitialImpulse` toward the anchor;
2. then applies `pullAcceleration` every fixed tick toward the anchor;
3. preserves tangential/existing momentum instead of replacing the full velocity vector.

Arrival:

- stops automatic pull;
- clears the cable target;
- removes only velocity still pointing into the anchor;
- ordinary gravity and player control resume.

## Current tuning

```text
grappleRange        72 m
pullInitialImpulse  18 m/s
pullAcceleration    32 m/s²
arrivalDistance     0.9 m

groundMaxSpeed      6 m/s
groundAcceleration  80 m/s²
groundBraking       100 m/s²
airAcceleration     5 m/s²
airControlMaxSpeed  5 m/s
```

## Movement lab changes

- floor enlarged to 120 × 150 m;
- multiple 44–72 m vertical spires/towers;
- elevated bridges and sky bars;
- aerial stepping targets;
- thin long-range grapple targets;
- small center-screen crosshair dot.

## Technical verification

GitHub Actions:

https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36805953261

- build: success;
- 57 tests passed, 0 failed;
- 0 warnings / 0 errors;
- Godot headless spawn smoke: success.

Do not proceed to Stage 6 until a human playtest evaluates this control model.
