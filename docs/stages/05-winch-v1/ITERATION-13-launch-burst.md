# Stage 5 — Iteration 13: Launch Burst

**Status: IMPLEMENTED — awaiting human playtest**

## Goal

Make every fresh grapple feel like a strong launch rather than immediately entering a flat travel speed.

The requested shape is:

```text
RMB / retarget
    ↓
strong speed spike
    ↓
smooth decay while pulling
    ↓
normal high-speed grapple travel
```

This is a controlled speed envelope, not physical inertia.

## Active profile

Distance still determines the sustained direct-pull speed:

- short/medium sustained speed: **90 m/s**;
- long sustained speed: up to **160 m/s**;
- full long-range speed around **250 m** anchor distance.

A fresh pull then applies a temporal multiplier:

- launch multiplier: **1.45×**;
- decay time: **0.75 s**;
- decay curve: smoothstep from 1.45× back to 1.0×.

Approximate launch values:

- 90 m/s base → **130.5 m/s** launch;
- 160 m/s base → **232 m/s** launch.

## State behavior

`WinchState.PullElapsedSeconds` tracks the age of the current pull.

- new grapple: timer starts at zero;
- continued pull: timer advances every simulation tick;
- retarget: a new winch state resets the timer to zero;
- latch/release does not carry old launch energy into future travel.

This remains deterministic simulation state suitable for future prediction/replay.

## Zero-inertia rules remain

- old velocity is discarded while pulling;
- velocity always points directly toward the active anchor;
- gravity and air-control are not mixed into active pull;
- walls/ceilings hard-stop;
- arrival latches at zero velocity;
- RMB miss releases with zero carried grapple velocity.

## Human gate focus

Check whether:

1. firing a fresh cable now gives a satisfying immediate kick;
2. the speed falloff is clearly perceptible but not sluggish;
3. chaining rapid retargets feels exciting because each new cable gets a fresh kick;
4. 1.45× is strong enough without making close-range grapples unreadable;
5. the decay time of 0.75 s feels natural for the intended ODM-like rhythm.


### Iteration 13 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36916365526

- build: success;
- warnings/errors: 0 / 0;
- tests: 69 passed / 0 failed;
- burst-decay curve covered by automated tests;
- pull elapsed time advances deterministically;
- retarget restarts the launch burst;
- spawn smoke: stable and grounded;
- room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual Stage 5 gate.
