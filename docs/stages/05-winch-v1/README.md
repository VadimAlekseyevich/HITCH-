# Stage 5 — Winch v1: World-Anchor Movement

Parent roadmap stage: [ROADMAP.md — Stage 5](../../ROADMAP.md)

**Status: READY FOR HUMAN GATE — ITERATION 12 ODM SPEED**

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


## Human gate result — iteration 7

**Result: ITERATE / SECOND CABLE REJECTED**

Human feedback:

- the second independent cable was unnecessary;
- return to one cable;
- a small residual orbit/rotation was still visible after the player appeared fully reeled in.

The dual-cable state/input/physics are removed from the active implementation.

## Root cause of residual post-arrival rotation

The old completion rule used a fixed `0.9 m` center-to-anchor threshold.

That is geometrically wrong for the controlled capsule:

- capsule height: 1.8 m;
- capsule half-height: 0.9 m;
- collision margin: 0.02 m.

When grappling a ceiling, the capsule center cannot physically reach closer than roughly 0.92 m before collision, so a `0.9 m` threshold could leave the cable permanently active even though the player looked fully reeled in.

That continuing pull produced the small orbit/rotation.

## Iteration 8 — single unlimited cable + capsule-aware completion

Current behavior:

- RMB is the only grapple input;
- RMB raycasts/replaces the single cable and immediately starts pull;
- LMB has no grapple behavior;
- gameplay rope length remains unlimited;
- current pull values remain 24 m/s initial impulse, 300 m/s² radial acceleration, 42 m/s inward radial target;
- arrival is calculated from capsule geometry instead of a fixed center distance;
- current extra contact tolerance is 0.06 m;
- completed pull clears the cable and **fully zeroes residual velocity**;
- ordinary gravity resumes immediately.

The enclosed room and center crosshair remain.

Iteration 8 must pass a new human movement test before Stage 5 may advance.


### Iteration 8 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36810109190

- build: success;
- warnings/errors: 0 / 0;
- tests: 56 passed / 0 failed;
- spawn smoke: `tick=45 y=0.9150 grounded=True`;
- enclosed-room smoke: west/east/back/front/ceiling all detected;
- dual-cable implementation removed;
- no gameplay rope-length cap;
- fixed `ArrivalDistance` removed;
- capsule-aware surface completion covered by regression tests, including ceiling contact;
- completed pull clears residual velocity before ordinary gravity resumes.

Iteration 8 is technically ready for human movement testing.


## Human gate result — iteration 8

**Result: ITERATE / CORE FEEL STILL NOT THERE**

Human feedback:

- the movement room still felt too small for the intended high-speed traversal fantasy;
- the enclosed lab was too dark, making it hard to read what was happening at speed;
- after apparently reaching a grapple point, a small amount of continued motion/orbiting was still visible and irritating;
- even beyond those issues, traversal did not yet produce the desired high-energy aerial PvP feeling;
- fall/impact damage may eventually add stakes, but damage must not be used yet to hide a movement system that is not intrinsically fun.

Iteration 8 therefore does **not** pass the Stage 5 human gate.

## Iteration 9 — expanded bright lab + hard arrival settle

This iteration deliberately changes environment readability and arrival correctness without adding damage or combat.

### Movement lab

- enclosed room expanded from roughly **120 × 150 × 86 m** to **240 × 320 × 150 m**;
- major towers/anchors are spaced much farther apart;
- additional high-altitude bars and precision targets create longer traversal lines;
- ambient light is substantially stronger;
- greybox materials are brighter;
- a shadowless directional fill light keeps the closed room readable even under the ceiling.

### Grapple completion

Iteration 8 only checked capsule-aware arrival before locomotion movement.

At high speed that was insufficient: the capsule could reach/collide with the target surface during the locomotion step, while the cable remained active until the next simulation tick. That extra active tick could reintroduce the small orbit/jitter seen in human play.

Iteration 9 treats completion as an explicit hard-settle event:

1. pre-movement arrival still completes immediately;
2. the completion tick does not reapply gravity or air control;
3. after locomotion/collision movement, arrival is checked again;
4. if contact was reached during movement, the cable clears in the same tick;
5. final player velocity is exactly zero on the settle tick;
6. ordinary locomotion/gravity resumes on the next tick.

Automated coverage includes both immediate ceiling contact and high-speed arrival that occurs during the movement step.

### Iteration 9 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36911381535

- build: success;
- warnings/errors: 0 / 0;
- tests: 58 passed / 0 failed;
- spawn smoke: stable and grounded;
- expanded-room smoke: west/east/back/front/ceiling all detected.

### Human gate focus

Do not proceed to Stage 6 yet.

The next playtest should answer:

- does reaching a grapple point now feel completely stable, with no residual orbit/jitter;
- is the enlarged/brighter room easier to read at speed;
- do longer traversal lines make momentum and retargeting more satisfying;
- does movement begin to feel exciting on its own before damage, combat pressure, VFX, or audio are added.

If the answer to the last question is still no, continue iterating Stage 5 rather than adding damage as compensation.


## Human gate result — iteration 9

**Result: FAILED BADLY / SUPERSEDED**

Human feedback:

