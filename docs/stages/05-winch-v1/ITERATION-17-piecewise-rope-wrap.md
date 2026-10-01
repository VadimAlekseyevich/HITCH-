# Stage 5 — Iteration 17: Piecewise Rope Wrap

**Status: IMPLEMENTED — awaiting human playtest**

## Human feedback

Iteration 16 made gravity-driven swinging possible, but the launch surge became too strong.

The next requested change has two parts:

1. reduce the launch pulse to a more natural level;
2. give the grapple conditional rope geometry so it can wrap around posts and building corners without simulating hundreds of rope particles.

## Calmer launch profile

Sustained radial pull remains:

- 30 m/s short/medium;
- 52 m/s long-range.

The launch pulse is reduced to:

- immediate: **1.35×**;
- peak: **2.0×**;
- peak delay: **0.14 s**;
- decay: **0.60 s**.

At the 52 m/s long-range base:

```text
52 m/s sustained
  ↓ RMB
~70 m/s immediate
  ↓ 0.14 s
~104 m/s peak
  ↓ 0.60 s
52 m/s sustained
```

This keeps a noticeable hook throw without the previous 234 m/s launch spike.

## Rope representation

The grapple rope is now represented as a deterministic piecewise-linear path:

```text
player
  ↓
newest bend
  ↓
older bend
  ↓
...
  ↓
world anchor
```

No rope particles, rigid bodies, or per-segment physical constraints are created.

The simulation stores at most **4 bend contacts**.

That is enough to test wrapping around several corners while keeping state compact enough for future prediction/networking.

## Wrap rule

Each active-grapple tick:

1. test whether the player has regained line of sight to the point behind the current bend;
2. if yes, pop the now-unnecessary bend;
3. test the visible segment from the player to the current pull point;
4. if an obstacle blocks that segment before its endpoint, create a new bend at the collision point;
5. offset the bend slightly away from the contacted surface to avoid immediately re-hitting the same face.

Current contact tuning:

- surface offset: 0.08 m;
- endpoint tolerance: 0.22 m;
- minimum contact spacing: 0.35 m.

## Pull behavior with bends

The cable pulls toward the **nearest bend**, not directly through geometry toward the original anchor.

Gravity and swing tangent behavior from iteration 16 remain active.

Total cable distance is the sum of every piece of the polyline.

The actual world anchor can only complete/latch when all intermediate bends have unwound.

## Intended movement

A rectangular post/building corner should now allow movement like:

```text
anchor behind post
       ●
       │
   bend●──── player
       │
       │ post

player swings farther around
       ● anchor
       │
 bend1 ●
       │
 bend2 ●──── player
```

As the player comes back around, bend2 and then bend1 should automatically disappear when direct visibility is restored.

## Debug presentation

The development rope line now renders the full polyline.

- yellow: rope segments / anchor;
- cyan: intermediate bend contacts;
- HUD: current bend count.

## Human gate focus

Test:

1. Does the reduced launch feel strong but natural?
2. Can the cable visibly catch a building corner instead of passing through it?
3. Can moving around a narrow building/post create additional bends?
4. Does the player swing around the nearest bend rather than through the obstacle?
5. Do bends disappear naturally when returning around the corner?
6. Are four contacts enough for the intended city traversal?
7. Does any bend jitter or rapid contact spam appear on flat walls?
