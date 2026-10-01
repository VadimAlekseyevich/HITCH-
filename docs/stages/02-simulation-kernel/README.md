# Stage 2 — Simulation Kernel and Explicit State

Parent roadmap stage: [ROADMAP.md — Stage 2](../../ROADMAP.md)

**Status: ACTIVE**

## Stage objective

Create the minimum replayable gameplay-simulation kernel needed by later movement work.

Stage 2 answers one architectural question:

> Where does gameplay state live, what input advances it, and what advances it by exactly one simulation tick?

There is still **no player locomotion, grapple, combat, or networking** in this stage.

## Task order

| ID | Task | Status | Depends on |
|---|---|---|---|
| 01 | [Create pure C# simulation assembly](./TASK-01-simulation-assembly.md) | PLANNED | Stage 1 |
| 02 | [Define config, input, and explicit state contracts](./TASK-02-state-input-config.md) | PLANNED | 01 |
| 03 | [Define minimal world-query boundary](./TASK-03-world-query.md) | PLANNED | 02 |
| 04 | [Implement one-tick simulation kernel](./TASK-04-kernel.md) | PLANNED | 02, 03 |
| 05 | [Connect kernel to Godot fixed physics tick](./TASK-05-godot-runner.md) | PLANNED | 04 |
| 06 | [Verify Stage 2 gate and synchronize docs](./TASK-06-stage-gate.md) | PLANNED | 05 |

## Stage exit gate

Stage 2 is complete when:

- gameplay simulation is a separate plain C# assembly;
- simulation state is explicit, copyable, and inspectable;
- gameplay input is explicit and independent from keyboard/mouse APIs;
- one call advances simulation by exactly one fixed tick;
- world queries cross one small explicit boundary;
- Godot drives the kernel from its fixed physics update without making scene state the gameplay source of truth;
- ordinary simulation tests do not need Godot;
- CI remains green.

## Stage-wide non-scope

Do not implement:

- actual walking acceleration;
- jump physics;
- gravity gameplay;
- capsule collision resolution;
- camera controls;
- grapple/winch;
- combat;
- networking;
- prediction/reconciliation;
- final serialization format.

Those belong to later roadmap stages.
