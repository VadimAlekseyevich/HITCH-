# Task 05 — Connect Kernel to Godot Fixed Physics Tick

**Status:** PLANNED  
**Depends on:** Task 04

## Goal

Prove that Godot can host and observe the plain C# simulation kernel without becoming the gameplay source of truth.

## Scope

- add a small Godot-side runner/controller;
- drive exactly one simulation step from Godot's fixed physics callback;
- create a temporary no-hit world-query adapter sufficient before real casts are implemented;
- show current simulation tick / configured tick rate in the bootstrap scene;
- explicitly configure the current Godot physics tick rate to match the temporary simulation baseline.

## Non-scope

- device gameplay input;
- player transform movement;
- camera;
- real ray/capsule query adapter;
- debug HUD framework;
- 60 vs 120 feel experiment.

## Acceptance criteria

- scene launches;
- simulation advances while running;
- gameplay state lives in simulation types, not Label/Node fields;
- headless CI smoke launch succeeds;
- Godot-specific code stays outside the simulation assembly.
