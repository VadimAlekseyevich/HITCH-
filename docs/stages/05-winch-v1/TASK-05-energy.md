# Task 05 — Add High-Speed Reel Falloff and Verify Detach Momentum

**Status:** DONE  
**Depends on:** Task 04

## Goal

Allow reel timing to create speed while preventing reel-in from remaining equally powerful at arbitrarily high player speed.

## Requirements

- no hard/soft global velocity cap;
- high-speed falloff affects **reel-in motor effectiveness**, not all existing momentum;
- falloff start/end speeds are configurable;
- multiplier has a configurable non-zero minimum;
- reel-out is not weakened by this rule unless later testing justifies it;
- detach preserves exact current velocity except for ordinary simulation forces applied later;
- tests verify momentum preservation and falloff curve endpoints.
