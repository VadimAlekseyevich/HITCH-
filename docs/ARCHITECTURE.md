# HITCH! — Architecture Contract

This document defines intended boundaries before production code exists.

It deliberately avoids locking exact class names or APIs too early.

---

# 1. Architectural goals

The architecture must optimize for:

1. **responsive movement**;
2. **rapid winch tuning**;
3. **future authoritative networking without rewriting core movement**;
4. **AI-agent maintainability**;
5. **clear ownership of gameplay state**;
6. **cheap automated testing of simulation logic**.

The architecture is allowed to be small. It is not allowed to be ambiguous.

---

# 2. Primary dependency direction

Target conceptual flow:

```text
Input / Network Commands
          │
          ▼
   Gameplay Simulation
          │
          ▼
    Gameplay State
      ┌───┴────┐
      ▼        ▼
Presentation  Network serialization
(Godot)       / snapshots
```

Presentation observes gameplay state.

Presentation must not become the authority that determines gameplay state.

---

# 3. Planned top-level code ownership

The exact directory names may change during bootstrap, but ownership should remain recognizable.

```text
src/
├── Simulation/
│   ├── Shared/
│   ├── Player/
│   ├── Winch/
│   └── Combat/
│
├── Godot/
│   ├── Input/
│   ├── World/
│   ├── Presentation/
│   └── Adapters/
│
├── Network/
│   ├── Protocol/
│   ├── Client/
│   └── Server/
│
└── Debug/
```

## Simulation

Owns gameplay rules and replayable state transitions.

Examples:

- player velocity;
- movement state;
- health;
- grapple state;
- rope target/rest length;
- reel motor state;
- spring calculations;
- combat state transitions.

Simulation should know as little as practical about scene-tree presentation.

## Godot

Owns engine integration.

Examples:

- reading actual device input;
- camera;
- visual rope;
- meshes;
- audio;
- scene instantiation;
- collision/raycast adapter implementations;
- translating simulation state to transforms.

Godot integration is expected; the goal is not to pretend Godot does not exist.

The rule is that **Godot presentation state must not be the only representation of gameplay state**.

## Network

Owns transport and synchronization concerns.

Examples:

- serialization;
- input sequence numbers;
- snapshots;
- prediction buffers;
- reconciliation;
- remote interpolation;
- connection/session state.

Network code should consume the same simulation rules as local play.

## Debug

Owns development-only instrumentation and tuning surfaces.

Examples:

- velocity/tension overlay;
- anchor visualization;
- simulation tick information;
- parameter inspection;
- local movement telemetry.

---

# 4. World-query boundary

Core movement needs information from the world:

- raycasts for grapple targeting;
- collision sweeps for capsule movement;
- surface validity;
- obstruction checks;
- contact information.

These queries may use Godot/Jolt internally.

However, gameplay algorithms should consume them through a small explicit boundary rather than scattering direct scene queries everywhere.

Conceptually:

```text
Simulation
   │ asks
   ▼
World Query Interface
   │ implemented by
   ▼
Godot/Jolt adapter
```

Do not create an elaborate abstraction framework before needed. The boundary can begin very small.

---

# 5. Player state requirements

Before networking, core local-player gameplay state should be explicit enough to:

- inspect;
- copy/snapshot;
- replay from a previous state;
- serialize later;
- compare during tests.

Likely categories include:

- position;
- linear velocity;
- orientation or gameplay-relevant facing;
- grounded/contact state;
- health;
- current winch state;
- combat cooldown/state.

Exact fields are not frozen.

---

# 6. Input requirements

Simulation should receive gameplay intent, not raw hardware input.

Bad simulation input:

```text
"Mouse button 4 is down"
```

Better simulation input:

```text
GrapplePressed
ReelAxis
MoveAxis
JumpPressed
FirePressed
MeleePressed
View/facing intent
```

Raw keyboard/mouse/gamepad mapping belongs outside simulation.

This separation is required for future input replay during network reconciliation.

---

# 7. Fixed-step requirement

Gameplay simulation uses a fixed timestep independent of render FPS.

