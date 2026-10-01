# src/

Production C# code lives here.

Read before modifying:

- `../AGENTS.md`
- `../DESIGN.md`
- `../docs/ARCHITECTURE.md`
- the active stage in `../docs/ROADMAP.md`

## Current structure

```text
src/
├── Simulation/
│   ├── Hitch.Simulation.csproj
│   ├── Input/
│   ├── State/
│   └── World/
│
└── Godot/
    ├── Bootstrap/
    └── World/
```

### Simulation

`Hitch.Simulation` is a plain `net8.0` assembly with **no Godot dependency**.

It owns replayable gameplay state/contracts and will own gameplay rules.

### Godot

Godot-side code adapts engine callbacks/world services/presentation to the simulation assembly.

Godot nodes must not become the only representation of gameplay state.

## Planned later ownership

```text
src/
├── Simulation/
│   ├── Player/
│   ├── Winch/
│   └── Combat/
├── Godot/
│   ├── Input/
│   ├── Presentation/
│   └── Adapters/
├── Network/
└── Debug/
```

Create these only when the relevant roadmap stage actually needs them.
