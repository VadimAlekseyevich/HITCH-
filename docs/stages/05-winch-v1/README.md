# Stage 5 — Winch v1: World-Anchor Movement

Parent roadmap stage: [ROADMAP.md — Stage 5](../../ROADMAP.md)

**Status: READY FOR HUMAN GATE — ITERATION 7 DUAL CABLE**

## Objective

Build the first genuinely playable winch loop:

```text
aim → attach → tension/swing → reel in/out → detach → keep momentum → reattach
```

This stage is complete only after a **human core-movement playtest**, not merely green CI.

## Tasks

| ID | Task | Status | Depends on |
|---|---|---|---|
| 01 | [Define winch config, path abstraction, and explicit state](./TASK-01-state-config.md) | DONE | Stage 4 |
| 02 | [Implement hitscan attach/detach state machine](./TASK-02-attach.md) | DONE | 01 |
| 03 | [Implement accelerating reel motor](./TASK-03-reel.md) | DONE | 02 |
| 04 | [Implement elastic tension model v1](./TASK-04-spring.md) | DONE | 03 |
| 05 | [Add high-speed reel falloff and verify detach momentum](./TASK-05-energy.md) | DONE | 04 |
| 06 | [Add rope/debug visualization and telemetry](./TASK-06-debug-telemetry.md) | DONE | 05 |
| 07 | [Add explicit forbidden grapple surface to movement lab](./TASK-07-surfaces.md) | DONE | 06 |
| 08 | [Automated Stage 5 verification](./TASK-08-technical-gate.md) | DONE | 07 |
| 09 | [Human core-movement gate](./TASK-09-human-gate.md) | WAITING | 08 |

## Stage exit gate

Automated requirements:

- attach/detach/re-attach state transitions are explicit and tested;
- grapple is hitscan with configurable range and collision mask;
- winch state is simulation-owned;
- reel motor accelerates/decelerates;
- elastic tension is tunable;
- reeling can add kinetic energy;
- high-speed reel-in effectiveness can fall off without a general speed cap;
- detach preserves existing velocity;
- debug overlay/lines expose rope state;
- basic movement telemetry is visible;
- build/tests/headless smoke pass.

Human requirements:

- attaching/detaching feels immediate;
- reel timing meaningfully changes trajectory;
- spring behavior creates learnable movement rather than automated traversal;
- releasing the rope produces satisfying momentum;
- repeated greybox traversal is fun enough to continue.

If the human gate fails, tune/iterate Stage 5 instead of proceeding.


## Technical gate evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36800570203

- build: 0 warnings / 0 errors;
- tests: 56 passed / 0 failed;
- Godot headless smoke: success;
- editable tuning: `config/mvp_tuning.json`;
- play command: `build_and_run.bat`.

**Stage 5 is not DONE yet.** Task 09 requires a human playtest of movement feel.


## Human gate result — iteration 1

**Result: ITERATE / FAILED**

Observed problems from human playtest:

- inertia felt broken and excessively wild;
- the player could try to pull without feeling meaningfully pulled;
- releasing near a wall could still feel sticky;
- controls felt cumbersome;
- ordinary walking felt like sliding on ice.

The previous spring + accelerating reel model is no longer the active prototype.

## Iteration 2 — direct selected-point pull

Current active loop:

1. LMB selects/replaces a hitscan world point.
2. Selection alone applies no force.
3. Hold RMB to immediately pull toward that point at fixed configured speed.
4. Release RMB early to stop force and preserve momentum.
5. LMB while pulling retargets immediately.
6. Reaching the point consumes the target, zeroes velocity, and lets gravity make the player fall.
7. Q/E reel controls are disabled.

Ground locomotion is also being made more responsive with stronger braking and velocity-to-target control.

Iteration 2 must pass its own human playtest before Stage 5 can become DONE.


## Iteration 2 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36804660303

- build: success;
- warnings/errors: 0 / 0;
- tests: 58 passed / 0 failed;
- spawn stability smoke: `tick=45 y=0.9150 grounded=True`;
- current controls: LMB select point, hold RMB pull;
- current pull model: immediate direct velocity, no spring/reel acceleration;
- ground locomotion: responsive desired-velocity control with active braking.

Iteration 2 is now ready for the next human movement test.


## Human gate result — iteration 2

**Result: ITERATE / MISUNDERSTOOD CONTROL MODEL**

Human correction:

- LMB should behave like throwing/placing the cable point;
- RMB should be clicked once, not held;
- one RMB click should start strong automatic pull;
- clicking LMB during pull should retract/cancel the current pull and preserve inertia;
- the newly placed cable should remain idle until another RMB click;
- ordinary movement still needed faster acceleration and better control;
- grapple range should be much longer;
- the lab needed significantly more vertical geometry;
- aiming needed a small center-screen point.

Iteration 2 hold-to-pull semantics are superseded.

## Iteration 3 — click-to-start automatic pull

Current active loop:

