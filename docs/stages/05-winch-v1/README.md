# Stage 5 — Winch v1: World-Anchor Movement

Parent roadmap stage: [ROADMAP.md — Stage 5](../../ROADMAP.md)

**Status: READY FOR HUMAN GATE — ITERATION 18 FINITE ROPE + LMB REEL**

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


### Iteration 12 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36915605205

- build: success;
- warnings/errors: 0 / 0;
- tests: 65 passed / 0 failed;
- spawn smoke: stable and grounded;
- large-room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual gate.


## Human gate result — iteration 12

**Result: IMPROVED / REQUESTED LAUNCH BURST PROFILE**

Human feedback:

- the new ODM-scale 90–160 m/s traversal is substantially better;
- the next desired feel change is temporal rather than simply "more top speed";
- every fresh cable throw should feel strongest immediately;
- while the cable keeps pulling, that initial speed should gradually decay.

## Iteration 13 — launch burst + decay

Iteration 13 keeps the iteration 12 movement/control model intact and changes only the speed envelope.

Current launch profile:

- sustained distance-based speed remains **90–160 m/s**;
- a new grapple or retarget starts at **1.45×** the current distance-based pull speed;
- the launch bonus smoothly decays back to 1.0 over **0.75 s**;
- short-line launch speed is roughly **130 m/s**;
- maximum long-line launch speed is roughly **232 m/s** (~835 km/h);
- as travel continues, both the launch bonus and the existing distance-based speed naturally decrease.

The burst is explicit simulation state, not inherited momentum:

- previous velocity is still discarded;
- retargeting immediately points velocity at the new anchor;
- every retarget resets the burst timer;
- wall/ceiling hard-stop behavior remains;
- latched arrival remains exactly zero velocity.

The existing speed-responsive FOV automatically reacts to the higher launch velocity, producing a visual kick without adding a separate fake camera-only boost.


### Iteration 13 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36916365526

- build: success;
- warnings/errors: 0 / 0;
- tests: 69 passed / 0 failed;
- burst-decay curve covered by automated tests;
- pull elapsed time advances deterministically;
- retarget restarts the launch burst;
- spawn smoke: stable and grounded;
- room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual Stage 5 gate.


## Human gate result — iteration 13

**Result: IMPROVED / LAUNCH IMPACT STILL TOO WEAK**

Human feedback:

- the temporal burst idea is correct;
- the launch still does not feel explosive enough;
- desired profile is not merely "highest speed on frame zero";
- the cable should launch hard immediately, surge even harder almost immediately afterward, then decay naturally;
- Space should fully detach the current cable while preserving the current flight direction/speed so the player can convert a grapple burst into free flight.

## Iteration 14 — two-stage explosive surge + Space detach

The sustained ODM-speed model remains:

- short/medium sustained speed around **90 m/s**;
- long sustained speed up to **160 m/s**.

The launch envelope is now deliberately extreme:

1. **Immediate launch:** 1.75× sustained speed at t = 0.
2. **Second-stage surge:** smooth rise to 2.75× at t ≈ 0.10 s.
3. **Natural decay:** smooth fall from the peak back to normal over the next 0.70 s.

At the 160 m/s long-line base this is approximately:

- **280 m/s immediately**;
- **440 m/s peak** (~1580 km/h);
- then decay back toward **160 m/s**.

This is still explicit direct velocity, not accumulated physical inertia. A retarget discards the previous direction and starts a new two-stage surge toward the new anchor.

### Space detach

Space is context-sensitive:

- no grapple: ordinary jump intent;
- grapple active/latched: fully detach current cable;
- detach preserves the current player velocity;
- the same Space press is consumed as detach, so it does not also trigger a jump;
- after detach, ordinary gravity/air locomotion resumes.

This allows the player to deliberately detach during the launch peak and carry that speed into free flight.


### Iteration 14 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36918044665

- build: success;
- warnings/errors: 0 / 0;
- tests: 72 passed / 0 failed;
- two-stage launch curve covered by automated tests;
- explicit detach preserves velocity;
- Space detach is consumed instead of also triggering jump;
- spawn smoke: stable and grounded;
- room smoke: west/east/back/front/ceiling all detected.

Human feel remains the actual Stage 5 gate.


## Human gate result — iteration 14

**Result: GOOD DIRECTION / WORLD SCALE WAS DISTORTING SPEED JUDGMENT**

Human feedback:

- the player finally recognized that the giant empty test volume was making otherwise enormous numerical speeds feel too slow;
- absolute room scale, not only movement tuning, was driving repeated speed increases;
- the next test should shrink world scale and movement speed together rather than continuing to inflate velocity;
- the launch surge may remain relatively stronger;
- replace the abstract room with a greybox city of differently sized buildings while keeping a fully enclosed shell so the player cannot escape the test space.

## Iteration 15 — compact enclosed city scale

The movement space is reduced by roughly four times in each major linear dimension compared with iteration 14.

Current enclosed volume:

- width: **200 m**;
- depth: **250 m**;
- height: **80 m**.

The empty arena layout is replaced by a compact greybox city:

