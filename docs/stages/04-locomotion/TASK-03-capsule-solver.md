# Task 03 — Implement Capsule Sweep/Slide Solver

**Status:** DONE  
**Depends on:** Task 02

## Goal

Move the explicit simulation capsule through the world without handing locomotion authority to a Godot CharacterBody/RigidBody.

## Requirements

- requested displacement comes from velocity × fixed dt;
- sweep before translation;
- move to safe fraction;
- remove velocity component into collision normal;
- continue remaining displacement for a small bounded number of slide iterations;
- avoid infinite zero-distance collision loops;
- state position/velocity remain authoritative in simulation;
- pure tests use fake world-query implementations.

## Non-scope

- step climbing framework;
- moving platforms;
- dynamic player collision;
- collision damage.
