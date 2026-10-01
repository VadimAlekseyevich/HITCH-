# Stage 5 — Winch v1: World-Anchor Movement

Parent roadmap stage: [ROADMAP.md — Stage 5](../../ROADMAP.md)

**Status: ACTIVE**

## Objective

Build the first genuinely playable winch loop:

```text
aim → attach → tension/swing → reel in/out → detach → keep momentum → reattach
```

This stage is complete only after a **human core-movement playtest**, not merely green CI.

## Tasks

| ID | Task | Status | Depends on |
|---|---|---|---|
| 01 | [Define winch config, path abstraction, and explicit state](./TASK-01-state-config.md) | PLANNED | Stage 4 |
| 02 | [Implement hitscan attach/detach state machine](./TASK-02-attach.md) | PLANNED | 01 |
| 03 | [Implement accelerating reel motor](./TASK-03-reel.md) | PLANNED | 02 |
| 04 | [Implement elastic tension model v1](./TASK-04-spring.md) | PLANNED | 03 |
| 05 | [Add high-speed reel falloff and verify detach momentum](./TASK-05-energy.md) | PLANNED | 04 |
| 06 | [Add rope/debug visualization and telemetry](./TASK-06-debug-telemetry.md) | PLANNED | 05 |
| 07 | [Add explicit forbidden grapple surface to movement lab](./TASK-07-surfaces.md) | PLANNED | 06 |
| 08 | [Automated Stage 5 verification](./TASK-08-technical-gate.md) | PLANNED | 07 |
| 09 | [Human core-movement gate](./TASK-09-human-gate.md) | WAITING | 08 |

## Stage exit gate

Automated requirements:

- attach/detach/re-attach state transitions are explicit and tested;
- grapple is hitscan with configurable range and collision mask;
- winch state is simulation-owned;
- reel motor accelerates/decelerates;
- elastic tension is tunable;
- reeling can add kinetic energy;
- high-speed reel-in effectiveness can fall off without a general speed cap;
- detach preserves existing velocity;
- debug overlay/lines expose rope state;
- basic movement telemetry is visible;
- build/tests/headless smoke pass.

Human requirements:

- attaching/detaching feels immediate;
- reel timing meaningfully changes trajectory;
- spring behavior creates learnable movement rather than automated traversal;
- releasing the rope produces satisfying momentum;
- repeated greybox traversal is fun enough to continue.

If the human gate fails, tune/iterate Stage 5 instead of proceeding.
