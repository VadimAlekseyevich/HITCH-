# Task 06 — Add Rope/Debug Visualization and Telemetry

**Status:** PLANNED  
**Depends on:** Task 05

## Goal

Make movement tuning observable without creating production analytics or VFX.

## Debug overlay

Show at least:

- attached/detached;
- anchor/pull point;
- current/rest length;
- actual distance;
- reel velocity;
- current tension acceleration/force metric;
- player speed;
- reattach cooldown.

## Debug lines

Draw:

- aim/grapple ray;
- active rope line;
- anchor marker/cross;
- existing velocity/forward vectors.

## Telemetry

Keep lightweight in-memory development metrics:

- peak player speed;
- peak winch tension;
- attach count;
- detach count;
- a simple running/average speed metric.

No production analytics backend.
