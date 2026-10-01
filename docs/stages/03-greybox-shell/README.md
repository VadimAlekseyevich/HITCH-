# Stage 3 — Greybox World, First-Person Shell, and Debug Instrumentation

Parent roadmap stage: [ROADMAP.md — Stage 3](../../ROADMAP.md)

**Status: ACTIVE**

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
| 01 | [Add simulation-owned first-person view state](./TASK-01-view-state.md) | PLANNED |
| 02 | [Add Godot device input adapter and first-person rig](./TASK-02-input-camera.md) | PLANNED |
| 03 | [Build greybox movement laboratory](./TASK-03-greybox.md) | PLANNED |
| 04 | [Add debug overlay](./TASK-04-overlay.md) | PLANNED |
| 05 | [Add 3D debug drawing hooks](./TASK-05-debug-draw.md) | PLANNED |
| 06 | [Verify Stage 3 gate](./TASK-06-stage-gate.md) | PLANNED |

## Exit gate

A developer can launch one scene and:

- capture/release the mouse;
- look around with simulation-owned yaw/pitch;
- see the greybox lab;
- observe tick, position, velocity, speed, grounded state, and input values;
- see simple 3D debug vectors/lines;
- feed hardware-independent input into the same simulation kernel;
- pass build/tests/headless smoke CI.
