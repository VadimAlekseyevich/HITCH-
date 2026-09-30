# HITCH!

HITCH! is an experimental first-person PvP movement game built around a physically expressive winch/grapple.

The current project goal is **not to build the final game**. The goal is to build and evaluate the smallest networked MVP that can answer:

> Is PvP built around physically expressive winch movement fun even with primitive graphics?

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

The project is in **pre-implementation planning / roadmap definition**.

Do not treat planned APIs, class names, folder names, numerical tuning values, or networking details as final unless they are explicitly marked **DECIDED** in the project documentation.
