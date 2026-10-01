# Task 04 — Add C# Automated Test Harness

**Status:** DONE  
**Stage:** 1 — Godot/C# Project Bootstrap  
**Depends on:** Task 03

## Goal

Create the smallest maintainable automated test setup for ordinary C# gameplay-simulation code.

This task establishes infrastructure only; there is no gameplay to test yet.

## Read first

- `AGENTS.md`
- `docs/ARCHITECTURE.md`
- `tests/README.md`
- `docs/TOOLCHAIN.md`
- local workflow produced by Task 03

## Scope

1. Choose a mainstream .NET test framework compatible with the pinned SDK.
2. Prefer a test project that can test simulation code without launching Godot when engine integration is unnecessary.
3. Add one or a few bootstrap smoke tests proving discovery and pass/fail propagation.
4. Integrate test execution into the documented local workflow.
5. Document where future unit tests versus Godot/Jolt integration tests should live.

## Architectural intent

Future simulation tests should be cheap ordinary C# tests whenever possible.

Do not force mathematical/state tests to boot a Godot scene.

Godot/Jolt integration tests can be added later when engine behavior itself is under test.

## Non-scope

- movement tests;
- winch tests;
- hypothetical scene test frameworks;
- coverage targets;
- mutation testing;
- benchmarks.

## Acceptance criteria

- One documented test command runs from repository root.
- At least one test is discovered and passes.
- A deliberately failing test causes non-zero exit status.
- Ordinary C# tests do not require opening Godot.
- Test framework/version choices are explicit and reproducible.

## Verification

Run the normal test command. Temporarily invert a smoke assertion to prove failure propagation, then restore it before commit.
