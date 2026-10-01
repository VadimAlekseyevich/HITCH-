# tests/

Automated tests for HITCH! live here.

## Current harness

- Framework: **xUnit v3 4.0.1**
- Target framework: **net8.0**
- Project: `tests/HITCH.Tests/HITCH.Tests.csproj`
- Run from repository root with:

```text
build_and_run.bat test
```

or, when the pinned SDK is already active:

```text
dotnet test tests/HITCH.Tests/HITCH.Tests.csproj -c Debug
```

## Testing priorities

1. pure simulation calculations;
2. gameplay state transitions;
3. regressions;
4. input replay/snapshot behavior once networking starts;
5. targeted Godot/Jolt integration tests where engine behavior itself matters.

Prefer ordinary C# tests that do not launch Godot when engine behavior is not under test.

Use scene/engine integration tests only where Godot/Jolt behavior itself must be verified.
