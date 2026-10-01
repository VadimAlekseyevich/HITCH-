# Task 06 — Add Weak Jump and Weak Air Correction

**Status:** PLANNED  
**Depends on:** Task 05

## Goal

Provide minimal recovery/control while keeping the future winch much more important than ordinary locomotion.

## Requirements

- jump is an edge-triggered gameplay action;
- jump only starts from grounded state;
- configurable deliberately weak jump speed;
- air steering is significantly weaker than ground acceleration;
- air correction must not instantly erase existing momentum;
- air control can be tuned to zero;
- tests cover grounded jump and in-air correction.

## Non-scope

- coyote time;
- jump buffering;
- double jump;
- bunny-hop assistance.
