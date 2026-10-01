# Stage 5 — Iteration 14: Explosive Surge + Space Detach

**Status: IMPLEMENTED — awaiting human playtest**

## Goal

Make grapple firing feel like an unmistakable launch event rather than a modest speed multiplier.

The requested shape is intentionally non-monotonic at the start:

```text
RMB
 ↓
already-fast immediate launch
 ↓
very short second-stage surge
 ↓
large peak
 ↓
natural decay
 ↓
sustained high-speed pull
```

## Speed envelope

Sustained distance-based speed remains:

- 90 m/s short/medium;
- up to 160 m/s long-range.

Fresh grapple/retarget envelope:

- t = 0: **1.75×**;
- t ≈ 0.10 s: **2.75× peak**;
- then decay to 1.0× over **0.70 s**.

Maximum long-range example:

- immediate: 160 × 1.75 = **280 m/s**;
- peak: 160 × 2.75 = **440 m/s**;
- sustained: **160 m/s**.

This is intentionally excessive for human feel testing.

## Detach control

Space now means:

- jump if no grapple exists;
- explicit detach if a grapple exists.

On detach:

- cable state becomes empty;
- current velocity is preserved;
- the Space press does not also trigger jump;
- ordinary gravity and air movement resume afterward.

The intended movement technique is now possible:

```text
grapple → launch → surge → Space detach → free-flight carry → retarget
```

## Important distinction

The free-flight momentum after Space is intentional.

The old rejected inertia problem was different: carried momentum was fighting the active grapple while the cable was still pulling.

Iteration 14 still guarantees that while a grapple is active, its direct velocity model owns the movement direction.

## Human gate focus

Check:

1. whether the second-stage surge is immediately obvious;
2. whether 440 m/s peak is exciting rather than unreadable;
3. whether timing Space near the peak creates satisfying movement tech;
4. whether Space cleanly detaches without accidental jump;
5. whether free-flight momentum after detach feels useful without recreating the old active-grapple drift problem.


### Iteration 14 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36918044665

- build: success;
- warnings/errors: 0 / 0;
- tests: 72 passed / 0 failed;
- two-stage launch curve covered by automated tests;
- explicit detach preserves velocity;
- Space detach is consumed instead of also triggering jump;
- spawn smoke: stable and grounded;
- room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual Stage 5 gate.
