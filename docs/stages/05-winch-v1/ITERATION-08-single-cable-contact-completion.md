# Stage 5 — Iteration 8: Single Cable + Capsule-Aware Completion

**Status: IMPLEMENTED — awaiting human playtest**

## Controls

- RMB shoots/replaces the single grapple cable.
- Pull starts immediately.
- LMB has no grapple action.
- Gameplay rope length is unlimited.

## Pull tuning

```text
pullInitialImpulse       24 m/s
pullRadialAcceleration   300 m/s²
pullTargetInwardSpeed    42 m/s
arrivalContactTolerance  0.06 m
```

## Completion bug fixed

The old implementation used a fixed center-to-anchor threshold of 0.9 m.

That can fail for a 1.8 m capsule at a ceiling because the capsule center cannot physically get closer than roughly 0.92 m once collision margin is included.

Result: the cable could stay active after the player visually reached the surface and continue producing small orbital/rotational movement.

Iteration 8 calculates completion distance from capsule geometry:

```text
capsule radius
+ projected capsule half-segment
+ collision margin
+ arrival contact tolerance
```

On completion:

1. cable clears;
2. full player velocity is set to zero;
3. ordinary gravity resumes in the following locomotion step.

## Technical verification

https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36810109190

- 56 tests passed / 0 failed;
- 0 warnings / 0 errors;
- stable spawn smoke;
- closed-room smoke passed;
- explicit regression test covers ceiling contact and removal of horizontal/orbital velocity.

Do not proceed to Stage 6 until a human playtest confirms the residual rotation is gone.
