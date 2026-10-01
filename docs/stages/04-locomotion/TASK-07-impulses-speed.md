# Task 07 — Add External Impulses and High-Speed/Timestep Checks

**Status:** PLANNED  
**Depends on:** Task 06

## Goal

Prepare locomotion for the future winch and inspect whether the controller behaves sensibly outside walking speeds.

## Requirements

- explicit API/state path to add an external velocity impulse;
- impulse composes with existing velocity rather than replacing it;
- no speed cap yet;
- test high requested displacements against sweep/slide behavior;
- add tests comparing representative fixed-duration free/controlled motion at 60 Hz and 120 Hz for timestep stability;
- keep simulation allocation-conscious.

## Non-scope

- winch force itself;
- collision damage;
- final soft speed cap.
