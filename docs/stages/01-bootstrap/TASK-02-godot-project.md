# Task 02 — Create Minimal Godot C# Project

**Status:** DONE  
**Stage:** 1 — Godot/C# Project Bootstrap  
**Depends on:** Task 01

## Goal

Create the smallest real Godot C# project using the pinned toolchain and make it launch to an intentionally minimal scene.

The purpose is only to prove that the repository is a valid Godot/.NET project.

## Read first

- `AGENTS.md`
- `docs/TOOLCHAIN.md`
- `docs/ARCHITECTURE.md`
- `src/README.md`
- `scenes/README.md`
- `docs/stages/01-bootstrap/README.md`

## Scope

1. Create the actual Godot project files with the exact pinned Godot .NET version.
2. Create the minimal C# solution/project files required by that version.
3. Configure Jolt explicitly if needed.
4. Create one minimal startup scene that proves the app launches.
5. Add/update `.gitignore` for Godot, .NET, editor/generated files, and local build artifacts.
6. Materialize only source directories that are genuinely useful now.
7. Ensure generated/intermediate files are not committed.
8. Update basic open/run instructions only where necessary.

## Non-scope

- player controller;
- camera controls;
- gameplay physics;
- tuning system;
- tests;
- CI;
- networking;
- polished startup UI.

## Acceptance criteria

- Godot recognizes the repository as a valid project.
- C# restores/compiles with the pinned SDK.
- Running the project opens the minimal scene without gameplay errors.
- Jolt is configured as intended.
- Ordinary editor/build use does not pollute Git with generated files.
- No gameplay code exists.

## Verification

Using the pinned toolchain:

1. restore/compile;
2. open/run the project;
3. confirm startup succeeds;
4. inspect Git status after the run.

Task 03 will automate this workflow; manual commands are acceptable here.
