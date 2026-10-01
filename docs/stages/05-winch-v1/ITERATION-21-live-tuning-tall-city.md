# Stage 5 — Iteration 21: Live Tuning + Taller City

**Status: IMPLEMENTED — awaiting human playtest**

## Why this iteration exists

Dual-hook traversal has reached the point where player skill and timing materially affect perceived movement quality.

That creates a tuning problem:

- one bad attempt can make good parameters feel bad;
- one learned movement loop can make the same parameters feel dramatically better;
- rebuild/commit cycles are too slow for coefficient-level feel work.

Iteration 21 moves feel tuning into the running game.

## Default environment changes

### City

- 1000 × 1200 m footprint;
- 320 m enclosed height;
- roughly 120 buildings;
- typical building range ~50–170 m;
- >200 m skyline landmarks.

### Gravity

- 14.5 m/s².

The stronger gravity and taller grapple field target the reported pattern where a good low-point release placed the player far above a low skyline and left the next useful hook cycle pointed mostly downward.

## F2 tuning window

F2 opens/closes the tuning UI.

Opening it:

- freezes simulation stepping;
- releases mouse capture;
- prevents UI clicks from firing hooks.

Closing it:

- restores mouse capture;
- resumes the same simulation state.

## Editing

Every exposed parameter has:

- Russian display name;
- slider;
- exact text entry;
- units.

Slider changes apply immediately.

Text entry accepts dot or comma decimal separators and applies on Enter/focus loss.

Invalid combinations are rejected by the same SimulationConfig validation used at startup.

## Runtime state preservation

Applying tuning does not reconstruct GameSimulation.

Current:

- tick;
- player position;
- player velocity;
- left hook;
- right hook;
- bend contacts;
- deployed rope lengths;

remain intact.

Only the active validated configuration object is replaced.

## Clipboard export

**Скопировать параметры** writes a `HITCH_TUNING_V1` text block to the OS clipboard.

The block includes the current city dimensions and all exposed movement/rope values.

It is deliberately designed to be pasted into ChatGPT without manual transcription.

## Reset

**Сбросить к стартовым** restores the config that was loaded when the scene started.

## Verification

CI: https://github.com/VadimAlekseyevich/HITCH-/actions/runs/36934949332

- 102 tests passed;
- 0 failed;
- 0 warnings;
- 0 errors;
- runtime config state-preservation test passed;
- headless stable-spawn smoke passed;
- enlarged room closure smoke passed.

## Human gate focus

1. Does 14.5 m/s² gravity make high releases easier to recover from without killing satisfying arcs?
2. Are 50–170 m buildings tall enough that a strong release usually leaves useful anchors near/beyond the player's altitude?
3. Is the 1000 × 1200 m city large enough for sustained practice?
4. Can tuning one parameter at a time in F2 make cause/effect understandable?
5. Is clipboard export convenient enough to replace rebuild-based micro-tuning?
