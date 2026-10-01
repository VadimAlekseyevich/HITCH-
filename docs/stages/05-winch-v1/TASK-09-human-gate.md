# Task 09 — Human Core-Movement Gate

**Status:** WAITING — ITERATION 8 SINGLE CABLE BUILD READY
**Depends on:** Task 08

## Goal

Play the greybox build and decide whether the central HITCH! movement hypothesis is promising.

## Run

On Windows after pulling/cloning:

```text
build_and_run.bat
```

## Evaluate

Do not judge visuals or content. Focus on:

- attach responsiveness;
- ability to understand tension;
- usefulness of reel-in/reel-out timing;
- ability to deliberately build/change momentum;
- quality of detach momentum;
- whether mistakes feel attributable to input/physics;
- whether traversal creates techniques worth practicing.

## Possible outcome

### PASS

Stage 5 may be marked DONE and Stage 6 planned.

### ITERATE

Keep Stage 5 ACTIVE. Record concrete feel problems and change tuning/model before proceeding.

CI cannot decide this gate.


## Iteration 1 result

**ITERATE**

The initial spring/reel implementation failed the feel gate. See the Stage 5 README for concrete feedback.

## Iteration 2 focus

Evaluate only:

- whether LMB target placement is obvious and reliable;
- whether RMB pull begins immediately;
- whether RMB release stops force immediately;
- whether LMB retargeting during a pull feels predictable;
- whether reaching a point cleanly drops the player instead of sticking;
- whether ordinary WASD ground motion no longer feels like ice;
- whether early RMB release creates useful, understandable momentum.


## Iteration 2 result

**ITERATE — control model misunderstood.**

Holding/releasing RMB is explicitly not the intended control scheme.

## Iteration 3 focus

Evaluate:

- does LMB feel like simply throwing/placing a cable?
- does one RMB click start a strong pull immediately?
- does the automatic pull continue correctly without holding RMB?
- does LMB during pull cancel force instantly while preserving useful inertia?
- does the new cable remain idle until the next RMB click?
- is ordinary WASD now responsive instead of sluggish/ice-like?
- is 72 m grapple range useful rather than excessive?
- does the taller arena create enough vertical flight opportunities?
- is the center crosshair accurate enough for deliberate cable placement?
- does preserving tangential momentum make trajectories feel player-authored instead of automated?


## Iteration 3 result

**ITERATE — input still had one unnecessary step.**

## Iteration 5 focus

Evaluate:

- does one RMB click feel immediate enough?
- does cable creation and pull clearly happen as one action?
- is the 30 m/s initial impulse strong enough?
- is 60 m/s² continued pull acceleration strong enough?
- does repeated RMB retargeting preserve useful momentum?
- does the player still retain understandable control at the higher pull strength?


## Iteration 5 result

**ITERATE — contraction worked, but completed pull and test-space behavior still needed correction.**

## Iteration 6 result

Retained changes:

- enclosed room;
- reduced pull values;
- full stop when the last cable completes.

## Iteration 7 focus

Evaluate only the current dual-cable build:

- does LMB clearly feel like the left cable and RMB like the right cable?
- can both cables stay active at once without feeling random?
- does firing the second cable produce a useful controllable change in trajectory?
- does one side retarget without destroying the other side?
- does missing with one side correctly clear only that side?
- does unlimited rope length remove arbitrary range frustration?
- does the closed room reliably contain high-speed movement?
- when one cable finishes and the other still pulls, does movement continue naturally?
- when the last cable finishes, does the complete stop feel correct rather than sticky or orbiting?
- does the dual-cable movement feel fast/action-oriented enough while still being understandable?


## Iteration 7 result

**ITERATE — second cable was unnecessary.**

The active build returns to a single RMB cable.

## Iteration 8 focus

Evaluate:

- does RMB-only grapple feel simpler/better again?
- does unlimited rope length still behave as intended?
- when pulling to a wall, does the cable complete only when the body is actually near the surface?
- when pulling to a ceiling, does the cable now complete instead of continuing to apply force forever?
- after full completion, is all sideways/orbital motion gone?
- does the player simply stop, then resume ordinary gravity without being slowly rotated around the anchor?
- does retargeting before completion still preserve useful movement flow?
- does the enclosed room continue to contain high-speed movement?
