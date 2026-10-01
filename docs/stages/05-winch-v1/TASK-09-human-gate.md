# Task 09 — Human Core-Movement Gate

**Status:** WAITING — ITERATION 4 BUILD READY  
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

## Iteration 4 focus

Evaluate:

- does one RMB click feel immediate enough?
- does cable creation and pull clearly happen as one action?
- is the 30 m/s initial impulse strong enough?
- is 60 m/s² continued pull acceleration strong enough?
- does repeated RMB retargeting preserve useful momentum?
- does the player still retain understandable control at the higher pull strength?
