# HITCH! — MVP Development Roadmap

This roadmap covers the path from the current empty/documentation-only repository to the first networked MVP.

It is intentionally **stage-level**, not task-level.

Before implementing any stage, that stage will be decomposed using `WORKFLOW.md` into small tasks suitable for one AI-agent request each.

---

# Roadmap principles

## Build risk-first

The largest product risk is not art, content, menus, or backend scale.

The largest risk is:

> Can winch-driven movement become fun, controllable, learnable, and compatible with PvP?

The roadmap therefore validates movement before investing heavily in networking or content.

## Networking must influence architecture early, but implementation comes later

The first prototype is local.

However, its simulation structure must remain replayable and explicit so that client prediction/reconciliation can be added later.

## Do not polish invalidated systems

Every major stage has an exit gate.

If the core feel is bad, iterate that stage instead of covering the problem with more features.

---

# Stage 0 — Repository contract and planning foundation

**Status:** substantially complete once the planning files in this repository exist.

## Goal

Make the repository understandable to a new human or AI agent before any production code exists.

## Deliverables

- `README.md` repository map;
- `AGENTS.md` agent rules;
- `DESIGN.md` MVP design source of truth;
- `docs/DECISIONS.md`;
- `docs/ARCHITECTURE.md`;
- `docs/ROADMAP.md`;
- `docs/WORKFLOW.md`;
- placeholder README files describing future major directories.

## Important outcomes

A new agent should be able to answer:

- what is HITCH!;
- what is the MVP hypothesis;
- what is already decided;
- what is still TBD;
- what is deliberately out of scope;
- where future code belongs;
- which stage comes next.

## Exit criteria

- No production code is required.
- Documentation does not contradict itself on known MVP decisions.
- A future implementation task can point agents to stable source-of-truth documents.

---

# Stage 1 — Godot/C# project bootstrap

**Status: DONE — 2026-10-01**

**Task breakdown:** [docs/stages/01-bootstrap/README.md](./stages/01-bootstrap/README.md)

## Goal

Create the smallest reproducible Godot C# project that every later agent can build and test.

This stage does **not** implement gameplay.

## Deliverables

### Engine/toolchain

- pin an exact Godot 4.x version;
- enable C#/.NET support;
- document required .NET SDK version;
- configure Jolt as intended physics backend;
- choose desktop development target(s) for MVP.

### Repository structure

Materialize the production directory structure described conceptually in `ARCHITECTURE.md`.

Do not create dozens of empty abstractions.

### Build

Provide documented repeatable commands or scripts for:

- restore/build;
- run development build;
- run automated tests.

### Testing

Create the smallest test harness that allows ordinary C# simulation code to be tested.

Exact framework is chosen here.

### CI

Add a minimal GitHub Actions workflow that:

- restores dependencies;
- compiles;
- runs automated tests.

Do not add deployment/release pipelines yet.

### Configuration conventions

Choose how gameplay tuning configuration will be represented.

Requirements:

- strongly understandable from C#;
- editable without scattering constants;
- compatible with later debug tuning.

### Logging/error policy

Establish a minimal standard for:

- development logging;
- assertions/invariants;
- unexpected simulation errors.

## Explicit non-scope

- player movement;
- grapple;
- networking;
- combat;
- polished game launcher;
- production export pipeline;
- performance optimization.

## Exit criteria

From a clean checkout, an agent can follow repository instructions and:

1. build the project;
2. run the project;
3. run tests;
4. see CI pass.

No manual editor-only ritual should be required for basic verification if it can reasonably be automated.

---

# Stage 2 — Simulation kernel and explicit state

**Status: DONE — 2026-10-01**

**Task breakdown:** [docs/stages/02-simulation-kernel/README.md](./stages/02-simulation-kernel/README.md)

## Goal

Create the minimum gameplay-simulation skeleton needed for future movement without yet attempting to make movement fun.

The important result is **state ownership and fixed-step execution**, not features.

## Deliverables

### Simulation clock

- fixed-step gameplay update;
- render FPS separated from simulation step;
- initial configurable tick rate;
- support for testing 60 Hz vs 120 Hz later.

### Input representation

Define gameplay input independent of hardware.

The representation must be suitable for future buffering/replay.

