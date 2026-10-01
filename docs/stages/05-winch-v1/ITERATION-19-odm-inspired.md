# Stage 5 — Iteration 19: ODM-Inspired Reel + Gas

**Status: IMPLEMENTED — awaiting human playtest**

## Goal

Move away from "extreme grapple speed" as the core feel.

This iteration approximates the energy structure of ODM-style movement:

- a finite anchor cable;
- gravity-driven arcs;
- a reel motor that changes radius;
- separate gas-like tangential propulsion;
- speed earned through timing and swing geometry.

This remains a single-cable prototype.

## Controls

- RMB: attach / retarget;
- LMB: engage automatic reel + gas propulsion;
- Space with cable: detach and keep current velocity;
- Space without cable: jump / consume the one reduced air hop.

## Human-scale locomotion

- ground speed: 8 m/s;
- acceleration: 50 m/s²;
- braking: 65 m/s²;
- gravity: 11 m/s²;
- ground jump: 5.8 m/s;
- one air hop: 4.8 m/s;
- air acceleration: 6 m/s²;
- air target speed: 7.5 m/s.

## Cable

- maximum length / acquisition range: 75 m;
- piecewise wrap path remains capped at 4 bend contacts;
- collision preserves legal tangent motion;
- bend creation still cannot instant-snap the full player velocity.

## Reel

- 9 m/s short-line target;
- 16 m/s long-line target;
- 55 m distance to maximum reel target;
- 40 m/s² radial acceleration;
- 1.0× immediate launch multiplier;
- 1.0× peak multiplier.

The active default therefore has no separate launch explosion.

## Gas

Gas propulsion acts only while reeling.

It is projected onto the plane tangent to the current cable segment, keeping propulsion and reel physically distinct.

- acceleration: 14 m/s²;
- full authority up to ~18 m/s;
- smoothly fades from 18 to 34 m/s;
- adds no further thrust at ~34 m/s and above.

There is no hard speed clamp.

## Camera

- base FOV: 80°;
- max speed FOV: 96°;
- speed effect begins around 12 m/s;
- reaches maximum around 36 m/s.

## Expected feel

Good play should come from:

- choosing a useful anchor;
- letting gravity establish a useful arc;
- reeling at the right time;
- aiming the body/view so gas adds useful tangential acceleration;
- releasing at the correct part of the swing.

Bad timing should feel slower instead of being rescued by a giant scripted burst.

## Verification

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36929379224

- 96 tests passed;
- 0 failed;
- 0 warnings;
- 0 errors;
- spawn smoke passed;
- enclosed-city smoke passed.

## Human gate focus

1. Is ordinary movement now slow enough that the gear matters without feeling sluggish?
2. Does RMB attach feel neutral rather than instantly launching the player?
3. Does LMB feel like a motor + gas system rather than a teleport toward the hook?
4. Can looking across the arc noticeably influence tangential acceleration?
5. Does speed build over a useful swing instead of appearing instantly?
6. Is ~34 m/s gas cutoff high enough for exciting traversal without returning to rocket movement?
7. Does a well-timed Space release feel substantially better than a badly timed one?
