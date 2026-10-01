# Task 06 — Define Configuration and Diagnostics Conventions

**Status:** DONE  
**Stage:** 1 — Godot/C# Project Bootstrap  
**Depends on:** Task 04

## Goal

Establish two small conventions needed before gameplay agents begin introducing tuning constants and diagnostics:

1. how tunable gameplay configuration is represented;
2. how development logging, invariants, and unexpected errors are expressed.

Create a convention, not a framework.

## Read first

- `AGENTS.md`
- `DESIGN.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- project/test structure after Tasks 02–04

## Scope

### Gameplay tuning configuration

Choose the simplest Godot/C# representation that is:

- clear from C#;
- authoritative in one place per setting;
- editable without scattered magic constants;
- inspectable by future debug tooling;
- not dependent on gameplay nodes owning global truth;
- testable/validatable where practical.

Document the convention.

A minimal placeholder/example type is acceptable only if it proves the pattern. Do not invent real movement/winch tuning values yet.

### Diagnostics

Define minimal policy for:

- normal development logs;
- warnings;
- programmer invariants/assertions;
- unrecoverable unexpected state.

Prefer Godot/.NET facilities unless a tiny wrapper has clear value. Do not build a logging platform.

### Documentation

Update architecture/agent docs only where the convention becomes a real contract.

## Non-scope

- debug HUD;
- telemetry/analytics;
- production logging backend;
- actual movement/winch parameters;
- crash-reporting service;
- runtime settings menu.

## Acceptance criteria

Future agents can answer unambiguously:

- Where should a new tunable gameplay parameter go?
- How should a violated simulation invariant be reported?
- How should a development warning be emitted?

The solution is small, documented, and minimally tested if it contains meaningful behavior.

## Verification

Build, run tests, and inspect any example usage for accidental coupling to hypothetical gameplay systems.