- roughly thirty building masses;
- building heights from low/mid-rise ~20–30 m up through ~60–70 m towers;
- varied footprints rather than identical pillars;
- one long central avenue;
- three major cross streets;
- narrower side alleys / street canyons;
- rooftop caps and small rooftop utility masses;
- two elevated traversal bridges;
- rooftop antenna targets;
- a small central tower/plaza landmark;
- one explicit forbidden-grapple billboard;
- outer walls and ceiling remain solid containment boundaries.

### Scale-corrected grapple speed

Absolute grapple speed is also reduced by roughly four times:

- short/medium sustained pull: **22.5 m/s**;
- long sustained pull: **40 m/s**;
- long-range speed reaches maximum around **62.5 m** rather than 250 m.

This preserves roughly the previous traversal-time relationship while putting nearby geometry back at a believable human/building scale.

### Stronger relative launch explosion

The relative launch pulse is intentionally stronger than iteration 14:

1. immediate launch: **2×** sustained speed;
2. second-stage peak after ~**0.12 s**: **4×** sustained speed;
3. decay back to sustained speed over ~**0.65 s**.

Maximum long-line example:

- sustained: **40 m/s**;
- immediate: **80 m/s**;
- peak: **160 m/s**;
- then natural decay back toward 40 m/s.

Space detach behavior from iteration 14 remains unchanged and still preserves current free-flight velocity.

### Iteration 15 technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36919217312

- build: success;
- warnings/errors: 0 / 0;
- tests: 72 passed / 0 failed;
- spawn smoke: stable and grounded;
- compact enclosed shell smoke: west/east/back/front/ceiling all detected.

The human gate now needs to judge **relative speed against readable urban scale**, not numerical velocity in isolation.


## Human gate result — iteration 15

**Result: CLEAR IMPROVEMENT / SUSTAINED SPEED TOO LOW + ACTIVE GRAPPLE TOO RIGID**

Human feedback:

- the compact city scale is a clear improvement;
- sustained pull speed after the launch explosion now falls too low;
- the active grapple currently feels too rigid because gravity does not meaningfully shape the trajectory;
- desired next step is the ability to swing/arc around the hook instead of only being directly translated toward it.

## Iteration 16 — gravity-driven swing + higher sustained speed

The compact 200 × 250 × 80 m enclosed city remains unchanged.

### Sustained speed

Post-burst radial pull is raised:

- short/medium: **30 m/s**;
- long: **52 m/s**;
- long-range maximum still reached around **62.5 m**.

### Stronger launch contrast

The launch profile is also increased relatively:

1. immediate: **2.25×** current radial pull speed;
2. peak after ~**0.12 s**: **4.5×**;
3. decay back to sustained radial speed over ~**0.72 s**.

At maximum long-range pull:

- sustained: **52 m/s**;
- immediate: **117 m/s**;
- peak: **234 m/s**;
- then decay to 52 m/s.

### Swing model

Gravity now applies during active grapple travel.

Each tick:

1. ordinary gravity is added to the player's current velocity;
2. velocity is decomposed into:
   - radial component along the cable;
   - tangential component around the cable;
3. tangential velocity is preserved;
4. the cable replaces the radial component with the configured inward pull speed;
5. radial motion away from the anchor is therefore not allowed to fight the cable.

The result is a controlled swing model:

- lateral momentum can carry the player around the hook;
- gravity curves the trajectory;
- the player can pass under/around an anchor rather than moving on a perfectly straight rail;
- Space detach preserves the resulting full velocity for free flight;
- a new RMB retarget establishes a new cable direction and a fresh launch surge.

### What is intentionally still prevented

This does **not** restore iteration 10's unwanted collision inertia:

- walls/ceilings still hard-stop velocity;
- impact energy is not converted into endless surface sliding;
- reaching the target surface still enters the stable latched stop.

The human gate should now judge whether the grapple finally feels like a swingable cable rather than a guided zipline.


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


## Human gate result — iteration 16

**Result: SWING DIRECTION IS RIGHT / BURST TOO STRONG / ROPE NEEDS GEOMETRY**

Human feedback:

- gravity-driven swing is a useful direction;
- the current 2.25× → 4.5× launch surge is too aggressive and should feel more natural;
- the next important grapple feature is conditional rope geometry;
- the cable should be able to catch building corners / posts so the player can rotate around them;
- do not simulate a rope as a large chain of independent physical particles;
- represent the cable as a small piecewise-linear path with geometric bend contacts.

## Iteration 17 — calmer burst + piecewise rope wrapping

### Launch profile

The sustained radial pull remains **30–52 m/s**.

The launch is reduced to:

- immediate: **1.35×**;
- peak: **2.0×**;
- peak delay: **0.14 s**;
- decay: **0.60 s**.

At maximum long-range pull this is approximately:

- 52 m/s sustained;
- 70 m/s immediate;
- 104 m/s peak;
- then back to 52 m/s.

### Piecewise rope state

The active cable now stores a deterministic polyline rather than only one straight segment.

Maximum path:

```text
player → newest bend → bend → bend → bend → world anchor
```

The state stores at most **4 bend contacts**.

This is intentionally compact for future prediction/networking and avoids a particle-chain rope.