Likely concepts:

- move intent;
- view/facing intent;
- jump;
- grapple attach/detach;
- reel input;
- fire;
- melee.

Do not freeze a network serialization format yet.

### Player state

Define the minimal explicit player simulation state needed by upcoming stages.

No unnecessary final-game fields.

### Simulation configuration

Centralize initial tuning values.

### World query boundary

Create the smallest explicit boundary through which simulation can ask the engine about:

- capsule movement/collision;
- raycasts;
- surface information.

### Debug runner

Make it possible to run/inspect simulation in a simple scene even before full gameplay exists.

## Automated validation

Add tests for:

- fixed-step execution;
- basic state transitions;
- configuration validation where relevant.

## Explicit non-scope

- satisfying movement feel;
- real winch forces;
- combat;
- multiplayer.

## Exit criteria

There is one obvious answer to:

> Where does gameplay state live and what function/process advances it by one simulation step?

The answer must not be "inside whatever Godot node currently happens to own it."

---

# Stage 3 — Greybox world, first-person shell, and debug instrumentation

**Status: DONE — 2026-10-01**

**Task breakdown:** [docs/stages/03-greybox-shell/README.md](./stages/03-greybox-shell/README.md)

## Goal

Create the environment in which movement experiments can be performed quickly.

## Deliverables

### Greybox test space

Build a temporary movement lab rather than the final MVP arena.

It should contain simple geometry for:

- flat movement;
- walls;
- ledges;
- vertical anchors;
- horizontal beams;
- open space;
- high-speed collision testing.

This is an engineering test map.

### First-person camera

Implement only enough camera behavior for movement evaluation.

Avoid final effects, weapon animation, head bob, etc.

### Device input adapter

Map keyboard/mouse into the simulation input representation.

Input mapping must remain outside core simulation.

### Debug overlay

Display at least:

- position if useful;
- velocity vector / speed;
- grounded/contact state;
- simulation tick rate.

Later stages add rope metrics.

### Debug visualization hooks

Provide a simple mechanism for drawing:

- collision normals;
- raycasts;
- future anchors;
- useful simulation vectors.

## Explicit non-scope

- final arena;
- final HUD;
- character model;
- animation system;
- audio.

## Exit criteria

An agent or developer can launch one scene and inspect simulation state while moving around a simple world.

Iteration must be cheap.

---

# Stage 4 — Base player locomotion

**Status: DONE — 2026-10-01**

**Task breakdown:** [docs/stages/04-locomotion/README.md](./stages/04-locomotion/README.md)

## Goal

Implement the non-winch movement foundation without accidentally making ordinary FPS movement overshadow the grapple.

## Deliverables

### Controlled capsule translation

Implement predictable capsule movement using explicit simulation velocity/state and Godot/Jolt world queries.

### Ground behavior

- grounded detection;
- stable movement over normal surfaces;
- sensible handling of steps/slopes only to the extent MVP needs them.

Do not build a general character-controller library.

### Walking

- intentionally slow;
- configurable acceleration/speed;
- no hidden dependence on render FPS.

### Jump

- intentionally weak;
- explicit state transition;
- configurable.

### Gravity

Configurable and owned by simulation.

### Air correction

Implement the deliberately weak air-steering assistance from `DESIGN.md`.

It must be possible to disable or tune it for comparison.

### External impulse path

Add an explicit way for gameplay systems to modify player velocity later.

This is required for:

- winch;
- collisions;
- future interactions.

### High-speed safety baseline

Begin detecting obvious tunneling/invalid movement at speeds above normal walking.

Do not solve theoretical extreme-speed cases yet.

## Experiments

Compare locomotion at candidate simulation rates (at least 60/120 if feasible).

Do not declare 120 Hz automatically superior; measure CPU cost and feel.

## Exit criteria

Without a winch:

- movement is stable;
- walking/jumping work;
- base movement is intentionally insufficient as the main traversal fantasy;
- state is observable and testable;
- external impulses can influence velocity cleanly.

This stage is successful even if ordinary locomotion is not particularly exciting.

---

# Stage 5 — Winch v1: world-anchor movement

**Status: READY FOR HUMAN GATE — ITERATION 8 SINGLE CABLE**

