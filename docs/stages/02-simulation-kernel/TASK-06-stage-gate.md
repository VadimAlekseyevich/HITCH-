# Task 06 — Verify Stage 2 Gate and Synchronize Docs

**Status:** PLANNED  
**Depends on:** Task 05

## Goal

Verify the real Stage 2 architecture, remove temporary bootstrap inconsistencies, and mark the stage complete only if its exit gate is satisfied.

## Scope

- run build/tests/headless smoke;
- verify simulation assembly has no Godot dependency;
- verify Godot game and tests both reference the same simulation assembly;
- verify one-step semantics and explicit state/input contracts;
- reconcile README/architecture/status docs with actual files;
- mark Stage 2 tasks DONE only after evidence exists.

## Non-scope

- Stage 3 implementation;
- movement;
- camera gameplay;
- debug overlay framework.

## Required evidence

Report:

- build result;
- test count/result;
- headless Godot smoke result;
- core simulation project path;
- exact temporary simulation tick rate;
- known Stage 2 limitations/TBDs.
