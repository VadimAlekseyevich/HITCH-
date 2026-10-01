# HITCH! — MVP Design Specification

> **Document purpose:** source of truth for humans and AI coding agents implementing the first playable MVP.
>
> **Status:** MVP design, evolving.
>
> **Priority rule:** if implementation, comments, or generated code contradict this file, this file wins unless explicitly updated.

---

## 1. Project hypothesis

**HITCH!** is a first-person PvP movement game built around a controllable winch/grapple.

The MVP exists to answer one question:

> **Is PvP built around physically expressive winch movement fun even with primitive graphics?**

The winch is not a traversal shortcut or a canned animation. It is the central movement system and a major source of player skill.

The desired experience is:

- the player creates movement through physics rather than triggering pre-authored moves;
- momentum matters;
- reeling in/out is an active skill;
- mistakes should be understandable and physically plausible;
- the game may assist slightly, but it must not perform impressive movement for the player;
- movement skill should matter more than shooting skill.

---

## 2. MVP definition

The MVP is **not** a content-complete game.

It is complete enough for evaluation when two players can:

1. connect to the same session;
2. move using the winch;
3. fight;
4. take damage;
5. die;
6. respawn;
7. repeat the loop without restarting the application.

The MVP must use a simple greybox arena and primitive visuals.

### Development order

#### Phase 0 — local movement prototype

Start with one local player.

Goal:

- make the winch enjoyable by itself;
- expose all important movement parameters for rapid tuning;
- validate the custom player simulation before networking.

#### Phase 1 — networked MVP

Add the second player as soon as the local movement model is promising.

Goal:

- validate that winch-based PvP is fun;
- complete the connect → fight → die → respawn loop.

---

## 3. Explicit non-goals for the MVP

Do **not** expand scope to these unless this document is updated:

- ranked matchmaking;
- progression;
- cosmetics;
- monetization;
- multiple maps;
- multiple game modes;
- classes/heroes;
- complex inventory;
- ammo economy;
- weapon pickups;
- map editor;
- sophisticated UI;
- final art style;
- final animation system;
- final audio;
- rope cutting;
- player-controlled rope escape/counter mechanics;
- full rope wrapping around arbitrary geometry;
- collision damage;
- final speed cap/balance;
- competitive balance.

The MVP is an experiment, not a vertical slice.

---

# 4. Core controls and movement

## 4.1 Camera

**DECIDED:** first-person for the MVP.

Reason:

- lowest implementation cost;
- fastest path to testing movement;
- final camera perspective is not locked by this decision.

Camera architecture should not make a future third-person experiment unnecessarily difficult.

---

## 4.2 Base locomotion

**DECIDED:**

- the winch remains the primary movement method;
- **CURRENT STAGE 5 OVERRIDE:** base locomotion has been brought back toward human scale after iteration 18 proved too fast;
- current ground speed target is about **8 m/s**;
- ground jump is about **5.8 m/s**;
- the previously requested one-time air jump remains, reduced to **4.8 m/s** and treated as a small gear-assisted hop rather than a second full-strength jump;
- gravity is approximately **11 m/s²**;
- wall/ceiling contacts remove only velocity into the surface; tangential motion survives so grapple swing/wrap can continue along geometry;
- air steering remains limited and does not clamp already-earned high momentum.

The player should not feel like a normal FPS character with a grapple added on top.

---

## 4.3 Player simulation model

**DECIDED:** custom-controlled capsule.

The authoritative gameplay movement should be computed by HITCH! code.

Jolt/Godot physics is primarily used for:

- collision queries;
- world geometry;
- contact detection;
- secondary physical objects.

Do not make the player's core locomotion depend entirely on a free-running rigid body solver.

### External impulses

Even though translation is custom-controlled, other gameplay systems must be able to modify player velocity.

Examples:

- collision with another player;
- winch pull;
- future impulse weapons;
- environmental interaction.

---

## 4.4 Player-to-player collision

**DECIDED:** players physically push each other.

