# Task 03 — Define Minimal World-Query Boundary

**Status:** DONE  
**Depends on:** Task 02

## Goal

Give future simulation code a small explicit way to ask the world about collision geometry without calling Godot/Jolt APIs directly.

## Scope

Define only contracts required by upcoming locomotion/grapple work:

- ray query + hit result;
- capsule sweep query + hit result;
- `IWorldQuery` (or equivalently small interface) exposing those operations.

Use plain simulation-side data types.

Include only metadata that has a clear near-term use, such as collision layer information.

## Non-scope

- Godot implementation of real casts;
- collision response;
- surface gameplay rules;
- arbitrary entity registry;
- player-target identification;
- rope wrapping.

## Acceptance criteria

- simulation project can express ray and capsule queries with no Godot types;
- callers do not need scene-tree access;
- interface is deliberately small;
- query/result validation/value semantics have useful tests where appropriate.
