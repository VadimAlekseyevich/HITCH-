# Roadmap Stage Breakdowns

This directory contains task-level decomposition for the **currently active or upcoming roadmap stage**.

The high-level sequence remains authoritative in [../ROADMAP.md](../ROADMAP.md).

A stage breakdown is created only when that stage is about to begin. We intentionally avoid writing code-level task plans for distant stages because the repository will change before we reach them.

## Stages

- [Stage 1 — Godot/C# project bootstrap](./01-bootstrap/README.md) — **DONE**
- [Stage 2 — Simulation kernel and explicit state](./02-simulation-kernel/README.md) — **DONE**
- [Stage 3 — Greybox world, first-person shell, and debug instrumentation](./03-greybox-shell/README.md) — **DONE**
- [Stage 4 — Base player locomotion](./04-locomotion/README.md) — **DONE**
- [Stage 5 — Winch v1: world-anchor movement](./05-winch-v1/README.md) — **READY FOR HUMAN GATE, ITERATION 9 EXPANDED LAB + HARD SETTLE**

## Status convention

- **PLANNED** — task is defined but not started.
- **IN PROGRESS** — an agent is actively implementing it.
- **DONE** — acceptance criteria were verified in the repository.
- **BLOCKED** — cannot continue until a named dependency/problem is resolved.
- **SUPERSEDED** — replaced by a later task/decision.

Task files are execution contracts for one focused AI-agent request.
