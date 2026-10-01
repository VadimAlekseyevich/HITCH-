# Task 07 — Verify Clean-Checkout Bootstrap and Close Stage 1

**Status:** DONE  
**Stage:** 1 — Godot/C# Project Bootstrap  
**Depends on:** Tasks 05 and 06

## Goal

Evaluate Stage 1 as a whole from the perspective of a new AI agent with a clean checkout.

This task is verification, cleanup, and documentation synchronization. It must not smuggle in gameplay.

## Read first

- all Stage 1 task files;
- `README.md`;
- `AGENTS.md`;
- `docs/TOOLCHAIN.md`;
- `docs/ARCHITECTURE.md`;
- `docs/DECISIONS.md`;
- Stage 1 in `docs/ROADMAP.md`.

## Scope

### Clean-checkout rehearsal

Using a clean checkout/worktree/container where practical:

1. follow only repository documentation;
2. verify toolchain assumptions;
3. restore/build;
4. run the minimal project;
5. run tests;
6. confirm CI is green.

### Documentation reconciliation

Fix:

- stale commands;
- contradictory version references;
- broken links;
- obsolete placeholder README text;
- directory maps that no longer match reality.

### Repository hygiene

Check that:

- generated Godot/.NET artifacts are ignored;
- no machine-specific files are committed;
- no accidental gameplay code was added;
- no unused speculative bootstrap abstractions remain.

### Stage status

Only after the exit criteria are satisfied:

- mark Tasks 01–07 DONE in the stage README;
- record Stage 1 as complete in the appropriate progress documentation;
- do not begin or redesign Stage 2.

## Non-scope

- Stage 2 implementation;
- simulation state;
- fixed timestep;
- gameplay input;
- player controller;
- unrelated refactors.

## Stage 1 exit criteria

A clean checkout can:

1. build;
2. run the minimal project;
3. run tests;
4. see equivalent verification pass in CI.

No reasonable basic verification step depends on undocumented editor interaction.

## Verification evidence to report

The completing agent should report:

- exact Godot version;
- exact .NET SDK;
- local build command/result;
- local test command/result;
- run command/manual launch result;
- CI run result/link if available;
- any known bootstrap limitation.


## Completion evidence

Stage 1 gate was verified by GitHub Actions on 2026-10-01.

- Godot: **4.7.2 stable .NET**
- .NET SDK: **8.0.425**
- Game restore/build: **success**
- Build warnings/errors: **0 / 0**
- Automated tests: **2 passed, 0 failed**
- Godot headless launch: **success**
- CI run: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36794551218

The root command surface used for local verification is:

```text
build_and_run.bat build
build_and_run.bat test
build_and_run.bat smoke
build_and_run.bat
```

A normal double-click on `build_and_run.bat` bootstraps missing local tools, builds, tests, downloads Godot when required, and launches the project.
