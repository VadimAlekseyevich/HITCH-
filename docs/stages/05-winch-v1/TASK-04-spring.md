# Task 04 — Implement Elastic Tension Model v1

**Status:** DONE  
**Depends on:** Task 03

## Goal

Implement the first deliberately experimental spring/tension force model.

## Initial v1 model

Use a **one-sided spring with configurable pretension and damping**:

- rope pulls inward; it never pushes outward;
- tension increases with extension beyond the current rest length;
- a small configurable pretension distance can keep the line feeling taut near rest length;
- radial outward velocity is damped;
- inward velocity should not be damped as if the rope were a brake;
- resulting acceleration modifies simulation velocity before capsule movement;
- no general player speed cap.

The exact coefficients are tuning values, not design truth.

## Slack handling

If the player moves substantially inside the current rope length, a configurable slack-take-up mechanism may shorten the rest length toward current distance without applying an outward force.

## Tests

Cover:

- zero/near-zero distance safety;
- no outward push;
- stronger extension → stronger tension;
- outward radial motion increases damping contribution;
- detach means zero winch force.
