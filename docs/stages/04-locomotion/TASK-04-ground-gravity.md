# Task 04 — Add Ground Detection and Gravity

**Status:** DONE  
**Depends on:** Task 03

## Goal

Make the capsule fall, land, and remain stably grounded on normal movement-lab surfaces.

## Requirements

- configurable gravity;
- short downward ground probe/sweep;
- configurable slope threshold based on surface normal;
- stable grounded flag;
- remove small downward velocity when grounded;
- walking off a ledge resumes gravity immediately;
- avoid hidden Godot CharacterBody floor state.

## Non-scope

- stairs/step-up beyond what current greybox needs;
- moving platforms;
- wall-running.