**Task breakdown:** [docs/stages/05-winch-v1/README.md](./stages/05-winch-v1/README.md)

## Goal

Answer the first core-feel question:

> Is attaching, swinging, reeling, detaching, and preserving momentum intrinsically enjoyable?

No networking and no combat are needed.

## Deliverables

### Grapple targeting

- hitscan/raycast targeting;
- no gameplay rope-length cap in the active prototype;
- valid vs forbidden grapple surfaces;
- debug visualization of hit/anchor.

### Winch state machine

At minimum:

- detached;
- attached;
- detach;
- reattach delay.

State transitions must be explicit.

### Rope/winch state

Represent the gameplay-relevant rope state explicitly.

Do not make visual rope state authoritative.

### Reel motor

Implement:

- reel-in;
- reel-out;
- current reel speed;
- acceleration;
- deceleration;
- configurable limits.

### Elastic force model v1

Implement a tunable strongly elastic model.

Important:

- this is an experiment;
- do not over-generalize it;
- coefficients must be visible/tunable.

### Tension behavior

Honor the design intent that the attached system generally tries to stay under tension.

Exact math is experimental.

### Momentum preservation

Detaching must preserve earned movement momentum almost completely.

### High-speed reel falloff experiment

Introduce a tunable mechanism by which reel-generated acceleration can weaken at high speed.

Do not introduce a permanent general speed cap yet.

### Winch debug overlay

Display:

- attached state;
- anchor;
- rope/rest/target length values as applicable;
- reel speed;
- tension/force;
- player speed.

### Basic telemetry

Record enough data to inspect:

- peak speeds;
- typical movement speeds;
- tension extremes;
- attach/detach frequency.

This can remain developer-only and simple.

## Experimental questions

This stage should explicitly test:

- spring strength;
- damping;
- reel acceleration;
- reel maximum speed;
- gravity;
- grapple reach/target behavior;
- reattach delay;
- air correction;
- 60 vs 120 simulation Hz if unresolved.

## Explicit non-scope

- grappling players;
- rope wrapping;
- player damage;
- networking;
- final VFX;
- weapons.

## Exit gate — CORE MOVEMENT GATE

Do **not** proceed merely because the feature list is complete.

Proceed when repeated manual play suggests:

- attaching/detaching is responsive;
- reeling changes trajectories in an expressive way;
- preserving momentum creates interesting movement;
- the system rewards timing rather than playing an animation for the user;
- moving around the greybox space is enjoyable enough to repeat voluntarily.

If this fails, iterate Stage 5.

---

# Stage 6 — Winch geometry interactions and robustness

## Goal

Make the local winch model robust enough for an arena and future PvP without turning the project into a full rope simulator.

## Deliverables

### Obstruction/contact precursor

Implement the simplified design where world obstruction can alter the effective rope path/contact.

The representation should permit future multiple contact points, but MVP behavior must stay intentionally limited.

### Contact add/remove rules

Define and test minimal behavior for:

- detecting obstruction;
- introducing a contact;
- removing a contact when line of sight clears;
- avoiding rapid contact flicker.

### Path debug visualization

Render all effective gameplay anchor/contact points clearly in debug mode.

### Edge-case handling

Test:

- attaching close to geometry;
- rapid reattachment;
- passing around corners;
- fast movement toward/away from anchor;
- reel direction reversal;
- collision while highly tensioned.

### Performance sanity check

Profile the local simulation in representative worst-case movement.

Only optimize measured problems.

## Explicit non-scope

- arbitrary physically correct rope wrapping;
- rope self-collision;
- knots;
- rope cutting;
- complex deformable rope.

## Exit criteria

The winch behaves consistently enough in representative greybox geometry that level layout does not constantly expose catastrophic rope bugs.

The simplified obstruction system remains understandable and bounded in scope.

---

# Stage 7 — Player physical interaction prototype

## Goal

Validate the second major physical pillar before networking:

> Can another player-shaped body meaningfully interact with winch forces and collisions?

Use a local proxy/test target before building network synchronization.

## Deliverables

### Player-shaped test body / second simulation entity

Provide a controllable or scripted second gameplay body sufficient to test two-body interactions locally.

Do not build split-screen or final local multiplayer unless it is actually the cheapest test method.

