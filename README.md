# HITCH!

HITCH! is an experimental first-person PvP movement game built around a physically expressive winch/grapple.

The current project goal is **not to build the final game**. The goal is to build and evaluate the smallest networked MVP that can answer:

> Is PvP built around physically expressive winch movement fun even with primitive graphics?

## One-click Windows build

After cloning the repository on Windows, double-click:

```text
build_and_run.bat
```

The script will:

1. use .NET SDK 8.0.425 if it is already installed, otherwise install it locally under `.tools/`;
2. restore and build the C# project;
3. restore and run automated tests;
4. download Godot 4.7.2 .NET locally under `.tools/` if needed;
5. launch HITCH!.

Nothing is installed system-wide and administrator privileges are not required for the intended path.

Optional command-line modes:

```text
build_and_run.bat build
build_and_run.bat test
build_and_run.bat smoke
build_and_run.bat run
```

See [docs/TOOLCHAIN.md](./docs/TOOLCHAIN.md) for pinned versions and bootstrap details.

## Start here

Humans and AI agents should read project documents in this order:

1. [AGENTS.md](./AGENTS.md) — rules for working in this repository.
2. [DESIGN.md](./DESIGN.md) — gameplay source of truth for the MVP.
3. [docs/DECISIONS.md](./docs/DECISIONS.md) — accepted design/technical decisions and unresolved decisions.
4. [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) — intended system boundaries and dependency direction.
5. [docs/ROADMAP.md](./docs/ROADMAP.md) — ordered development stages and exit gates.
6. [docs/WORKFLOW.md](./docs/WORKFLOW.md) — how roadmap stages will later be decomposed into one-agent tasks.

## Repository map

```text
/
├── AGENTS.md              # Mandatory instructions for coding agents
├── DESIGN.md              # MVP gameplay design source of truth
├── README.md              # Project entry point and repository map
│
├── docs/
│   ├── README.md          # Documentation index
│   ├── ARCHITECTURE.md    # System boundaries and dependency rules
│   ├── DECISIONS.md       # Decision log / accepted hypotheses / TBDs
│   ├── ROADMAP.md         # Ordered path to the MVP
│   └── WORKFLOW.md        # Task sizing and agent execution protocol
│
├── src/
│   └── README.md          # Planned production-code layout
│
├── tests/
│   └── README.md          # Planned automated-test layout
│
├── scenes/
│   └── README.md          # Planned Godot scene ownership rules
│
└── assets/
    └── README.md          # Planned non-code asset ownership rules
```

The directory tree above intentionally exists before production code. The repository should remain understandable from documentation alone.

## Current status

**Stages 1–4 are complete. Stage 5 remains in human iteration, now at iteration 17: piecewise rope wrapping in the compact city. The launch surge is calmed to 1.35× immediate / 2× peak, while the grapple path can add/remove a small number of geometric bend points so the cable wraps around building corners and posts as a polyline. Damage remains deliberately out of scope while core movement feel is being fixed.**

Do not treat planned APIs, class names, folder names, numerical tuning values, or networking details as final unless they are explicitly marked **DECIDED** in the project documentation.
