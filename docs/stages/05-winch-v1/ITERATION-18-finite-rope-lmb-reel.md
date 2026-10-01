# Stage 5 — Iteration 18: Finite Rope + LMB Reel

**Status: IMPLEMENTED — awaiting human playtest**

## Control model

- RMB: attach / retarget;
- LMB: start reel-in;
- Space with cable: detach;
- Space without cable: jump;
- one additional Space jump is available in mid-air.

RMB does not automatically reel.

A missed RMB does not silently detach an existing cable.

Repeated LMB while the cable is already reeling does not restart the reel-start envelope.

## Finite cable

Current maximum gameplay rope length: **100 m**.

The acquisition ray uses the same maximum.

This is intentionally large relative to a city block but not sufficient to grapple across the entire enclosed city.

## Attached idle state

Immediately after RMB:

- cable exists;
- reel is off;
- deployed rope length is the initial player-to-anchor path length;
- gravity is on;
- tangent momentum is preserved;
- a taut cable blocks further outward radial velocity.

This is a swing state, not an arrival latch.

## LMB reel

After LMB:

- deployed rope length decreases;
- current distance-based reel target is ~30–52 m/s;
- the existing restrained launch target curve is 1.35× → 2.0×;
- actual radial velocity approaches that target through bounded radial acceleration.

The velocity vector is therefore no longer snapped instantly when the active segment changes direction.

## Piecewise rope and bend stability

The iteration 17 polyline stays:

```text
player -> newest bend -> ... -> world anchor
```

with at most 4 bend contacts.

New stability rule:

- if adding a bend makes the geometric path longer, rope may pay out only that required increase;
- payout never exceeds the global 100 m max;
- over-length correction is bounded;
- radial direction changes are acceleration-bounded.

This specifically targets the reported unrealistically strong throw when an angle appears in the rope.

## Wall-adjacent behavior

Ordinary collision still removes only velocity into the contacted surface.

Tangential motion survives.

Because reel tension is now acceleration-bounded rather than full-vector snapping, wall contact + changing bend geometry should be less likely to enter the previous unstable spin.

This still requires human playtest verification.

## Faster locomotion

Current tuning:

- 13 m/s ground speed;
- 150 m/s² ground acceleration;
- 160 m/s² braking;
- 10 m/s ground jump;
- 9.5 m/s one-time air jump;
- 20 m/s² air acceleration;
- 12 m/s air-control target.

Landing restores the air jump.

## Verification

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36925626760

- 92 passed;
- 0 failed;
- 0 warnings;
- 0 errors;
- stable grounded spawn smoke;
- closed city shell smoke passed.

## Human gate focus

Test these specific behaviors:

1. RMB should visibly attach without immediately pulling.
2. LMB should clearly start reel-in.
3. A second LMB during the same reel should not create another burst.
4. 100 m should feel long-range but not effectively unlimited.
5. Swinging near a wall should no longer enter the old unstable spin.
6. A new bend should redirect tension without launching the player unrealistically.
7. Fast ground movement should feel useful without making the grapple irrelevant.
8. Ground jump + exactly one air jump should feel strong and predictable.
