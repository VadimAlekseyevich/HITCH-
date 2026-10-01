# Task 01 — Create Pure C# Simulation Assembly

**Status:** PLANNED

## Goal

Create a plain .NET simulation project that does not depend on Godot assemblies and can be referenced by both the Godot game project and ordinary C# tests.

## Read first

- `AGENTS.md`
- `docs/ARCHITECTURE.md`
- `docs/CONVENTIONS.md`
- this stage README

## Scope

- create `src/Simulation/Hitch.Simulation.csproj`;
- target the pinned `net8.0`;
- reference it from `HITCH.csproj`;
- reference it from `tests/HITCH.Tests/HITCH.Tests.csproj`;
- ensure source files in the simulation project are not also compiled directly into the Godot assembly;
- add only the minimum placeholder needed to prove references compile.

## Non-scope

- gameplay state design;
- input model;
- world queries;
- fixed-step runner;
- Godot nodes.

## Acceptance criteria

- simulation assembly has no Godot package/reference;
- game builds while referencing it;
- test project builds while referencing it;
- CI/build commands remain the same.
