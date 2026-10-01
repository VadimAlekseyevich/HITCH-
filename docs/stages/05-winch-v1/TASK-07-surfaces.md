# Task 07 — Add Explicit Forbidden Grapple Surface

**Status:** PLANNED  
**Depends on:** Task 06

## Goal

Prove that world geometry can remain physically collidable while being invalid for grapple attachment.

## Requirements

- reserve separate collision layers/masks for player movement vs grapple attachment;
- normal greybox geometry remains grapplable;
- add one clearly distinguishable movement-lab block that collides with the capsule but is not hit by the grapple ray mask;
- document the layer convention;
- no per-object hardcoded gameplay exceptions.