This interaction must affect velocity and be gameplay-relevant.

The implementation may stabilize contacts to prevent solver explosions, but it must not reduce player collision to purely cosmetic overlap.

---

## 4.5 Rotation from impacts

**DECIDED:** a sufficiently strong impact can physically rotate the player.

This is gameplay intent, not just an animation requirement.

### Important implementation tension

The MVP also uses:

- a custom-controlled capsule;
- first-person camera.

Therefore, **do not automatically interpret this requirement as “hand complete player orientation to a rigid-body solver.”**

The exact representation of physical rotation is **TBD**.

Possible implementations may include a separate orientation state, body frame, visual body, or controlled angular response.

The chosen implementation must preserve responsive locomotion.

---

# 5. Winch / grapple

## 5.0 Current Stage 5 playtest override

**CURRENT PROTOTYPE HYPOTHESIS — ITERATION 20, DUAL-HOOK ODM + STRONG REEL.**

The dual-cable experiment from iteration 7 was rejected as unnecessary.

The active local movement prototype uses **two finite cables as an ODM-inspired approximation**:

- RMB clicks alternate cable slots: **left → right → left → right**;
- each RMB raycasts and attaches/replaces only that side's anchor;
- RMB by itself does **not** start reel-in;
- LMB starts reel-in on **every active cable**;
- repeated LMB clicks while a cable is already reeling do not restart that cable's launch envelope;
- Space detaches both cables and preserves the current full flight velocity.

### Finite gameplay rope length

The active Stage 5 tuning uses a **150 m maximum rope/acquisition length per cable**.

This is intentionally large relative to individual buildings but no longer allows effectively map-wide acquisition in the compact 200 × 250 m test city. The exact final maximum remains a tuning hypothesis, not a locked balance value.

### Current rope / reel / gas model

When attached but not reeling:

- gravity remains active;
- the rope keeps its deployed length;
- tangential swing momentum is preserved;
- when taut, the rope prevents further outward radial motion rather than continuously pulling inward.

After LMB starts reel-in:

- deployed rope length decreases over time;
- reel target is deliberately powerful at roughly **20–32 m/s** depending on line length;
- radial reel acceleration is about **110 m/s²**;
- a short controlled motor-assist envelope is **1.25× immediate → 1.65× peak**, then decays to sustained reel;
- a separate compressed-gas-like thrust adds about **11 m/s²** along each active cable tangent, based on the player's view direction;
- gas keeps full authority up to roughly **24 m/s**, then smoothly fades to zero added acceleration by roughly **46 m/s**;
- while both cables are active, each cable motor uses roughly **0.72×** normal motor authority so dual-hook control is stronger without blindly doubling all force;
- this gas fade is **not a hard speed cap**: gravity, stored momentum, and swing geometry may carry the player faster;
- radial velocity changes remain acceleration-bounded rather than snapping instantly to a new direction;
- a newly created bend may pay out only the geometric length needed by the new polyline, up to the global maximum, specifically to prevent corner creation from becoming a catapult;
- gravity and tangential swing momentum remain active during reel-in.

The design intent is that speed comes from combining reel, gas, gravity, and swing timing rather than receiving a scripted launch explosion.

World collision removes only velocity into a surface; legal tangential motion survives. This is intended to support wall-adjacent swing/wrap instead of making contact an automatic stop.

### Capsule-aware completion

A fixed center-to-anchor arrival distance is incorrect because the player is a capsule.

For example, with the current 1.8 m capsule, the center cannot physically approach a ceiling closer than roughly 0.92 m before collision margin/tolerance.

Therefore completion distance must account for:

- capsule radius;
- projected capsule half-segment length along the cable direction;
- collision margin;
- small configurable arrival contact tolerance.

When the capsule has effectively reached the grapple surface:

- the pull ends;
- the cable remains latched;
- **all player velocity is cleared and held at zero**;
- gravity/air control stay suppressed while latched;
- another RMB retargets; Space releases.

