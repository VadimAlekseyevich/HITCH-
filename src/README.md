# src/

Production C# code will live here after project bootstrap.

Do not add code before reading:

- `../AGENTS.md`
- `../DESIGN.md`
- `../docs/ARCHITECTURE.md`
- the active stage in `../docs/ROADMAP.md`

Planned ownership:

```text
src/
├── Simulation/   # Replayable gameplay state and rules
├── Godot/        # Engine/input/world/presentation adapters
├── Network/      # Transport, protocol, prediction, server/client sync
└── Debug/        # Development-only instrumentation
```

This is a conceptual map, not permission to pre-create speculative classes or frameworks.

The exact folder tree should evolve only when real implementation requires it.
