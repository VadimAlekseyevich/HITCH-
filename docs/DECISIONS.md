# HITCH! Decision Ledger

This file is a compact index of decisions already made.

It is not a replacement for `DESIGN.md`. Use `DESIGN.md` for behavioral detail and rationale.

---

## Status vocabulary

- **DECIDED** — accepted for the MVP. Do not change implicitly.
- **HYPOTHESIS** — current implementation direction that must be validated.
- **TBD** — intentionally unresolved.
- **DEFERRED** — not needed to answer the MVP hypothesis.

---

# Product / MVP

| Topic | Status | Decision |
|---|---|---|
| MVP question | DECIDED | Test whether winch-centric PvP is fun even with primitive graphics. |
| Player count sequence | DECIDED | Build one local player first, then move to two players as early as practical. |
| MVP mode | DECIDED | Free fight; formal scoring/winner is unnecessary for the first evaluation. |
| MVP completion loop | DECIDED | Connect → fight → take damage → die → respawn → repeat. |
| Presentation | DECIDED | Greybox/primitive visuals are enough. |
| Camera | DECIDED | First-person for MVP implementation speed. |
| Final camera choice | TBD | MVP camera does not permanently decide final game perspective. |

# Player movement

| Topic | Status | Decision |
|---|---|---|
| Base walking | DECIDED | Slow. |
| Base jump | DECIDED | Weak. |
| Primary traversal | DECIDED | Winch/grapple. |
| Momentum on detach | HYPOTHESIS | Reopened in Stage 5 iteration 11. Active prototype clears grapple-carried velocity on release/miss instead of preserving inertia. |
| Air control | DECIDED | Very weak hidden correction only. |
| Player locomotion model | DECIDED | Custom-controlled capsule. |
| Player/player collision | DECIDED | Players physically push each other. |
| Strong-impact rotation | DECIDED | Strong impacts may physically rotate the player. |
| Exact controlled-capsule + rotation representation | TBD | Must be prototyped without sacrificing responsiveness. |
| Initial speed cap | DECIDED | No speed cap during first local measurements. |
| Final speed-limiting model | TBD | Choose after data/feel tests. |

# Winch / grapple

| Topic | Status | Decision |
|---|---|---|
| Targeting | DECIDED | Hitscan/raycast. |
| Current grapple control | DECIDED | RMB shoots/replaces the single active cable and starts pull immediately; LMB has no grapple action in the active prototype. |
| Valid world targets | DECIDED | Almost all surfaces except explicitly forbidden ones. |
| Range | DECIDED | No gameplay rope-length limit in the active MVP movement prototype. Finite ray length is engine-only. |
| Pull completion geometry | DECIDED | Completion uses anchor surface geometry plus capsule support; completion becomes a latched zero-velocity stop. |
| Reattachment delay | TBD | Previous cooldown model is not used by the active direct-pull playtest. |
| Reel control | TBD | Reopened after first human movement gate; Q/E reel controls are disabled in the current direct-pull iteration. |
| Reel behavior | TBD | Previous accelerating reel motor was rejected for the active playtest iteration. |
| Elasticity | TBD | Previous spring implementation failed the first human feel gate; direct-pull prototype is being tested before revisiting elasticity. |
| Slack | TBD | Not represented in the current selected-point direct-pull prototype. |
| Reel-generated energy | TBD | Reopened while direct-pull movement is evaluated. |
| Player grapple targets | DECIDED | Other players are valid targets and both players influence each other physically. |
| Player attachment location | DECIDED | Use nearest point from predefined attachment points. |
| Explicit rope escape | DEFERRED | No dedicated break/counter action in MVP. |
| Full rope wrapping | DEFERRED | Not part of first MVP implementation. |
| Obstruction precursor | DECIDED | Effective path may move to wall intersection; architecture must allow future multiple contacts. |
| Current Stage 5 pull model | HYPOTHESIS | Iteration 17: compact-city gravity swing with deterministic piecewise rope wrapping. Sustained radial pull remains 30–52 m/s; launch is reduced to 1.35× immediate / 2× peak at 0.14 s / 0.60 s decay. The rope is represented as player → up to 4 bend contacts → anchor. Obstruction adds a bend; restored visibility removes bends; pull targets the nearest bend while rope length is the sum of all polyline segments. Space preserves full velocity on detach; hard impacts still stop. |
| Exact spring force law | TBD | Previous one-sided spring experiment failed the first human movement gate and is inactive. |