This rule exists specifically to prevent the rejected behavior where the player looked fully reeled in but continued to orbit/rotate because the cable never formally reached an unreachable surface point.

### Base locomotion playtest correction

Current local-prototype requirement:

- grounded movement must respond quickly to direction changes;
- releasing input should strongly brake grounded horizontal velocity;
- ground walking may dissipate excess momentum;
- limited air steering is allowed for corrections;
- grapple movement must remain substantially more powerful than walking.

### Current movement-lab usability

For Stage 5 feel testing only:

- the greybox lab is a fully enclosed high room;
- four walls and a ceiling prevent high-speed escape;
- multiple grappleable structures provide vertical routes;
- a small center-screen crosshair marks the exact aiming direction.

These are prototype-testing aids, not necessarily final arena/UI decisions.

---

## 5.1 Target acquisition

**DECIDED:** grapple targeting is hitscan.

Meaning:

- no travelling hook projectile in the MVP;
- pressing grapple immediately raycasts for a valid target.

---

## 5.2 Valid surfaces

**DECIDED:** the grapple can attach to almost any world surface except explicitly forbidden surfaces.

Implementation expectation:

- use collision layers, tags, groups, or equivalent data;
- do not hardcode individual objects in gameplay code.

---

## 5.3 Grapple range

**CURRENT STAGE 5 HYPOTHESIS:** finite maximum rope length.

Current playtest value:

- maximum acquisition/deployed rope length per cable: **150 m**.

Intent:

- large enough for aggressive city traversal;
- not large enough to target arbitrary geometry across the entire arena;
- use the same gameplay limit for the acquisition ray and deployed-rope state.

The exact final range remains open to human feel testing.

---

## 5.4 Detach and reattach

**DECIDED:** there is a very small reattachment delay.

Intent:

- nearly immediate re-grappling;
- no large cooldown;
- still gives the state machine a clear detach → reattach transition.

**Exact delay is not locked.**

---

## 5.5 Rope length control

**CURRENT STAGE 5 CONTROL:** RMB attaches/retargets; LMB starts reel-in; Space detaches.

Reel-in is directly controllable by the player. Reel-out is not currently bound as an active input in the Stage 5 prototype; geometry may pay out limited rope when a new bend makes the piecewise path longer.

The winch must not instantly rotate/snap the player's full velocity when rope direction changes.

### Reel motor behavior

**DECIDED:** the reel motor accelerates and decelerates.

Therefore the winch should have a state such as:

- current reel speed;
- target reel direction/speed;
- reel acceleration;
- reel deceleration.

Do not implement reel-in/out as a simple instantaneous length delta with no motor dynamics.

---

## 5.6 Elasticity

**DECIDED:** the rope/winch connection is strongly springy.

Elastic behavior is part of the movement mechanic rather than a cosmetic smoothing effect.

The player should be able to feel stored/released energy.

### No intended slack state

**DECIDED:** once attached, the system should try to keep the rope under tension rather than behaving like a freely dangling rope.

### Important unresolved detail

“Strongly springy” + “always tries to remain taut” does **not yet define the exact force law**.

Do not silently invent one permanent model.

The force law must remain easy to tune during Phase 0.

At minimum expose parameters for:

- target/rest length;
- spring strength;
- damping;
- reel acceleration;
- reel maximum speed;
- tension behavior near zero extension.

---

## 5.7 Energy gain from reeling

**DECIDED:** reeling can add meaningful kinetic energy to the player.

This is intended movement tech.

However:

- reel-generated acceleration should become less effective at high player speed;
- the MVP should not start with a hard maximum velocity.

The exact falloff curve is a tuning problem.

---

## 5.8 Speed limit

**DECIDED:** no hard or soft speed cap in the first local prototype.

Reason:

- first measure what speeds naturally emerge;
- only then design a cap/falloff model.

Instrumentation should record observed player speeds.

A future speed limit is expected, but its model and value are **TBD**.

---

## 5.9 Grappling another player

**DECIDED:** players are valid grapple targets.