The exact rate is not yet decided.

The initial performance/feel experiment will compare 60 Hz and 120 Hz.

Do not encode gameplay as "per rendered frame."

---

# 8. Player physics model

The player uses a custom-controlled capsule as the primary locomotion representation.

Jolt/Godot provides collision/world information.

Gameplay code owns intended velocity/movement response.

This is chosen to retain control over:

- feel;
- prediction;
- replay;
- future reconciliation;
- extreme-speed behavior.

## Strong-impact rotation

Design requires strong impacts to be able to rotate the player.

How this interacts with the controlled capsule is still TBD.

Keep translational locomotion and orientation response separable enough to experiment.

---

# 9. Winch model

The winch is gameplay simulation, not merely a rendered rope.

At minimum it needs explicit state for:

- attached/detached;
- anchor/path information;
- current/rest/target length as required by chosen model;
- reel motor velocity/state;
- tuning data needed by the spring model.

## Rope representation

The first implementation may be simple, but its data model must not permanently assume that the rope can only ever have exactly one immutable straight segment.

Reason:

- MVP already includes a simplified obstruction/contact concept;
- future rope wrapping may introduce multiple path/contact points.

Do not implement full wrapping early.

Design extensibility here means avoiding a dead-end data model, not building the final system.

---

# 10. Combat model

Combat should remain intentionally small for MVP.

Simulation owns:

- health;
- damage application;
- death transition;
- respawn transition;
- weapon timing/state.

Presentation owns:

- muzzle flashes;
- hit effects;
- screen effects;
- sound;
- animation.

Do not build an inventory or generic ability system for two fixed MVP attacks.

---

# 11. Networking constraints on architecture

Networking arrives after local movement becomes promising, but local simulation must already permit it.

Expected future flow:

```text
Local input
   │
   ├──> immediate client simulation
   │
   └──> send input command to server

Server
   │
   └──> runs same gameplay rules authoritatively

Authoritative state
   │
   └──> client corrects old state and replays buffered inputs
```

Therefore:

- input must be representable as explicit commands/state;
- simulation order must be stable and intentional;
- state must be snapshot-able;
- local gameplay must not depend on non-replayable camera/VFX state.

Bit-perfect deterministic physics across machines is **not currently a requirement**.

---

# 12. Configuration and tuning

All high-impact movement values belong in a documented configuration layer.

Examples:

- gravity;
- walk acceleration/speed;
- jump impulse;
- air correction;
- grapple range;
- spring coefficients;
- damping;
- reel acceleration;
- reel speed;
- reattach delay;
- melee damage;
- ranged damage;
- fire interval.

The Stage 1 convention is defined in [`CONVENTIONS.md`](./CONVENTIONS.md): editor-facing tuning may use Godot `Resource` data, but gameplay simulation consumes a plain typed C# configuration snapshot rather than reading scene/editor state directly.

Requirements:

- values are not duplicated across systems;
- debug builds can inspect them;
- movement iteration is cheap.

---

# 13. Scene ownership

Godot scenes should compose engine-facing objects.

Scenes must not become undocumented service locators.

Avoid logic like:

```text
Find node ../../../../Player/Winch/Whatever and mutate its internal field
```

Prefer explicit references/adapters and clear ownership.

More specific scene rules will be added once the first Godot project structure exists.

---

# 14. Testability

Prefer unit-level simulation tests for logic that does not require real engine collision geometry.

Use integration/scene tests only where Godot/Jolt behavior itself is what must be verified.

High-value eventual test areas:

- reel acceleration/deceleration;
- spring-force math;
- detach momentum preservation;
- grapple state transitions;
- health/death/respawn;
- input replay;
- snapshot serialization;
- network reconciliation regressions.

---

# 15. Architectural non-goals

Before MVP, do not create:

- ECS solely because it might scale later;
- generic gameplay ability systems;
- service-container frameworks;
- event buses for every interaction;
- complex plugin/module loaders;
- production backend architecture;
- generalized rope simulation engines;
- generalized inventory/equipment frameworks.

Small explicit code is preferred until real complexity justifies abstraction.
