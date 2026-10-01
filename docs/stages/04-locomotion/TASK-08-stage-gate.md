# Task 08 — Verify Stage 4 Gate

**Status:** DONE  
**Depends on:** Task 07

## Goal

Verify the locomotion foundation in the actual movement lab and synchronize documentation.

## Required evidence

- game build result;
- automated test count/result;
- headless smoke result;
- capsule dimensions and temporary movement defaults;
- confirmation that simulation, not Godot body nodes, owns position/velocity;
- confirmation that external impulses are supported;
- known limitations such as steps/moving platforms;
- manual note that subjective 60 Hz vs 120 Hz feel still requires human play if not decided.

Do not start the winch in this task.


## Completion evidence

Verified by GitHub Actions on 2026-10-01:

- build: success;
- warnings/errors: 0 / 0;
- automated tests: 38 passed, 0 failed;
- Godot 4.7.2 headless smoke: success;
- CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36798793458

Temporary locomotion defaults:

- capsule: 1.80 m height, 0.45 m radius;
- gravity: 18 m/s²;
- ground max speed: 3.5 m/s;
- jump speed: 4.2 m/s;
- ground acceleration: 12 m/s²;
- air acceleration: 1.8 m/s²;
- air-control speed target: 2.5 m/s;
- simulation baseline: 60 Hz.

Architecture verification:

- `PlayerState.Position` / `Velocity` remain authoritative;
- Godot/Jolt only supplies world collision queries;
- no CharacterBody3D/RigidBody3D owns locomotion;
- external velocity impulses compose with existing momentum;
- no speed cap is applied.

Known limitations deliberately left for later:

- no step-climbing system;
- no moving platforms;
- no dynamic player/player collision;
- no collision damage;
- no subjective 60 Hz vs 120 Hz feel decision yet.

Manual feel verification can be performed with `build_and_run.bat`.