When one player grapples another:

- both bodies influence each other physically;
- the interaction is not “pull only the attacker toward the target”;
- forces must respect both players' movement state.

### Attachment location

**DECIDED:** player characters expose several predefined grapple attachment points.

When a grapple hits a player:

- choose the nearest valid predefined point to the hit location;
- do not attach every grapple to the exact center;
- do not use arbitrary triangle-level attachment for the MVP.

This allows asymmetric pulling without requiring fully arbitrary body attachment.

---

## 5.10 Rope/world obstruction

Full general-purpose rope wrapping is **out of scope for the first MVP**.

However, the architecture must not assume that a rope is permanently a single unobstructed segment.

### MVP obstruction behavior

**DECIDED:** when world geometry intersects the rope path, the effective attachment path may move to the wall intersection point.

This is intended as a simplified precursor to future rope wrapping.

Implementation should therefore represent a rope path in a way that can evolve toward multiple contact/anchor points later.

Do **not** build the entire codebase around a permanent assumption of exactly one immutable line segment.

---

## 5.11 Rope escape/counterplay

**DECIDED:** no explicit escape mechanic in the MVP.

A player grappled by another player does not receive a dedicated “break rope” action.

Rope cutting, shooting the rope, or special counters are future design space.

---

# 6. Combat

Combat exists to support and stress the movement system.

Shooting skill must not replace movement skill as the main source of mastery.

---

## 6.1 Health and death

**DECIDED:** the primary combat death condition is health reaching zero.

Respawning is part of the MVP gameplay loop.

### TBD: falling out of the arena

The MVP arena has a void around it, but the exact gameplay result of falling into the void has not been explicitly locked.

Do not assume whether it:

- kills instantly;
- deals damage;
- teleports/respawns;
- has another rule.

For early prototyping, a temporary debug reset is acceptable, but this behavior must be marked as temporary.

---

## 6.2 Collision damage

**DECIDED:** no collision damage in the first MVP.

High-speed collisions should still:

- affect momentum;
- push/deflect players;
- create physical interaction.

Damage based on impact speed is intentionally deferred.

---

## 6.3 Melee weapon

The MVP has one melee weapon/action.

**DECIDED:**

- high direct damage;
- little effect on target physics.

Its first job is to create a meaningful close-range threat, not to serve as an impulse tool.

Exact:

- damage;
- range;
- attack rate;
- attack shape;
- cooldown

are tuning parameters and currently **TBD**.

---

## 6.4 Ranged weapon

The MVP has one simple ranged weapon.

**DECIDED:**

- hitscan;
- low direct damage;
- no magazine;
- no finite ammo economy.

The weapon should remain mechanically simple so that aim does not dominate MVP evaluation.

Exact damage and fire rate are **TBD**.

---

# 7. Arena

## 7.1 MVP arena shape

**DECIDED:** one small open arena surrounded by a void.

The arena should contain enough geometry to test:

- swinging;
- reeling;
- rapid reattachment;
- chase behavior;
- player-to-player grappling;
- high-speed traversal;
- vertical and horizontal momentum;
- line obstruction.

The arena is greybox only.

Do not spend meaningful MVP time on environment art.

---

# 8. Session / game loop

## 8.1 MVP game mode

**DECIDED:** free fight.

There is no score objective required for the first MVP.

The session does not need a formal match winner.

The purpose is repeated combat experimentation.

---

## 8.2 Player count

Development sequence:

1. one local player;
2. two players as soon as the movement prototype is viable.

Two-player PvP is the target MVP test.

Larger lobbies are out of scope.

---

# 9. Technical stack

## 9.1 Engine

**DECIDED / current technical hypothesis:**

- Godot 4.x;
- desktop-first;
- C# gameplay code;
- Jolt physics backend.

Do not introduce Unity or Unreal unless the project explicitly reopens the engine decision.

---

## 9.2 Why C#

All production gameplay logic is expected to be heavily AI-generated.

C# is preferred because:

