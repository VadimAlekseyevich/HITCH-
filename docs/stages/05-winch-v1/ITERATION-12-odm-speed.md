# Stage 5 — Iteration 12: ODM-Scale Speed

**Status: IMPLEMENTED — awaiting human playtest**

## Human feedback

Iteration 11 materially improved control by removing carried grapple inertia.

The remaining complaint is speed: the direct 42 m/s pull still feels like ordinary fast traversal rather than the exaggerated aerial movement fantasy that motivates HITCH!.

## Movement rule

Iteration 12 keeps the zero-inertia control model.

While pulling:

- prior velocity is discarded;
- velocity points directly at the active anchor;
- ordinary gravity and air control remain suppressed;
- retargeting changes direction immediately;
- walls/ceilings still hard-stop instead of converting impact into sideways slide.

## Extreme direct speed

Actual grapple travel speed is now distance-scaled.

Current prototype values:

- base direct pull speed: **90 m/s**;
- long-range direct pull speed: **160 m/s**;
- full long-range speed reached around **250 m**;
- smoothstep interpolation avoids an abrupt speed threshold.

Approximate equivalents:

- 90 m/s ≈ **324 km/h**;
- 160 m/s ≈ **576 km/h**.

A long grapple should therefore cross hundreds of meters in a few seconds rather than feeling like a conventional zipline.

## Speed presentation

The camera now responds to actual velocity:

- base FOV: **82°**;
- maximum speed FOV: **108°**;
- widening starts above roughly **20 m/s**;
- maximum is reached around **160 m/s**;
- FOV changes are smoothed over multiple physics ticks.

The visual effect supplements actual movement speed; it is not a substitute for it.

## Human gate focus

Test:

1. whether long grapple lines now feel genuinely extreme rather than merely fast;
2. whether 90–160 m/s remains controllable with zero inertia;
3. whether immediate retargeting at these speeds feels exciting or too abrupt;
4. whether FOV expansion improves speed perception without becoming uncomfortable;
5. whether the large 800 × 1000 × 320 m lab finally has enough scale to exploit the speed.

Do not proceed to damage/combat to compensate for weak traversal feel.
