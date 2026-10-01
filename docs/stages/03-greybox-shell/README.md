# Stage 3 — Greybox World, First-Person Shell, and Debug Instrumentation

Parent roadmap stage: [ROADMAP.md — Stage 3](../../ROADMAP.md)

**Status: DONE — 2026-10-01**

## Objective

Create the environment needed to iterate on movement in Stage 4.

Stage 3 does not implement locomotion. It provides:

- a first-person view whose gameplay-facing orientation lives in simulation state;
- a device-input adapter that produces `PlayerInput`;
- a 3D greybox movement laboratory;
- a debug overlay;
- simple reusable 3D debug-line drawing.

## Tasks

| ID | Task | Status |
|---|---|---|
| 01 | [Add simulation-owned first-person view state](./TASK-01-view-state.md) | DONE |
| 02 | [Add Godot device input adapter and first-person rig](./TASK-02-input-camera.md) | DONE |
| 03 | [Build greybox movement laboratory](./TASK-03-greybox.md) | DONE |
| 04 | [Add debug overlay](./TASK-04-overlay.md) | DONE |
| 05 | [Add 3D debug drawing hooks](./TASK-05-debug-draw.md) | DONE |
| 06 | [Verify Stage 3 gate](./TASK-06-stage-gate.md) | DONE |

## Exit gate

A developer can launch one scene and:

- capture/release the mouse;
- look around with simulation-owned yaw/pitch;
- see the greybox lab;
- observe tick, position, velocity, speed, grounded state, and input values;
- see simple 3D debug vectors/lines;
- feed hardware-independent input into the same simulation kernel;
- pass build/tests/headless smoke CI.


## Completion evidence

CI gate: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36797560255

Verified:

- first-person mouse capture/release shell;
- simulation-owned yaw/pitch;
- hardware-independent `PlayerInput`;
- collidable greybox movement lab;
- debug overlay;
- reusable ImmediateMesh debug lines;
- 13/13 tests passing;
- headless Godot smoke launch passing.
