# Task 03 — Implement Accelerating Reel Motor

**Status:** DONE  
**Depends on:** Task 02

## Goal

Make reel-in/out a motor with state rather than instantaneous rope-length edits.

## Requirements

- positive `ReelAxis` means reel in;
- negative `ReelAxis` means reel out;
- reel velocity accelerates toward configured target speed;
- releasing input decelerates motor toward zero;
- rope/rest length changes by motor velocity × fixed dt;
- enforce a small minimum rope length;
- no per-frame hidden constants;
- tests cover acceleration, reversal, deceleration, and min length.