1. LMB throws/selects a cable point and cancels any existing pull.
2. The cable is visible but idle.
3. One RMB click starts automatic pull.
4. Pull begins with an immediate impulse and continues with acceleration toward the anchor.
5. Existing tangential momentum is preserved.
6. LMB during pull cancels the pull immediately and preserves inertia while placing/replacing the cable.
7. Another RMB click is required to pull toward the new cable.
8. Reaching the anchor ends the pull automatically.

Current playtest changes:

- grapple range: 72 m;
- ground acceleration/braking greatly increased;
- limited air steering increased;
- movement lab enlarged to 120 × 150 m with towers reaching up to roughly 72 m;
- center-screen crosshair dot added.

## Iteration 3 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36805953261

- build: success;
- warnings/errors: 0 / 0;
- tests: 57 passed / 0 failed;
- headless spawn stability: `tick=45 y=0.9150 grounded=True`;
- one-click bootstrap remains `build_and_run.bat`.

Iteration 3 is technically ready. Stage 5 remains blocked on human movement feel.


## Human gate result — iteration 3

**Result: ITERATE / SIMPLIFY INPUT AGAIN**

Human correction:

- one RMB click should both shoot the cable and immediately start pulling;
- there should be no separate LMB grapple-placement action;
- pull should be noticeably faster/stronger.

Iteration 3 two-step cable-then-pull input is superseded.

## Iteration 4 — one-button grapple + pull

Current active loop:

1. Aim with the center crosshair.
2. Click RMB.
3. Raycast a fresh target.
4. Replace any previous cable.
5. Create the new cable and begin pull in the same simulation tick.
6. Apply an immediate pull impulse.
7. Continue automatic pull acceleration until arrival or another RMB retarget.
8. RMB miss clears the old cable and preserves current momentum.

Current pull tuning:

- grapple range: 72 m;
- initial impulse: 30 m/s;
- continuous acceleration: 60 m/s²;
- arrival distance: 0.9 m.

## Iteration 4 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36807075655

- build: success;
- warnings/errors: 0 / 0;
- tests: 55 passed / 0 failed;
- spawn stability smoke: `tick=45 y=0.9150 grounded=True`;
- one-click local run remains `build_and_run.bat`.

Iteration 4 is technically ready. Stage 5 remains blocked on human movement feel.


## Human gate result — iteration 4

**Result: ITERATE / PULL DID NOT FEEL LIKE REAL CONTRACTION**

Human feedback:

- cable creation/input was improved;
- however the player still did not feel strongly pulled toward the anchor;
- swing physics existed, but cable length did not feel like it was being aggressively shortened.

## Iteration 5 — strong radial contraction

Current force rule:

- preserve tangential velocity for swing;
- operate directly on radial speed toward the anchor;
- aggressively push inward radial speed toward **55 m/s**;
- allow radial speed to change at up to **420 m/s²**;
- retain the existing **30 m/s** initial RMB impulse;
- do not clamp already-faster inward motion.

Technical evidence:

- CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36807674608
- build: success;
- warnings/errors: 0 / 0;
- tests: 58 passed / 0 failed;
- spawn smoke: `tick=45 y=0.9150 grounded=True`.

Iteration 5 is ready for human feel testing.


## Human gate result — iteration 5

**Result: ITERATE / ARRIVAL AND TEST-SPACE PROBLEMS**

Human feedback:

- strong radial contraction finally made the cable visibly pull toward the anchor;
- the extreme pull values could now be reduced somewhat;
- completed pull still left unwanted motion/orbiting around the anchor;
- high-speed testing could leave the movement lab.

## Iteration 6 — enclosed room + completed-pull stop

Changes retained in the current build:

- pull initial impulse reduced to 24 m/s;
- radial acceleration reduced to 300 m/s²;
- target inward radial speed reduced to 42 m/s;
- movement lab enclosed with four continuous walls and a ceiling;
- when the last active cable reaches its anchor, player velocity is cleared completely;
- ordinary gravity resumes immediately afterward.

The headless smoke now verifies all four room walls and the ceiling.

## Human correction after iteration 6

Additional requested changes:

- gameplay rope length should be unlimited;
- add a second independent cable;
- LMB controls one cable and RMB controls the other;
- movement should feel inspired by fast dual-cable aerial traversal, but more action-oriented and less physically strict where useful.

## Iteration 7 — dual cable + unlimited gameplay length

Current active behavior:

- LMB shoots/replaces the left cable and immediately pulls;
- RMB shoots/replaces the right cable and immediately pulls;
- both cables can remain active simultaneously;
- missing with one side clears only that side;
- there is no gameplay rope-length cap;
- both pull corrections are computed symmetrically and summed;
- tangential momentum remains available for swinging;
- if one cable reaches its anchor while the other remains active, only the arrived cable clears;
- when the last active cable completes, player velocity is fully stopped.

### Iteration 7 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36809172303

- build: success;
- warnings/errors: 0 / 0;
- tests: 58 passed / 0 failed;
- spawn smoke: stable and grounded;
- enclosed-room smoke: west/east/back/front/ceiling all detected;
- unlimited gameplay range: old grapple-range tuning removed;
- dual-cable state/input/physics covered by automated tests.

Iteration 7 is ready for human movement testing.