- static typing catches many generated-code errors;
- compiler feedback is useful to coding agents;
- gameplay simulation can live in ordinary C# types rather than being tightly coupled to Godot scenes;
- it supports clearer interfaces and testable state transitions than a highly scene-dependent implementation.

---

## 9.3 Simulation architecture

Gameplay-critical simulation should be separated from Godot presentation code.

Target dependency direction:

```text
PlayerInput
    ↓
Gameplay Simulation
    ↓
PlayerState
    ↓
Godot presentation / camera / VFX
```

Suggested conceptual modules:

```text
Game/
├── Simulation/
│   ├── Player/
│   ├── Winch/
│   ├── Combat/
│   └── Shared/
├── Network/
└── Godot/
    ├── Presentation/
    ├── Input/
    └── SceneAdapters/
```

This structure is conceptual, not a mandatory exact folder tree.

### Core rule

**Godot Node hierarchy must not become the source of truth for gameplay state.**

Gameplay state should be representable as explicit data suitable for:

- local simulation;
- future server simulation;
- prediction;
- reconciliation;
- tests;
- replay/debug tooling.

---

## 9.4 Fixed timestep

Use a fixed simulation timestep.

Initial target/hypothesis:

- test 60 Hz vs 120 Hz;
- prefer 120 Hz if it materially improves high-speed winch feel and remains performant.

Do not tie gameplay behavior to render FPS.

All gameplay equations must use the simulation timestep correctly.

---

## 9.5 Physics backend responsibilities

Use Jolt/Godot physics for:

- collision detection;
- raycasts;
- sweep/shape casts;
- static world geometry;
- contact queries;
- non-critical secondary physics.

Prefer custom gameplay code for:

- player locomotion;
- winch force model;
- reel motor;
- movement assistance;
- gameplay velocity state.

Avoid making core winch behavior depend on opaque physics joints unless a test proves that approach is superior and compatible with future networking.

---

# 10. Networking direction

Networking is not the first development task, but the simulation must be designed so that networking does not require rewriting movement.

Expected future model:

- authoritative server;
- client-side prediction for the local player;
- server reconciliation;
- interpolation for remote players;
- ENet / Godot multiplayer transport as the initial candidate;
- headless dedicated server should remain possible.

Definitions:

- **client-side prediction:** the client immediately simulates its own input instead of waiting for the server;
- **reconciliation:** after authoritative server state arrives, the client corrects and replays inputs when necessary;
- **interpolation:** remote players are rendered smoothly between received network states.

### Networking design constraint

The same gameplay step logic should be callable by both client and server.

Conceptually:

```csharp
PlayerState Step(
    PlayerState previous,
    PlayerInput input,
    SimulationContext context,
    float dt
);
```

This exact API is not mandatory.

The important property is that core simulation is explicit and replayable.

---

# 11. AI coding-agent rules

This section exists because most implementation code is expected to be generated by AI.

## 11.1 Do not invent design

If a gameplay behavior is marked **TBD**, an agent must:

1. choose the simplest temporary implementation required to continue;
2. isolate it behind a parameter or small interface when practical;
3. clearly mark it as temporary;
4. not silently promote it into a permanent design rule.

---

## 11.2 Prefer explicit state

Prefer:

- typed state structures;
- small focused classes;
- explicit inputs and outputs;
- configurable parameters;
- deterministic order of operations;
- unit-testable pure calculations where practical.

Avoid:

- hidden mutable global state;
- gameplay logic scattered across scene scripts;
- string-based behavior dispatch when typed alternatives exist;
- implicit dependencies on scene-tree location;
- magic constants inside simulation code.

---

## 11.3 Hot-loop allocation rule

Avoid unnecessary managed allocations inside the fixed simulation loop.

In particular, do not repeatedly allocate:

- temporary lists;
- dictionaries;
- LINQ pipelines;
- short-lived wrapper objects

every physics step unless profiling proves the cost irrelevant.

---

## 11.4 Tuning rule

Movement constants must be configurable.

Do not bury values such as:

