# Stage 4 — Base Player Locomotion

Parent roadmap stage: [ROADMAP.md — Stage 4](../../ROADMAP.md)

**Status: DONE — 2026-10-01**

## Objective

Implement predictable custom-controlled capsule locomotion that remains intentionally weaker than the future winch.

This stage validates the player-controller foundation, not the main movement fantasy.

## Tasks

| ID | Task | Status | Depends on |
|---|---|---|---|
| 01 | [Define locomotion tuning and capsule semantics](./TASK-01-config.md) | DONE | Stage 3 |
| 02 | [Implement Godot/Jolt world-query adapter](./TASK-02-world-query-adapter.md) | DONE | 01 |
| 03 | [Implement capsule sweep/slide solver](./TASK-03-capsule-solver.md) | DONE | 02 |
| 04 | [Add ground detection and gravity](./TASK-04-ground-gravity.md) | DONE | 03 |
| 05 | [Add slow view-relative walking](./TASK-05-walking.md) | DONE | 04 |
| 06 | [Add weak jump and weak air correction](./TASK-06-jump-air.md) | DONE | 05 |
| 07 | [Add external impulses and high-speed/timestep checks](./TASK-07-impulses-speed.md) | DONE | 06 |
| 08 | [Verify Stage 4 gate](./TASK-08-stage-gate.md) | DONE | 07 |

## Exit gate

Without a winch:

- capsule movement is stable against the movement-lab geometry;
- gravity/grounding work;
- walking is deliberately slow;
- jumping is deliberately weak;
- air correction is weak and configurable;
- external gameplay impulses have an explicit path into velocity;
- movement behavior is fixed-timestep based and tested;
- debug overlay reflects the real movement state;
- build/tests/headless CI pass.

Do not make ordinary FPS locomotion the interesting part of HITCH!.


## Completion evidence

CI gate: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36798793458

Verified:

- custom simulation-owned capsule locomotion;
- real Godot/Jolt ray/sweep adapter;
- bounded sweep/slide collision response;
- gravity and ground detection;
- slow view-relative walking;
- weak jump;
- weak configurable air correction;
- external velocity impulses without speed clamping;
- representative 60/120 Hz timestep tests;
- 1000 m/s requested-displacement sweep regression;
- 38/38 automated tests;
- build with 0 warnings / 0 errors;
- Godot headless smoke launch.

The simulation remains temporarily configured at 60 Hz. Subjective 60/120 Hz feel comparison remains a manual Stage 4/5 experiment rather than an automatic design decision.
