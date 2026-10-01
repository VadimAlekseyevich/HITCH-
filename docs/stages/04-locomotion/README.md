# Stage 4 — Base Player Locomotion

Parent roadmap stage: [ROADMAP.md — Stage 4](../../ROADMAP.md)

**Status: ACTIVE**

## Objective

Implement predictable custom-controlled capsule locomotion that remains intentionally weaker than the future winch.

This stage validates the player-controller foundation, not the main movement fantasy.

## Tasks

| ID | Task | Status | Depends on |
|---|---|---|---|
| 01 | [Define locomotion tuning and capsule semantics](./TASK-01-config.md) | PLANNED | Stage 3 |
| 02 | [Implement Godot/Jolt world-query adapter](./TASK-02-world-query-adapter.md) | PLANNED | 01 |
| 03 | [Implement capsule sweep/slide solver](./TASK-03-capsule-solver.md) | PLANNED | 02 |
| 04 | [Add ground detection and gravity](./TASK-04-ground-gravity.md) | PLANNED | 03 |
| 05 | [Add slow view-relative walking](./TASK-05-walking.md) | PLANNED | 04 |
| 06 | [Add weak jump and weak air correction](./TASK-06-jump-air.md) | PLANNED | 05 |
| 07 | [Add external impulses and high-speed/timestep checks](./TASK-07-impulses-speed.md) | PLANNED | 06 |
| 08 | [Verify Stage 4 gate](./TASK-08-stage-gate.md) | PLANNED | 07 |

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