### Wrap / unwrap behavior

- if the player-to-current-pull-point segment becomes blocked before its endpoint, a new bend is added at the contacted surface with a small outward offset;
- pull direction switches to that nearest bend;
- gravity/tangential swing continues around that bend;
- total cable distance is the sum of every polyline segment;
- if the player regains direct visibility to the point behind the newest bend, that bend is removed;
- several obsolete bends may unwind in one tick;
- reaching an intermediate bend never counts as reaching the actual grapple anchor.

### Debugging

- the full rope polyline is rendered;
- intermediate bend points are cyan;
- anchor/rope remains yellow;
- HUD displays bend count plus radial/tangent speed.

### City wrap targets

Three thin columns were added to the city specifically for rope-wrap playtesting:

- WrapPostA;
- WrapPostB;
- WrapPostC.

These make it easier to test winding/unwinding without relying only on large building corners.

The human gate should now focus on whether the rope actually feels like it catches and releases geometry naturally rather than whether the mathematical representation is physically exact.


### Iteration 17 collision correction

A follow-up human playtest found that automatic hard-stop on any wall contact caused the player to hang unnaturally against geometry even while the cable should continue pulling.

The active/current rule is now:

- remove only velocity into the collision normal;
- preserve tangential velocity along walls/ceilings;
- keep grapple active through ordinary contacts;
- allow rope wrapping / corner bends to redirect the pull;
- reserve full zero-velocity stop for actual anchor latch.

Older notes describing wall/ceiling hard-stop are historical behavior from earlier iterations, not the current Stage 5 rule.


## Human gate result — iteration 17

**Result: ROPE WRAP DIRECTION USEFUL / CONTROL AND TENSION MODEL NEED REWORK**

Human feedback:

- unlimited rope reach is too permissive; the cable should remain long-range but not reach arbitrarily across the whole city;
- attaching and reeling should become separate actions;
- RMB should attach/retarget only;
- reel-in should start only after an LMB click;
- occasional wall-adjacent pull behavior still enters unstable spinning;
- creating a rope corner can produce an unrealistically strong throw;
- ordinary movement should be much faster;
- ground jump should be strong;
- one additional jump should be available in mid-air.

## Iteration 18 — finite rope + explicit reel control + faster locomotion

### Input/state split

Current controls:

- **RMB:** attach or retarget a cable;
- **LMB:** start automatic reel-in for an already attached cable;
- **Space with cable:** detach while preserving full current velocity;
- **Space without cable:** jump / consume the one available air jump.

Important state change:

- attached but not reeling is a real free-swing state;
- it is no longer treated as the same thing as completed/latched arrival;
- repeated LMB while already reeling does not restart the launch envelope;
- RMB miss leaves an existing cable attached; Space is the explicit detach action.

### Finite rope

Current playtest maximum:

- **100 m** maximum acquisition distance;
- **100 m** maximum deployed rope length.

This is intentionally large for the compact city, but cannot target arbitrary geometry across the full ~200 × 250 m map.

The exact final maximum is still a tuning hypothesis.

### Rope-length/tension model

Rope behavior is now length-driven instead of continuously forcing the full velocity vector toward the nearest point.

When attached but not reeling:

- gravity remains active;
- deployed rope length stays fixed;
- tangential swing motion survives;
- when taut, the rope prevents further outward radial motion.

After LMB:

- rope length starts shrinking;
- reel target remains distance-scaled around **30–52 m/s**;
- the restrained start envelope remains **1.35× immediate → 2.0× peak**;
- radial velocity is allowed to approach the reel target only through the configured radial acceleration;
- the full velocity vector is never instantly rotated to a new bend direction.

### Bend-catapult correction

A newly created bend may increase the geometric polyline length without any real player motion.

To prevent that geometry change from becoming free kinetic energy:

- a new bend may pay out only the path-length increase required by the new polyline;
- payout is capped by the global 100 m rope maximum;
- radial correction caused by remaining over-length is capped;
- direction changes are acceleration-bounded;
- automated coverage verifies that an abrupt bend direction cannot create an instantaneous radial catapult.

### Faster base movement

Current Stage 5 locomotion experiment:

- ground max speed: **13 m/s**;
- ground acceleration: **150 m/s²**;
- ground braking: **160 m/s²**;
- ground jump: **10 m/s**;
- one air jump: **9.5 m/s**;
- air acceleration: **20 m/s²**;
- air control target speed: **12 m/s**.

Landing restores the single air jump.

This deliberately reopens the earlier slow/weak base-locomotion assumption for human feel testing.

### Technical evidence

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36925626760

- build: success;
- warnings/errors: 0 / 0;
- tests: **92 passed / 0 failed**;
- finite acquisition range covered;
- RMB attach without reel covered;
- LMB reel transition covered;
- repeated LMB does not restart burst;
- bend direction radial acceleration bound covered;
- one-air-jump consumption/restore covered;
- detach preserves horizontal and vertical swing momentum without adding a jump;
- spawn smoke: stable and grounded;
- compact-city enclosure smoke: west/east/back/front/ceiling all detected.

Human feel remains the Stage 5 gate.
