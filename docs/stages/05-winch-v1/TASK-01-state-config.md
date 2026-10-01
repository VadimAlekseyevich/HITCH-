# Task 01 — Define Winch Config, Path Abstraction, and Explicit State

**Status:** DONE

## Goal

Introduce simulation-owned winch data without committing the project to a forever-single-segment rope representation.

## Requirements

Define:

- `WinchConfig` with validated experimental tuning;
- explicit detached/attached state;
- world-anchor/path abstraction;
- rest/current rope length;
- reel motor velocity;
- reattach cooldown;
- last/active tension information useful for debugging.

The first path implementation may resolve to one world anchor, but callers should ask the path for its current pull point rather than reaching through arbitrary scene state.

## Non-scope

- actual attach raycast;
- spring force;
- obstruction contacts;
- player targets.
