# Task 08 — Automated Stage 5 Verification

**Status:** PLANNED  
**Depends on:** Task 07

## Goal

Verify implementation correctness before asking for subjective playtest feedback.

## Required evidence

- build succeeds with zero unexpected warnings/errors;
- all automated tests pass;
- Godot headless smoke passes;
- state transitions/reel/spring/falloff tests exist;
- no CharacterBody/RigidBody owns winch movement;
- no global speed cap exists;
- debug overlay/lines compile and run;
- `build_and_run.bat` remains the one-click play path.

Completing this task changes Stage 5 status to **READY FOR HUMAN GATE**, not DONE.
