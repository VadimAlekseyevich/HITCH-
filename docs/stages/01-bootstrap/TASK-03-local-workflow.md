# Task 03 — Create Repeatable Local Build/Run Workflow

**Status:** DONE  
**Stage:** 1 — Godot/C# Project Bootstrap  
**Depends on:** Task 02

## Goal

Turn the working project skeleton into a reproducible local workflow that an AI agent can invoke without guessing editor steps.

## Read first

- `AGENTS.md`
- `docs/TOOLCHAIN.md`
- `docs/ARCHITECTURE.md`
- Task 02 output
- this stage README

## Scope

Provide a small, documented command surface for operations later agents will repeatedly need.

At minimum define repeatable commands for:

- restore/build C#;
- run/launch the development project;
- an integration point for the test command that Task 04 will add;
- optionally one aggregate verification command if it stays simple.

Implementation may use scripts, a Makefile-like tool, PowerShell, shell scripts, or another small approach appropriate for the Stage 1 OS target.

Requirements:

1. commands delegate to the actual pinned Godot/.NET toolchain;
2. failures return non-zero status;
3. scripts do not silently install large dependencies;
4. commands are documented;
5. do not create a custom build system over Godot/.NET.

## Non-scope

- automated tests themselves;
- GitHub Actions;
- release exports;
- installers;
- gameplay developer console;
- hot reload tooling.

## Acceptance criteria

A fresh agent does not need hidden editor menu knowledge to:

- restore/build;
- launch the development project from documented instructions.

The workflow is small enough that CI can reuse the same underlying commands later.

## Verification

From repository root:

1. run documented build command;
2. confirm success;
3. run documented development launch command;
4. temporarily cause a build error and confirm the build command fails clearly;
5. revert the intentional error before commit.
