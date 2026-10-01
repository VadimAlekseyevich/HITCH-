# Documentation Index

This directory contains planning and engineering contracts for HITCH!.

## Core documents

### [../DESIGN.md](../DESIGN.md)

Defines the MVP gameplay intent.

Answers:

- what the player should be able to do;
- what the winch should feel like;
- what combat exists in the MVP;
- what is explicitly out of scope;
- which gameplay questions remain TBD.

### [DECISIONS.md](./DECISIONS.md)

Compact decision ledger.

Use it when an agent needs to know whether a choice has already been made.

A decision may be:

- **DECIDED** — do not reopen implicitly;
- **HYPOTHESIS** — current engineering direction, validate during implementation;
- **TBD** — intentionally unresolved.

### [ARCHITECTURE.md](./ARCHITECTURE.md)

Defines intended module boundaries and dependency direction.

This file should answer:

- where a new piece of logic belongs;
- what may depend on what;
- what state must be replayable/networkable;
- which Godot features are adapters rather than gameplay truth.

### [TOOLCHAIN.md](./TOOLCHAIN.md)

Pins Godot, .NET, Jolt, the bootstrap platform, and one-click setup behavior.

### [CONVENTIONS.md](./CONVENTIONS.md)

Defines gameplay tuning ownership and diagnostics/assertion conventions.

### [ROADMAP.md](./ROADMAP.md)

Defines the ordered development stages to the first networked MVP.

A later stage must not be implemented simply because it looks useful. Each stage has an exit gate.

### [WORKFLOW.md](./WORKFLOW.md)

Defines how we will later split a roadmap stage into AI-sized implementation tasks.

The roadmap intentionally stays coarser than individual agent prompts.

### [stages/](./stages/README.md)

Contains task-level execution contracts for the active/upcoming roadmap stage.

Current breakdown: [Stage 1 — Godot/C# Project Bootstrap](./stages/01-bootstrap/README.md).

## Documentation discipline

Do not duplicate the same contract in many files.

- Gameplay behavior belongs in `DESIGN.md`.
- Accepted choices belong in `DECISIONS.md`.
- Dependency/ownership rules belong in `ARCHITECTURE.md`.
- Work ordering belongs in `ROADMAP.md`.
- Agent/task process belongs in `AGENTS.md` and `WORKFLOW.md`.

When a decision changes, update the authoritative location and only adjust cross-references elsewhere.
