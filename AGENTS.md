# AGENTS.md — HITCH! AI Development Contract

This repository is expected to be implemented primarily by AI coding agents.

This file defines mandatory behavior for any agent modifying the repository.

## 1. Read before changing code

Before implementing a task, read at minimum:

1. `DESIGN.md`
2. `docs/DECISIONS.md`
3. `docs/ARCHITECTURE.md`
4. the relevant stage in `docs/ROADMAP.md`
5. any README inside directories touched by the task

Do not infer the game's design from existing code when documentation says otherwise.

## 2. Source-of-truth priority

When information conflicts, use this order:

1. the user's explicit current task;
2. `DESIGN.md` for gameplay intent;
3. `docs/DECISIONS.md` for accepted decisions;
4. `docs/ARCHITECTURE.md` for structural constraints;
5. `docs/ROADMAP.md` for sequencing/scope;
6. existing implementation.

If the current task intentionally changes a documented decision, update the affected document in the same change.

## 3. Do not invent product decisions

When behavior is marked `TBD`, unresolved, experimental, or tunable:

- implement only the smallest temporary behavior needed for the current task;
- make the choice easy to replace;
- expose important constants through configuration rather than burying them;
- explicitly document temporary assumptions;
- do not rewrite documentation as if the temporary choice were approved.

## 4. One task must stay one task

A future task should normally produce one focused change that can be reviewed independently.

Do not opportunistically:

- redesign unrelated modules;
- add speculative frameworks;
- add content "for later";
- implement later roadmap stages;
- refactor unrelated files merely for style.

If a required dependency is missing, implement the smallest dependency needed or report the blocker.

## 5. Architecture boundaries are mandatory

Core gameplay simulation must not depend on presentation.

In particular:

- camera code must not own gameplay state;
- UI must not own gameplay state;
- VFX must not determine winch physics;
- Godot scene-tree location must not be required to understand core simulation state;
- networking must call/reuse gameplay simulation rather than reimplementing different movement rules.

See `docs/ARCHITECTURE.md`.

## 6. Prefer explicit, typed, testable state

Prefer:

- C# types with clear ownership;
- explicit inputs and outputs;
- small focused components;
- deterministic order of operations;
- dependency injection or explicit context over hidden globals;
- pure calculations where practical;
- configuration objects for tunable gameplay values.

Avoid:

- giant manager classes;
- mutable global state;
- gameplay logic distributed across unrelated scene callbacks;
- string-driven behavior where typed alternatives are reasonable;
- magic numbers inside algorithms.

## 7. Performance rules

HITCH! is intended to feel highly responsive.

Do not optimize blindly, but avoid obvious hot-loop problems.

Inside fixed-step simulation code:

- avoid avoidable per-tick managed allocations;
- avoid LINQ in hot paths unless measured and justified;
- avoid repeated scene-tree searches;
- do not tie simulation behavior to render frame rate;
- keep profiling/debug instrumentation possible.

Correctness and clarity come before micro-optimization until profiling exists.

## 8. Networking-aware implementation

Networking is not required in the first local prototype, but core movement must be written so that it can later support:

- authoritative server simulation;
- client-side prediction;
- reconciliation;
- replay of buffered inputs;
- interpolation of remote players.

Do not make the local prototype depend on non-replayable presentation state.

## 9. Testing expectations

Every task should add or update automated tests when the changed behavior is meaningfully testable without fragile scene-level integration.

Prioritize tests for:

- mathematical simulation behavior;
- state transitions;
- configuration validation;
- input/state serialization once networking starts;
- regressions discovered during development.

Do not create meaningless tests solely to increase test count.

## 10. Completion protocol

Before considering a coding task complete:

1. build the project;
2. run relevant tests;
3. check the diff for accidental unrelated changes;
4. confirm the task did not cross roadmap-stage scope without reason;
5. update documentation if a contract or decision changed;
6. report what changed, how it was verified, and any remaining known limitation.

## 11. No silent scope expansion

The MVP deliberately excludes many final-game systems.

Unless specifically requested, do not add:

- matchmaking;
- ranked systems;
- progression;
- cosmetics;
- inventory frameworks;
- generic ability frameworks;
- generalized mod support;
- production analytics;
- multiple maps;
- production UI architecture;
- final animation/content pipelines.

Build the experiment first.
