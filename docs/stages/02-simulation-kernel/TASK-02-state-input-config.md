# Task 02 — Define Config, Input, and Explicit State Contracts

**Status:** DONE  
**Depends on:** Task 01

## Goal

Define the minimum value-oriented contracts needed to represent a simulation tick without implementing movement.

## Scope

Create plain C# types for:

- simulation configuration, including configurable tick rate;
- simulation tick/index;
- player gameplay input independent from hardware APIs;
- minimal player state;
- top-level simulation state.

Use plain .NET numeric types (for example `System.Numerics`) rather than Godot types inside the simulation assembly.

Input must be suitable for later buffering/replay, but no network wire format is chosen yet.

State must be cheaply copyable/snapshot-able.

## Non-scope

- movement equations;
- input device mapping;
- health/combat state not needed yet;
- winch state;
- serialization;
- network sequence numbers.

## Acceptance criteria

- no Godot dependency;
- no scene references;
- no raw keyboard/mouse keys in simulation input;
- tick-rate configuration validates invalid values;
- state can be copied/snapshotted by value;
- tests cover important contract/validation behavior.