### Player grapple attachment points

Implement predefined attachment points.

On grapple hit:

- choose nearest valid attachment point;
- retain enough information for force direction.

### Two-body winch force

When one player/body grapples another:

- both gameplay bodies react;
- force transfer is physically meaningful;
- neither side is treated as an immovable anchor by default.

### Player/player collision

Implement physical pushing between player bodies.

### Impact rotation experiment

Prototype how strong impacts affect orientation while preserving controlled translation.

This must resolve enough of the current architecture TBD to support networked players later.

### First-person camera response experiment

Determine a minimally tolerable relationship between physical orientation changes and first-person camera.

Do not over-polish.

## Exit gate — PHYSICAL PvP FOUNDATION GATE

Local tests should demonstrate that:

- pulling another player-shaped body creates interesting trajectories;
- collisions are readable rather than random explosions;
- orientation response does not destroy control;
- the interaction is worth carrying into networking.

If not, revise the interaction model before networking multiplies its complexity.

---

# Stage 8 — Local combat prototype

## Goal

Add only enough combat to stress movement and create reasons to approach/avoid another player.

## Deliverables

### Health state

- explicit health;
- damage application;
- zero-health transition.

### Melee

One simple melee attack:

- high direct damage;
- low physical impulse;
- configurable timing/range/damage.

Do not build a general melee framework.

### Ranged hitscan weapon

One simple weapon:

- hitscan;
- low direct damage;
- unlimited ammo;
- no magazine/reload economy;
- configurable damage/fire interval.

### Hit validation boundary

Structure hit queries so server authority can later own actual combat validation.

Do not build final anti-cheat.

### Local death/reset prototype

Create a minimal local death/reset transition to test combat loops.

The final network respawn flow comes later.

## Explicit non-scope

- weapon inventory;
- pickups;
- recoil meta;
- multiple weapons;
- complex animations;
- collision damage;
- final void behavior.

## Exit criteria

Combat creates tactical reasons to use movement without becoming a better standalone FPS than a movement game.

If stationary aiming dominates, tune/reconsider combat before proceeding.

---

# Stage 9 — MVP arena greybox

## Goal

Replace the engineering movement lab with the single arena needed for networked MVP evaluation.

## Deliverables

### Arena geometry

One small open greybox arena with void around it.

Design geometry specifically to create:

- multiple useful grapple options;
- swing arcs;
- vertical recovery opportunities;
- pursuit/evasion;
- close-range melee opportunities;
- exposed crossings;
- rope obstruction cases;
- meaningful high-speed lines.

### Surface metadata

Clearly mark any forbidden grapple surfaces.

### Spawn locations

Add minimal spawn points suitable for two-player repeated fighting.

### Void placeholder behavior

If the final void rule remains TBD, use an explicit temporary reset/death behavior and mark it temporary.

## Explicit non-scope

- art pass;
- decorative clutter;
- multiple arenas;
- competitive map balancing;
- final spawn system.

## Exit criteria

The arena can support repeated local traversal/combat tests without needing the engineering lab for normal play.

---

# Stage 10 — Networking bootstrap and session shell

## Goal

Connect two game instances reliably before attempting sophisticated movement synchronization.

## Deliverables

### Transport

Implement the selected initial Godot/ENet-based connection path.

### Minimal session flow

Support development-friendly:

- host/server start;
- client connect;
- client disconnect;
- basic connection errors.

No matchmaking.

### Player identity

Define minimal network player identity/session ownership.

### Server process direction

Ensure architecture does not block a headless authoritative server.

A listen-server development mode may be used if useful, but do not let client presentation become server authority.

### Protocol/version sanity

Introduce the smallest mechanism needed to detect gross client/server protocol mismatch during development.

## Explicit non-scope

- prediction;
- reconciliation;
- networked combat;
- matchmaking;
- accounts;
- lobby UX;
- NAT traversal productization.

## Exit criteria

Two instances can establish a session and spawn network identities reliably enough for development.

Gameplay synchronization may still be primitive.

---

# Stage 11 — Authoritative movement networking

## Goal

Make one player's movement responsive locally while remaining server-authoritative.

This is the most technically risky networking stage.

## Deliverables

### Input commands

Represent and transmit ordered/timestamped simulation input suitable for replay.

