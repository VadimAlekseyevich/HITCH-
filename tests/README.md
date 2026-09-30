# tests/

Automated tests for HITCH! will live here or in the exact structure selected during Stage 1 bootstrap.

Testing priorities:

1. pure simulation calculations;
2. gameplay state transitions;
3. regressions;
4. input replay/snapshot behavior once networking starts;
5. targeted Godot/Jolt integration tests where engine behavior itself matters.

Avoid brittle scene-level tests when a smaller simulation test can verify the same contract.

The exact C# test framework is intentionally TBD until Stage 1.
