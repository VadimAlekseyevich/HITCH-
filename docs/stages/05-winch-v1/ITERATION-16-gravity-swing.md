# Stage 5 — Iteration 16: Gravity Swing

**Status: IMPLEMENTED — awaiting human playtest**

## Human feedback

Iteration 15 fixed world-scale perception, but exposed two new feel issues:

- the post-burst sustained pull speed is too low;
- the grapple is too rail-like because gravity does not meaningfully influence travel while attached.

The requested fantasy is closer to a cable that can be swung on.

## Sustained radial pull

Current values:

- short/medium radial pull: **30 m/s**;
- long radial pull: **52 m/s**;
- long-range saturation distance: **62.5 m**.

This is intentionally higher than iteration 15's 22.5–40 m/s.

## Launch pulse

Current profile:

- immediate multiplier: **2.25×**;
- peak multiplier: **4.5×**;
- peak delay: **0.12 s**;
- decay: **0.72 s**.

At 52 m/s base:

```text
52 m/s sustained
  ↓ RMB
117 m/s immediate
  ↓ 0.12 s
234 m/s peak
  ↓ 0.72 s
52 m/s sustained
```

## Gravity during active grapple

Gravity is now part of the grapple simulation.

The active-cable update is:

```text
current velocity
  + gravity
  ↓
decompose relative to cable
  ├─ radial velocity
  └─ tangential velocity
  ↓
preserve tangent
replace radial with controlled inward pull
  ↓
move through collision solver
```

This means:

- sideways velocity around the anchor survives;
- gravity bends the tangent over time;
- the trajectory can become an arc rather than a straight line;
- outward radial velocity cannot overpower the cable;
- Space detach carries the full current swing velocity into free flight.

## Why this is not the old bad inertia

The earlier inertia problem allowed motion to keep dragging the player even after hard impacts and made the cable fight the desired direction.

Iteration 16 separates responsibilities:

- active cable controls radial travel;
- tangent is where swing lives;
- wall/ceiling collision still hard-stops;
- surface arrival still latches at zero velocity.

## Human gate focus

Test:

1. Does the player visibly sag/arc under gravity while attached?
2. Can an off-axis approach produce a useful swing around a building edge/roof anchor?
3. Does Space at the right point in the arc create a satisfying free-flight launch?
4. Is 30–52 m/s sustained pull high enough after the burst?
5. Does the 2.25× → 4.5× surge still feel explosive without dominating the whole flight?
6. Does any old unwanted wall-drag behavior return?


### Iteration 16 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36921058182

- build: success;
- warnings/errors: 0 / 0;
- tests: 74 passed / 0 failed;
- active-grapple gravity covered by automated tests;
- tangential swing velocity preservation covered by automated tests;
- retarget radial replacement + tangent preservation covered by integration tests;
- spawn smoke: stable and grounded;
- compact city enclosure smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual Stage 5 gate.
