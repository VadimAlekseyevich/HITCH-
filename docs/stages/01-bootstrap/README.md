# Stage 1 — Godot/C# Project Bootstrap

Parent roadmap stage: [ROADMAP.md — Stage 1](../../ROADMAP.md)

## Stage objective

Create the smallest reproducible Godot C# project that a future AI agent can build, run, and test from a clean checkout.

There is **no gameplay implementation in this stage**.

## Stage exit gate

Stage 1 is complete only when a clean checkout can be used to:

1. install/use the documented toolchain;
2. build the project;
3. run the minimal Godot project;
4. run automated tests;
5. observe the same build/tests passing in GitHub Actions.

There should be no hidden editor-only ritual for basic verification when it can reasonably be automated.

## Task order

| ID | Task | Status | Depends on |
|---|---|---|---|
| 01 | [Pin toolchain and bootstrap constraints](./TASK-01-toolchain.md) | PLANNED | Stage 0 |
| 02 | [Create minimal Godot C# project](./TASK-02-godot-project.md) | PLANNED | 01 |
| 03 | [Create repeatable local build/run workflow](./TASK-03-local-workflow.md) | PLANNED | 02 |
| 04 | [Add C# automated test harness](./TASK-04-tests.md) | PLANNED | 03 |
| 05 | [Add minimal CI](./TASK-05-ci.md) | PLANNED | 04 |
| 06 | [Define configuration and diagnostics conventions](./TASK-06-conventions.md) | PLANNED | 04 |
| 07 | [Verify clean-checkout bootstrap and close Stage 1](./TASK-07-stage-gate.md) | PLANNED | 05, 06 |

## Why this order

Tool versions come first because every later task must target one known environment.

The project skeleton comes before scripts because scripts should automate the **actual** project rather than a guessed future layout.

Tests come before CI because CI should run the same local verification commands, not introduce a second build procedure.

Configuration/logging conventions are deliberately late in bootstrap: by then there is a real C# project and test harness, so the convention can be represented concretely without inventing gameplay systems.

## Stage-wide non-scope

Do not implement:

- player movement;
- camera gameplay;
- grapple/winch;
- combat;
- networking;
- debug gameplay overlay;
- arena content;
- final export/release pipeline;
- performance optimization.

A minimal startup scene is allowed only to prove the engine project can run.
