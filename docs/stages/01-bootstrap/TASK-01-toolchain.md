# Task 01 — Pin Toolchain and Bootstrap Constraints

**Status:** PLANNED  
**Stage:** 1 — Godot/C# Project Bootstrap

## Goal

Choose and document the exact development toolchain for the MVP bootstrap.

This task removes ambiguity about Godot, .NET, Jolt, and the initial desktop development target. It does **not** create the game project yet.

## Read first

- `AGENTS.md`
- `DESIGN.md`
- `docs/DECISIONS.md`
- `docs/ARCHITECTURE.md`
- `docs/ROADMAP.md`
- `docs/stages/01-bootstrap/README.md`

## Scope

1. Verify the currently appropriate stable Godot 4.x .NET/C# release from official Godot sources.
2. Pin one exact Godot version for Stage 1.
3. Determine and pin the required/compatible .NET SDK version.
4. Confirm Jolt as the intended 3D physics backend and document any project setting required to guarantee it.
5. Choose the initial development/CI desktop OS target. This is not the final supported-platform matrix.
6. Create `docs/TOOLCHAIN.md` as the authoritative toolchain document.
7. Update `docs/DECISIONS.md` if this resolves existing TBD/HYPOTHESIS entries.

## Non-scope

- `project.godot`;
- C# project files;
- scenes;
- tests;
- CI implementation;
- gameplay;
- exports/release packaging.

## Acceptance criteria

- Exact Godot version documented.
- Compatible .NET SDK documented precisely enough for automation.
- Jolt requirement explicit.
- Initial development/CI OS target explicit.
- Official-source rationale/links recorded.
- No contradictory toolchain statements remain.
- No gameplay decisions introduced.

## Verification

Cross-check Godot/.NET compatibility against current official documentation/release information and review the final diff for contradictory version references.

## Expected output

Primarily documentation. Task 02 must be able to create the project without choosing versions itself.
