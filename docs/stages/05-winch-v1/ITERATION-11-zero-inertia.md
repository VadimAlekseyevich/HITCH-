# Stage 5 — Iteration 11: Zero Inertia

**Status: IMPLEMENTED — awaiting human playtest**

## Human diagnosis

The dominant remaining problem is excessive carried inertia.

Symptoms:

- the player keeps being dragged after high-speed movement;
- wall impacts convert motion into sideways travel instead of producing a clean stop;
- grapple travel carries old tangential/orbital velocity that fights direct control.

## Active movement rule

Iteration 11 removes carried grapple inertia from the Stage 5 prototype.

### While grapple is pulling

- previous player velocity is ignored;
- velocity is assigned directly toward the current anchor;
- current direct pull speed is 42 m/s;
- no previous tangential component survives;
- gravity is not applied during active pull;
- air-control is not applied during active pull.

### World impacts

- walkable floor contacts still support horizontal locomotion;
- walls, ceilings, and steep non-walkable surfaces hard-stop velocity;
- impact momentum is not projected into a tangent slide.

### Release / arrival

- RMB miss releases the cable and clears grapple-carried velocity;
- reaching the selected surface enters the existing latched state;
- latched velocity remains exactly zero until RMB retargets or releases.

## Design status

The previous "preserve momentum almost completely" rule is reopened for Stage 5.

Do not restore momentum preservation unless a later human playtest specifically asks for it.

## Human gate

Verify:

1. grapple retarget immediately changes direction instead of blending with the old trajectory;
2. flying into a wall stops the player instead of dragging them sideways;
3. completed grapple contact remains stable and motionless;
4. movement feels more controllable and deliberate without carried inertia.


### Iteration 11 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36914543762

- build: success;
- warnings/errors: 0 / 0;
- tests: 61 passed / 0 failed;
- spawn smoke: stable and grounded;
- room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual gate.