### Server simulation

Server runs gameplay movement using the same simulation rules as the client.

### Client-side prediction

The local client immediately simulates its own input.

### Authoritative snapshots

Server returns enough authoritative player state to validate/correct the client.

### Reconciliation

Client can:

1. restore an authoritative past state;
2. discard acknowledged inputs;
3. replay remaining buffered inputs;
4. continue current simulation.

### Correction presentation

Prevent small corrections from looking unnecessarily harsh where smoothing is safe.

Do not hide large gameplay corrections that indicate a real bug.

### Remote interpolation

Render the other player's state smoothly between network updates.

Remote interpolation must not redefine authoritative gameplay state.

### Network debug tools

Expose:

- ping/RTT;
- input sequence;
- latest acknowledged input;
- prediction error magnitude;
- correction count;
- snapshot/update rate.

### Artificial network conditions

Provide a development method to test representative:

- latency;
- jitter;
- packet loss

if practical with available tooling.

## Explicit non-scope

- final bandwidth optimization;
- anti-cheat;
- global server infrastructure;
- matchmaking.

## Exit gate — RESPONSIVENESS GATE

Under realistic development latency, the local player's movement must still feel immediate.

Server corrections should be bounded and diagnosable.

Do not proceed to complex networked winch interactions while ordinary predicted movement is visibly unstable.

---

# Stage 12 — Networked winch: world anchors

## Goal

Synchronize the core winch movement under server authority without destroying local responsiveness.

## Deliverables

### Grapple command/state networking

Transmit/validate:

- grapple attempt;
- chosen target/anchor as appropriate;
- detach;
- reel input/state.

### Server validation

Server owns authoritative decisions such as:

- target validity;
- valid surface;
- attachment state.

### Predicted winch movement

Local player should receive immediate winch response.

### Reconciliation coverage

Replaying inputs must correctly reproduce:

- attach/detach transitions;
- reel motor evolution;
- elastic force effects;
- obstruction/contact state as designed.

### Remote representation

Remote player winch state and rope presentation are understandable.

### Desync diagnostics

Detect/log important disagreements in grapple state/contact path.

## Exit criteria

With latency, a player can traverse the arena using the winch without feeling that grapple actions wait for a round trip to the server.

The server remains authoritative.

---

# Stage 13 — Networked player-to-player grapple and collision

## Goal

Bring the defining PvP physical interaction online.

This stage should not begin until world-anchor networking is stable.

## Deliverables

### Networked predefined attachment points

Server validates player grapple hits and resulting attachment point.

### Coupled two-player winch forces

Server simulates the authoritative interaction between both players.

### Local prediction strategy

Determine the practical prediction policy when local movement depends on another networked player's state.

This may require approximations.

Do not assume ordinary single-player prediction extends trivially to coupled bodies.

### Player/player collision authority

Implement authoritative physical player interaction.

### Error handling

Test scenarios where:

- both players grapple each other;
- players collide at speed;
- one player grapples while the other changes direction;
- latency differs between clients.

### Orientation synchronization

Include whatever impact-rotation model was validated earlier.

## Exit gate — NETWORKED PHYSICS GATE

Two players should be able to pull and collide with each other under normal latency without:

- constant violent snapping;
- obvious duplicate realities;
- uncontrollable solver explosions;
- one client consistently gaining authority over the other.

If this gate fails, simplify/adjust prediction or interaction rules before adding more gameplay.

---

# Stage 14 — Networked combat, death, and respawn loop

## Goal

Complete the actual MVP gameplay loop.

## Deliverables

### Ranged weapon authority

Server validates hits/damage.

Client may provide immediate presentation feedback where safe, but damage truth is authoritative.

### Melee authority

Server validates melee timing/contact/damage.

### Health replication

Both clients receive correct health/death state.

### Death

Define minimal gameplay death transition.

### Respawn

Server owns respawn timing/location.

### Session continuity

No application restart is required.

Players can repeatedly:

```text
spawn → move → grapple → fight → die → respawn
```

### Void temporary rule

If still unresolved, choose/document a temporary MVP behavior sufficient to keep the loop running.

## Explicit non-scope

- scoreboard;
- match winner;
- ranked rules;
- post-match flow;
- progression.

