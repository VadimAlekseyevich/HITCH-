# Task 01 — Define Locomotion Tuning and Capsule Semantics

**Status:** DONE

## Goal

Define the explicit physical meaning and tuning values that the controller/queries will use.

## Requirements

- `PlayerState.Position` means the **center of the upright gameplay capsule**;
- capsule radius and total height are configurable;
- eye height is presentation data derived relative to capsule center;
- gravity, ground acceleration, ground max speed, jump speed, air acceleration/correction, ground-probe distance, collision margin, slope threshold, and solver iteration count are configurable;
- defaults intentionally make walking/jumping weak;
- all values validate;
- no Godot dependency enters the simulation assembly;
- tests cover invalid configuration.

## Non-scope

- collision solving;
- actual movement equations;
- winch tuning.
