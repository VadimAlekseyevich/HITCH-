# Stage 5 — Iteration 15: Compact City Scale

**Status: IMPLEMENTED — awaiting human playtest**

## Why this iteration exists

Repeated speed increases were being evaluated inside an enormous mostly empty room.

That made very high numerical velocity read as ordinary because:

- buildings/targets were hundreds of meters apart;
- the player capsule was visually tiny relative to the enclosure;
- there was little nearby geometry producing parallax;
- crossing the room took long enough that the player naturally asked for more speed.

Iteration 15 changes the scale first.

## New city volume

The closed test space is now approximately:

- **200 m wide**;
- **250 m deep**;
- **80 m high**.

It remains a full box:

- west wall;
- east wall;
- north/back wall;
- south/front wall;
- ceiling;
- floor.

The player should still be unable to leave the Stage 5 test volume.

## Greybox city

The old sparse tower field is replaced by a compact city layout with:

- low-rise buildings;
- mid-rise buildings;
- several taller ~50–70 m buildings;
- varied rectangular footprints;
- central avenue;
- cross streets;
- narrow gaps between blocks;
- roof caps;
- rooftop utility masses;
- sky bridges;
- antenna-like high targets;
- central landmark tower;
- one forbidden-grapple surface.

The goal is not environment art.

The goal is strong scale reference, parallax, occlusion, rooftop routes, street-canyon traversal, and frequent retarget opportunities.

## Movement scale

Iteration 14 sustained speed:

- 90–160 m/s.

Iteration 15 sustained speed:

- **22.5–40 m/s**.

Distance for the high end of the pull curve is reduced:

- 250 m → **62.5 m**.

This is intentionally the same approximate 4× scaling used on the test environment.

## Relative launch explosion

The burst is stronger relative to sustained travel:

- immediate multiplier: **2.0×**;
- peak multiplier: **4.0×**;
- peak delay: **0.12 s**;
- post-peak decay: **0.65 s**.

Long-line example:

```text
40 m/s sustained
   ↓ RMB
80 m/s immediate
   ↓ 0.12 s
160 m/s peak
   ↓ 0.65 s
40 m/s sustained
```

This should make every hook throw feel explosive without requiring the entire traversal model to live permanently at hundreds of meters per second.

## Controls retained

- RMB: grapple / retarget;
- Space with active cable: detach and preserve current velocity;
- Space without cable: jump;
- active grapple still owns direction and does not preserve old tangential inertia;
- wall/ceiling impacts still hard-stop;
- reaching an anchor still latches at zero velocity.

## Technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36919217312

- 72 passed / 0 failed;
- 0 warnings / 0 errors;
- stable grounded spawn;
- all enclosure boundaries detected by headless smoke.

## Human gate focus

The next playtest should answer:

1. Do 20–70 m buildings finally give an intuitive sense of player size and movement speed?
2. Does 22.5–40 m/s sustained travel now feel fast because nearby urban geometry moves past the camera?
3. Is the 2× → 4× launch surge strong enough relative to normal travel?
4. Are streets, roofs, alleys, and height differences creating useful retarget decisions?
5. Does Space-detach movement become more interesting when there are nearby buildings to thread between?
6. Does the box feel like a contained city test arena rather than a giant empty hangar?