## Exit criteria

The full loop works repeatedly between two networked instances.

---

# Stage 15 — MVP stabilization and evaluation build

## Goal

Stop adding features and make the experiment trustworthy enough to answer the product hypothesis.

## Deliverables

### Bug triage

Prioritize:

1. movement-breaking bugs;
2. desync/correction bugs;
3. winch state bugs;
4. death/respawn loop failures;
5. crash/data-loss issues;
6. minor presentation issues.

### Performance pass

Profile representative play.

Measure:

- simulation cost;
- render performance;
- GC allocations/pauses;
- network traffic/update rates;
- expensive collision queries.

Optimize measured bottlenecks only.

### Network-condition pass

Test at several representative latency/loss settings.

Record limitations honestly.

### Movement tuning pass

Tune parameters without adding mechanics.

Focus on:

- reel acceleration;
- spring behavior;
- damping;
- grapple target behavior;
- reattach delay;
- air assistance;
- gravity;
- weapon pressure.

### Minimal usability

Only enough UI/feedback to understand:

- health;
- death;
- connection failure;
- basic controls if needed for testers.

### Test build packaging

Produce a repeatable build for external/internal testers.

No store/release infrastructure needed.

## Exit criteria

The build is stable enough that tester feedback is about the game rather than primarily setup failures or catastrophic bugs.

---

# Stage 16 — MVP playtest and decision gate

## Goal

Answer the hypothesis rather than automatically continuing development.

## Primary question

> Would people willingly keep playing the greybox build because moving, grappling, chasing, and fighting another player is fun?

## Observe

Qualitatively:

- Do players voluntarily attempt advanced movement?
- Do they understand why they failed?
- Does improvement become visible?
- Is grappling another player fun or merely disruptive?
- Does shooting dominate movement?
- Do players create unexpected but understandable techniques?
- Do players want "one more fight" without content rewards?

Quantitatively, if useful:

- typical/peak speeds;
- grapple frequency;
- death sources;
- correction/desync frequency;
- session duration;
- repeated voluntary rematches/restarts.

Do not overbuild analytics for one experiment.

## Possible outcomes

### A. Core works

Proceed to post-MVP product design.

Only now discuss in depth:

- actual game modes;
- scoring;
- larger player counts;
- map philosophy;
- art direction;
- progression if any;
- matchmaking/ranked;
- production networking infrastructure.

### B. Movement is fun, PvP interaction is not

Return to physical interaction/combat stages.

Do not solve it by adding content.

### C. PvP is fun, networking undermines it

Return to networking/prediction model.

Consider simplifying interaction rules before changing engine.

### D. Core movement itself is not fun

Return to local winch stages.

This is a valid MVP result.

Do not proceed to productization.

---

# Dependency summary

```text
0  Documentation / contracts
│
1  Project bootstrap
│
2  Simulation kernel
│
3  Greybox lab + camera + debug
│
4  Base locomotion
│
5  Winch v1 world anchors
│   └── CORE MOVEMENT GATE
│
6  Geometry interaction robustness
│
7  Local player↔player physics prototype
│   └── PHYSICAL PvP FOUNDATION GATE
│
8  Local combat
│
9  MVP arena
│
10 Networking/session bootstrap
│
11 Predicted authoritative movement
│   └── RESPONSIVENESS GATE
│
12 Networked world-anchor winch
│
13 Networked player↔player physics
│   └── NETWORKED PHYSICS GATE
│
14 Networked combat + death + respawn
│
15 Stabilization/evaluation build
│
16 Playtest / go-back-or-forward decision
```

---

# What is intentionally absent from this roadmap

The following are not forgotten. They are intentionally postponed until the MVP hypothesis is answered:

- final art direction;
- final character;
- animation production pipeline;
- audio production;
- multiple maps;
- multiple modes;
- formal scoring;
- ranked;
- matchmaking;
- accounts;
- progression;
- cosmetics;
- monetization;
- achievements;
- Steam integration;
- localization;
- controller certification;
- console support;
- anti-cheat productization;
- live-service backend;
- replay/spectator product features;
- tutorials/onboarding beyond what MVP testers need.

Planning those systems now would create false certainty around a game whose core interaction is still experimental.
