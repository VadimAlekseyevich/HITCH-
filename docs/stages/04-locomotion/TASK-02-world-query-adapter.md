# Task 02 — Implement Godot/Jolt World-Query Adapter

**Status:** PLANNED  
**Depends on:** Task 01

## Goal

Replace the Stage 2 no-hit adapter with real physics-space ray and upright-capsule sweep queries against Godot/Jolt.

## Requirements

- physics queries execute from Godot `_PhysicsProcess`;
- simulation-facing data uses only simulation types;
- raycasts return hit point/normal/travel fraction;
- capsule sweeps return maximum safe travel fraction and collision normal;
- query shape reuse avoids unnecessary per-tick allocations where practical;
- CSG movement-lab collision is detectable;
- adapter remains Godot-side.

## Non-scope

- movement response;
- player/player dynamic collision;
- grapple surface rules.
