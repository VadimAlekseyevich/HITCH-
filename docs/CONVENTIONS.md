# HITCH! — Configuration and Diagnostics Conventions

These conventions are intentionally small. They exist to stop future AI agents from scattering tuning values and ad-hoc logging across gameplay code.

## 1. Gameplay tuning configuration

### Simulation owns typed snapshots

Gameplay simulation code should consume **plain typed C# configuration values**.

The simulation must not read values directly from UI widgets, scene-tree paths, environment variables, or arbitrary Godot nodes during a simulation step.

Conceptually:

```text
Godot/editor authoring data
        ↓ adapter/load boundary
typed C# tuning snapshot
        ↓
gameplay simulation
```

### Godot Resources may be used for authoring

When Stage 2+ introduces real movement/winch settings, Godot `Resource` assets are the preferred initial editor-facing authoring format because they are easy to inspect and tune.

The Godot-facing resource should be converted or copied into a plain simulation configuration object/snapshot before gameplay logic consumes it.

This keeps:

- editor tuning convenient;
- simulation tests independent from scenes;
- future prediction/replay code explicit;
- network/server simulation free from presentation ownership.

### One authoritative value per setting

Do not duplicate a gameplay value in multiple nodes/classes.

A parameter such as gravity, reel acceleration, spring strength, or grapple range should have one authoritative tuning source for a given runtime configuration.

### No magic gameplay constants

Values that materially change feel or balance must not be buried inside algorithms.

Temporary experimental constants are acceptable only when clearly labeled and easily promoted into configuration.

---

## 2. Diagnostics

### Godot-facing code

Use built-in Godot diagnostics:

- `GD.Print(...)` for intentional development information;
- `GD.PushWarning(...)` for recoverable suspicious conditions;
- `GD.PushError(...)` when an operation failed but the process can continue.

Do not create a custom logging framework during the MVP unless a measured need appears.

### Pure simulation code

Core simulation should avoid routine per-tick logging.

For programmer invariants in debug/development code, prefer `System.Diagnostics.Debug.Assert` when continuing execution is safe enough for debugging.

For impossible state where continuing would corrupt simulation or hide a real programming error, throw a specific standard exception such as `InvalidOperationException` or `ArgumentOutOfRangeException`.

Do not use exceptions for expected normal gameplay branches.

### Hot-loop rule

Do not emit normal logs every simulation tick.

Repeated diagnostics must be rate-limited, aggregated, or exposed through the future debug overlay rather than flooding output and allocating strings every frame.

---

## 3. Validation boundary

Validate authored configuration when it is loaded/converted, not repeatedly every simulation tick.

Examples of invalid authored values that should fail early:

- negative timestep;
- negative ranges that are defined as distances;
- NaN/infinite tuning values;
- logically impossible min/max relationships.

Exact validation rules belong to the subsystem that introduces each parameter.

---

## 4. Agent rule

When adding a new tunable gameplay parameter, an AI agent must answer:

1. which typed configuration object owns it;
2. how it is authored/loaded;
3. whether it needs validation;
4. whether tests need a custom value.

When reporting abnormal state, an agent must choose deliberately between:

- development info;
- warning;
- recoverable error;
- invariant/assertion;
- exception.

Do not invent a new diagnostics abstraction without a concrete need.
