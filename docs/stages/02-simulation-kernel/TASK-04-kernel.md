# Task 04 — Implement One-Tick Simulation Kernel

**Status:** DONE  
**Depends on:** Tasks 02 and 03

## Goal

Create the single authoritative operation that advances explicit gameplay state by one fixed simulation tick.

## Scope

Implement a small simulation owner/kernel that:

- owns current `SimulationState`;
- owns/uses validated `SimulationConfig`;
- accepts one explicit player-input value;
- accepts the world-query boundary;
- advances the tick exactly once per call;
- exposes fixed delta derived from configuration;
- can be created from an explicit initial state.

Movement remains intentionally unchanged in this stage.

## Non-scope

- accumulator tied to render frames;
- walking/gravity/jump;
- collision response;
- network replay buffers;
- serialization.

## Acceptance criteria

- one call means one deterministic ordering of one simulation tick;
- tick increments exactly once;
- state is observable after the step;
- the same operation is callable from tests and, later, client/server code;
- tests prove tick advancement and snapshot behavior.
