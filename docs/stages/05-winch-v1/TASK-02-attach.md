# Task 02 — Implement Hitscan Attach/Detach State Machine

**Status:** PLANNED  
**Depends on:** Task 01

## Goal

Make grapple press acquire a world anchor immediately and grapple release detach while preserving player velocity.

## Requirements

- ray originates from simulation-derived eye position;
- direction comes from simulation yaw/pitch;
- range is configurable;
- grapple collision mask controls valid surfaces;
- press while detached and cooldown-ready may attach;
- release while attached detaches;
- detach starts a very small configurable reattach cooldown;
- attach initializes rope length from actual player-center-to-anchor distance;
- no travelling hook projectile;
- no velocity reset on attach/detach.

## Non-scope

- player targets;
- rope obstruction;
- VFX.
