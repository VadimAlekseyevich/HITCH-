# Stage 5 — Iteration 20: Dual-Hook ODM

**Status: IMPLEMENTED — awaiting human playtest**

## Why

Iteration 19 demonstrated that reducing reel strength too aggressively makes ODM-like movement less usable rather than more physical.

The new hypothesis is:

> strong reel-in is compatible with believable movement if rope geometry, acceleration, gravity, and dual-anchor control remain explicit.

## Controls

- RMB #1: left hook;
- RMB #2: right hook;
- RMB #3: left retarget;
- RMB #4: right retarget;
- repeat alternating;
- LMB: reel all attached hooks;
- Space: detach all attached hooks and preserve flight velocity.

## Cable state

Left and right hooks are independent:

- independent anchors;
- independent rope lengths;
- independent bend polylines;
- independent reel state;
- independent arrival state.

Both operate on one shared player body/velocity.

## Physics

Gravity is applied once per simulation tick.

Then cable forces are composed.

With one cable:

- full motor authority.

With two cables:

- each cable uses 0.72× motor authority.

The intent is to make dual hooks more powerful and more steerable without a naive 2× acceleration explosion.

## Reel values

- 20 m/s short sustained;
- 32 m/s long sustained;
- 110 m long-range blend distance;
- 110 m/s² radial motor acceleration;
- 1.25× immediate launch assist;
- 1.65× peak;
- 0.11 s to peak;
- 0.48 s decay.

## Gas

Gas is intentionally secondary to reel strength:

- 11 m/s² tangential acceleration;
- full authority through ~24 m/s;
- smooth fade to zero added gas by ~46 m/s;
- no hard speed clamp.

## Rope range

- 150 m per cable.

## World scale

The city shell is now approximately:

- 640 m wide;
- 800 m deep;
- 180 m high.

Horizontal footprint is ~10.24× the previous 200 × 250 m city.

Density is preserved through deterministic procedural blocks rather than stretching the old city.

Rough content:

- ~80 buildings;
- building heights ~24–105 m;
- three longitudinal avenues;
- three major cross streets;
- rooftop utility targets;
- wrap posts;
- far spires.

## Debug colors

- left cable: yellow;
- right cable: magenta;
- intermediate rope bends: cyan markers.

## Verification

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36931936584

- 101 passed;
- 0 failed;
- 0 warnings;
- 0 errors;
- stable spawn;
- closed enlarged room.

## Human gate focus

1. Is one cable now strong enough to make ordinary traversal viable?
2. Does the short 1.25× → 1.65× assist feel explosive without becoming the old teleport-like burst?
3. Does a second RMB naturally create the second hook?
4. Does LMB pulling both hooks feel controllable rather than chaotic?
5. Can two differently placed anchors noticeably steer the player between them?
6. Does the 0.72× per-cable dual scaling prevent absurd acceleration?
7. Does 150 m feel appropriate in the 640 × 800 m city?
8. Does the city feel genuinely larger while still retaining useful nearby geometry?