# Combat

| Topic | Status | Decision |
|---|---|---|
| Health | DECIDED | Health reaching zero is the primary combat death condition. |
| Collision damage | DEFERRED | High-speed impacts affect motion but do not deal damage in first MVP. |
| Melee | DECIDED | One melee action/weapon: high direct damage, low physical impulse. |
| Ranged | DECIDED | One simple hitscan weapon: low damage, unlimited ammo/no magazine. |
| Aim importance | DECIDED | Secondary to movement skill. |
| Void behavior | TBD | Exact fall/reset/death rule is not yet fixed. |

# Arena

| Topic | Status | Decision |
|---|---|---|
| Map count | DECIDED | One MVP arena. |
| Shape | DECIDED | Small open arena with void around it. |
| Art | DECIDED | Greybox. |
| Current Stage 5 room | HYPOTHESIS | Enclosed 200 × 250 × 80 m greybox city lab with varied buildings, streets, four boundary walls, and ceiling; final MVP arena shape remains a separate design question. |

# Technology

| Topic | Status | Decision |
|---|---|---|
| Engine family | DECIDED | Godot 4.x. |
| Exact Godot version | DECIDED | Godot 4.7.2 stable .NET build. |
| Language | DECIDED | C#. |
| Physics backend | DECIDED | Jolt/Godot physics for collision/world queries and secondary physics. |
| .NET SDK | DECIDED | 8.0.425, pinned by `global.json`; game target framework is `net8.0`. |
| Test framework | DECIDED | xUnit v3 (4.0.1) for ordinary C# tests. |
| Tuning configuration convention | DECIDED | Godot/editor authoring may use Resources, but simulation consumes plain typed C# configuration snapshots. |
| Simulation assembly | DECIDED | `src/Simulation/Hitch.Simulation.csproj` is plain net8.0 and has no Godot dependency. |
| Simulation numeric types | DECIDED | Core simulation uses `System.Numerics`; Godot adapters convert at engine boundaries. |
| Core movement | DECIDED | Custom gameplay simulation rather than opaque rigid-body locomotion. |
| Core winch | DECIDED | Custom gameplay model rather than relying entirely on a physics joint. |
| Target platform | HYPOTHESIS | Desktop first. Stage 1 local/CI bootstrap targets Windows x86_64. |
| Simulation frequency | HYPOTHESIS | Temporary baseline is 60 Hz. Compare 60 Hz and 120 Hz during movement work; prefer 120 Hz only if value is measurable and affordable. |
| Networking transport | HYPOTHESIS | Godot multiplayer/ENet as initial transport candidate. |
| Server model | HYPOTHESIS | Authoritative server. |
| Local-player networking | HYPOTHESIS | Client-side prediction + reconciliation. |
| Remote-player rendering | HYPOTHESIS | Interpolation. |
| Dedicated server | HYPOTHESIS | Headless Godot server should remain possible. |

# AI implementation

| Topic | Status | Decision |
|---|---|---|
| Primary code author | DECIDED | AI agents will write most/all implementation code. |
| Code style priority | DECIDED | Explicit typed contracts, compiler feedback, small modules, testability. |
| TBD behavior | DECIDED | Agents must not silently convert temporary assumptions into design decisions. |
| Speculative architecture | DECIDED | Avoid frameworks/systems not required by current MVP stage. |

---

## How to change a decision

A task that changes a **DECIDED** item should:

1. explicitly state that the decision is being reopened;
2. update `DESIGN.md` when gameplay intent changes;
3. update this ledger;
4. update architecture/roadmap if affected;
5. mention the migration impact on existing implementation.
