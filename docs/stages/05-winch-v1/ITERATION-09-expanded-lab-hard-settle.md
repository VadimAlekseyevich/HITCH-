# Stage 5 — Iteration 9: Expanded Lab + Hard Arrival Settle

**Status: IMPLEMENTED — awaiting human playtest**

## Why iteration 9 exists

Iteration 8 did not pass the human gate.

Observed issues:

- the test room was still too small for the intended high-speed aerial movement fantasy;
- the enclosed room was too dark to read comfortably at speed;
- a small residual motion/orbit could still remain after the player appeared to have reached the grapple surface;
- overall traversal was not yet exciting enough to justify advancing Stage 5.

Damage is intentionally not part of this iteration. The current goal remains to make movement intrinsically enjoyable before adding punishment or combat stakes.

## Environment changes

The enclosed movement lab is now approximately:

- width: 240 m;
- depth: 320 m;
- height: 150 m.

The layout includes more widely separated vertical landmarks, higher traversal bars, additional precision anchors, and longer lines suitable for sustained movement.

Lighting/readability changes:

- brighter floor and obstacle materials;
- stronger ambient lighting;
- brighter key directional light;
- additional shadowless interior fill light so the closed ceiling does not make the arena unreadably dark.

## Hard arrival settle

The remaining post-arrival jitter was caused by simulation ordering.

Iteration 8 checked arrival before locomotion movement. At high speed the capsule could reach the target surface during the movement/collision step, but the cable would remain active until the following tick.

Iteration 9 fixes this by:

- exposing explicit winch completion for the current tick;
- suppressing locomotion/gravity on an immediate completion tick;
- re-checking capsule-aware arrival after locomotion/collision movement;
- clearing the cable and zeroing velocity immediately if contact is reached during that movement step.

This means the completed pull has an exact zero-velocity settle frame. Gravity resumes on the following tick.

## Verification

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36911381535

- build: success;
- warnings/errors: 0 / 0;
- tests: 58 passed / 0 failed;
- spawn smoke: `tick=45 y=0.9150 grounded=True`;
- expanded-room smoke: west/east/back/front/ceiling all detected.

Regression coverage now checks:

- immediate ceiling contact settles to zero velocity;
- gravity resumes on the tick after settle;
- high-speed approach that reaches the anchor during locomotion clears the cable in the same tick and ends with zero velocity;
- expanded room boundary smoke probes use the new room dimensions.

## Human playtest gate

Iteration 9 should be judged on:

1. no visible residual orbit or tugging at completed contact;
2. readability of the larger room at high speed;
3. whether long lines and larger gaps create more satisfying acceleration/momentum;
4. whether traversal itself starts producing the desired high-energy aerial PvP feeling.

Do not add fall/impact damage merely to make an otherwise weak movement loop feel consequential. If movement still lacks excitement, iterate its mechanics first.