- gravity;
- walking acceleration;
- jump impulse;
- air correction;
- grapple range;
- spring strength;
- damping;
- reel acceleration;
- reel speed;
- grapple cooldown;
- damage;
- fire rate

inside algorithms.

Prefer one documented tuning/config layer.

---

## 11.5 Complexity rule

Do not implement a theoretically complete system when the MVP only needs a narrow experiment.

Examples:

- do not build a full rope simulation before the simplified obstruction model is evaluated;
- do not build production matchmaking to test two-player networking;
- do not build an ability framework for two fixed weapons;
- do not build a generic inventory system.

---

# 12. Required debug and tuning tooling

These are engineering requirements, not player-facing features.

The local prototype should make it easy to inspect at least:

- current player velocity;
- player speed;
- current rope length;
- target/rest rope length;
- rope tension/force;
- reel speed;
- current grapple anchor(s);
- current simulation tick rate;
- player health.

A developer/debug overlay is preferred.

### Parameter iteration

Movement/winch parameters should be editable without rewriting code.

If practical, support runtime tweaking in debug builds.

---

# 13. Metrics worth recording during movement tests

Before introducing a speed cap, collect simple data:

- peak player speed;
- typical traversal speed;
- time spent above candidate high-speed thresholds;
- reel-in duration;
- rope extension/tension extremes;
- number of grapple attaches/detaches;
- frequency of collision tunneling or invalid contacts.

The goal is not analytics infrastructure.

Simple debug logs or an in-memory sample buffer are sufficient.

---

# 14. Known design tensions / unresolved items

These are **real unresolved decisions**. Do not treat them as omissions to be automatically filled.

## 14.1 Physical rotation vs controlled capsule

Desired:

- controlled, predictable custom locomotion;
- strong impacts can rotate the player.

Implementation model is not yet selected.

---

## 14.2 First-person camera vs physical rotation

It is not decided how much physical body rotation should affect camera orientation.

Avoid making a nausea-inducing camera behavior permanent without testing.

---

## 14.3 Exact spring model

We know the rope should be:

- strongly elastic;
- effectively kept under tension;
- capable of storing/releasing useful movement energy.

The exact mathematical force model is still experimental.

---

## 14.4 Void behavior

The arena is surrounded by a void.

The exact death/reset behavior for leaving the arena is still TBD.

---

## 14.5 Speed limiting

Final speed limit behavior is intentionally not selected.

Phase 0 begins uncapped and gathers measurements.

---

## 14.6 Simplified rope obstruction

The chosen MVP behavior moves the effective attachment path to a wall intersection.

Exact rules for:

- adding the contact point;
- removing it;
- handling corners;
- multiple consecutive obstacles

must remain minimal until tested.

Do not accidentally turn this into a full rope-wrapping project.

---

# 15. First implementation target

The first meaningful build should contain:

- one greybox test arena;
- one first-person player;
- custom capsule movement;
- weak walking and jump;
- hitscan grapple;
- valid/invalid grapple surfaces;
- elastic tension model;
- reel-in;
- reel-out;
- reel acceleration/deceleration;
- momentum preservation on detach;
- weak air correction;
- debug visualization;
- exposed tuning parameters.

No networking is required for this first build.

The next target is two-player networking with the same simulation model.

---

# 16. MVP evaluation question

When the complete two-player MVP is playable, stop adding features and ask:

> **Would we willingly keep playing this greybox build solely because moving, grappling, chasing, and fighting another player feels good?**

If the answer is no, do not solve the problem with more content.

Fix the movement/PvP interaction first.

---

# 17. Document maintenance rules

For future AI agents:

- update this file when a design decision changes;
- preserve explicit **TBD** markers until a decision is actually made;
- distinguish design decisions from current implementation accidents;
- do not infer final-game requirements from MVP shortcuts;
- add rationale when a decision would otherwise look arbitrary;
- keep the document focused on behavior and constraints rather than code trivia.

Last conceptual update: initial MVP planning.
