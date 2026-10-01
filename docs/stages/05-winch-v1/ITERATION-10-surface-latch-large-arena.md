# Stage 5 — Iteration 10: Surface Latch + Large Arena

**Status: IMPLEMENTED — awaiting human playtest**

## Why iteration 9 failed

Iteration 9 did not solve the reported Stage 5 problems.

It made the closed lab over-bright, the room still felt too small, and the player could still oscillate around an apparently reached grapple point.

The previous diagnosis was incomplete.

## Actual arrival problem

The winch stored only an exact world point.

When the capsule reached a wall or ceiling:

1. collision removed the velocity component into the surface;
2. tangential velocity remained;
3. the capsule slid sideways along the surface;
4. center-to-anchor distance increased again;
5. the cable remained active and pulled back toward the exact point.

That creates the visible side-to-side/orbital behavior even though the player has already physically reached the target surface.

Iteration 10 stores the raycast surface normal together with the anchor point and treats reaching the selected **surface near the selected point** as completion.

A configurable tangential capture radius prevents a completely different position on the same wall from counting as arrival.

## Real stop semantics

Iteration 9 also cleared the cable after completion and resumed gravity.

That meant a persistent stop was impossible by design.

Iteration 10 introduces a latched completion state:

- target remains selected;
- pulling stops;
- velocity remains exactly zero;
- gravity/air-control are suppressed while latched;
- the player stays at the reached position;
- RMB can immediately retarget;
- RMB miss releases the player.

## Arena

Current enclosed test volume:

- 800 m wide;
- 1000 m deep;
- 320 m high.

The major grapple landmarks are separated by hundreds of meters.

## Lighting

The iteration 9 white/overexposed look is removed.

Current palette is deliberately darker:

- dark slate floor;
- blue-grey walls and ceiling;
- medium blue-grey traversal geometry;
- low ambient lighting;
- one normal key light;
- low-energy shadowless fill only to avoid unreadable black regions.

## Out of scope

No fall damage, impact damage, combat, VFX speed effects, or new movement powers are added in this iteration.

The next human test should first verify:

1. no oscillation after reaching a grapple surface;
2. the player can actually remain stopped at the reached point;
3. the room finally feels large enough for sustained aerial traversal;
4. the lighting is readable without being washed out.