- the new lighting was overexposed and made the room effectively white and painful to read;
- the room still felt small for the intended movement fantasy;
- the player still oscillated around the target after visually reaching it;
- iteration 9 did not solve the reported problems and made presentation worse.

Root-cause correction:

- the remaining oscillation was not primarily a one-tick timing problem;
- arrival still depended on capsule-center distance to one exact anchor point;
- collision removes velocity into a wall/ceiling but preserves tangential velocity, so the player can touch the target surface, slide sideways, move outside the exact point-distance threshold, and remain under winch force;
- additionally, the implementation cleared the cable after completion and resumed gravity, so a genuine persistent stop was impossible even when velocity briefly reached zero.

## Iteration 10 — surface-aware latch + genuinely large arena

### Grapple arrival

The grapple raycast now stores both:

- anchor point;
- anchor surface normal.

Arrival against a real surface is evaluated as:

- capsule support distance to the anchor's surface plane;
- plus the existing contact tolerance;
- plus a separate tangential capture radius around the chosen point.

This allows a high-speed player who has physically reached the selected wall/ceiling near the anchor to complete the pull even if collision has preserved some sideways velocity.

Touching the same surface far away from the selected anchor does not count.

### Persistent stop

Completion no longer clears the cable.

A completed cable becomes **latched**:

- pull force stops;
- player velocity is exactly zero;
- gravity and air control are suppressed;
- position remains fixed while latched;
- another RMB click retargets immediately;
- an RMB miss releases the latch and ordinary locomotion/gravity resumes.

This is the first iteration where "pull to the point and actually stop there" is an explicit gameplay state rather than a one-frame side effect.

### Arena and lighting

The Stage 5 shell is now approximately:

- width: **800 m**;
- depth: **1000 m**;
- height: **320 m**.

Major landmarks are hundreds of meters apart.

Iteration 9's bright materials and strong ambient/fill lighting were removed. The current lab uses:

- dark blue-grey floor;
- distinct medium-dark walls/ceiling;
- moderately brighter obstacles for silhouette separation;
- low ambient energy;
- restrained shadowless fill only for minimum interior readability.

Damage/fall damage is still intentionally not added. First verify that the movement and arrival behavior themselves are correct.


### Iteration 10 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36912647386

- build: success;
- warnings/errors: 0 / 0;
- tests: 60 passed / 0 failed;
- spawn smoke: `tick=45 y=0.9150 grounded=True`;
- large-room smoke: west/east/back/front/ceiling all detected.

This technical result does not close the human gate. The iteration is only successful if the actual playtest no longer shows residual oscillation and the arena/lighting feel materially better.


## Human gate result — iteration 10

**Result: ITERATE / EXCESSIVE INERTIA IDENTIFIED**

Human feedback:

- the remaining movement problem is excessive inertia rather than only arrival geometry;
- even after hitting a wall, carried velocity drags the player away;
- momentum preservation is not helping the desired movement feel and should be removed from the active prototype.

## Iteration 11 — zero inertia

The active Stage 5 prototype now intentionally rejects carried grapple inertia.

While pulling:

- previous velocity is discarded;
- player velocity is set directly toward the active anchor every tick;
- ordinary gravity and air-control are bypassed during grapple travel;
- no tangential/orbital component survives from the previous trajectory.

On collision:

- walkable floor contacts still allow ordinary horizontal ground movement;
- wall, ceiling, and steep-surface impacts clear velocity completely instead of converting impact energy into sideways slide.

On release:

- RMB miss clears the cable and clears grapple-carried velocity;
- completed grapple remains latched at exactly zero velocity until retarget/release.

This is a deliberate arcade movement experiment. The previous Stage 5 momentum-preservation hypothesis is reopened and must not be silently restored.


### Iteration 11 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36914543762

- build: success;
- warnings/errors: 0 / 0;
- tests: 61 passed / 0 failed;
- spawn smoke: stable and grounded;
- room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual gate.


## Human gate result — iteration 11

**Result: IMPROVED CONTROL / SPEED STILL TOO LOW**

Human feedback:

- removing carried inertia made the movement noticeably better and more controllable;
- however the traversal still lacks the extreme speed expected from the intended Attack-on-Titan-like movement fantasy;
- 42 m/s direct travel reads as fast FPS traversal, not extraordinary aerial movement.

## Iteration 12 — ODM-scale direct speed

Control rules from iteration 11 stay intact:

- zero carried grapple inertia;
- immediate retarget direction;
- no gravity/air-control mixed into active pull;
- wall/ceiling impacts hard-stop;
- surface arrival latches at zero velocity.

Traversal speed is now deliberately exaggerated:

- short/medium grapple speed: **90 m/s** (~324 km/h);
- long-range maximum: **160 m/s** (~576 km/h);
- maximum is reached around **250 m** anchor distance;
- speed scales smoothly with line distance rather than switching abruptly.

Presentation now reinforces real speed:

- camera baseline FOV: **82°**;
- FOV expands smoothly toward **108°** as actual player speed approaches 160 m/s;
- FOV returns toward baseline as speed drops.

This iteration is specifically testing whether extreme direct speed, while retaining the new zero-inertia control model, moves the prototype closer to the intended high-energy aerial PvP fantasy.
