# Task 05 — Add Minimal GitHub Actions CI

**Status:** PLANNED  
**Stage:** 1 — Godot/C# Project Bootstrap  
**Depends on:** Task 04

## Goal

Make GitHub independently verify that the repository builds and tests successfully.

CI must reproduce the local verification path rather than invent a second workflow.

## Read first

- `AGENTS.md`
- `docs/TOOLCHAIN.md`
- local workflow from Task 03
- test harness from Task 04

## Scope

Create one minimal GitHub Actions workflow that, on appropriate pushes/pull requests:

1. checks out the repository;
2. selects/installs the documented .NET SDK;
3. makes the pinned Godot/.NET runtime available if the build path needs it;
4. restores dependencies;
5. builds;
6. runs automated tests;
7. fails clearly when build or tests fail.

Use caching only if it stays straightforward and clearly useful.

## Non-scope

- release builds;
- player artifacts;
- deployment;
- code signing;
- OS matrices;
- benchmarks;
- coverage gates;
- automatic versioning.

## Acceptance criteria

- CI uses the same pinned toolchain assumptions as local development.
- CI requires no undocumented secrets.
- Build and tests are both checked.
- Workflow stays concise and understandable.
- Documentation reflects the CI expectation where useful.

## Verification

- Validate workflow syntax.
- Trigger the workflow and confirm a real successful run.
- If practical, verify broken tests/builds fail CI without leaving `main` broken.

This task is not complete merely because a YAML file exists; a passing workflow run is the intended evidence.
