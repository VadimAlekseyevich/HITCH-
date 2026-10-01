# Stage 5 — Iteration 2: Selected-Point Direct Pull

**Status: IMPLEMENTED — awaiting human playtest**

## Why this iteration exists

The first Stage 5 human gate rejected the spring + accelerating reel prototype.

Reported problems:

- inertia felt broken/wild;
- pulling could feel ineffective;
- releasing near a wall could feel sticky;
- controls were cumbersome;
- grounded movement felt slippery.

## Active hypothesis

Use the smallest possible control loop before reintroducing rope complexity:

1. LMB hitscan-selects a valid world point.
2. Selection alone applies no force.
3. Hold RMB to immediately set velocity toward the point at `PullSpeed`.
4. Release RMB early: stop force immediately and retain momentum.
5. LMB while pulling: replace target and redirect immediately.
6. Reach `ArrivalDistance`: clear target/pull, clear velocity, return to ordinary gravity.
7. No Q/E reel controls in this iteration.

## Ground movement correction

Grounded movement now converges toward desired horizontal velocity and applies strong braking toward zero when input stops.

The old rule that preserved above-walk-speed ground momentum was removed because it produced the reported ice-like feel.

## Human gate

Do not proceed to Stage 6 until this iteration is manually played.

Focus on:

- target placement clarity;
- immediate RMB pull response;
- clean release;
- predictable retargeting;
- clean arrival/drop;
- non-slippery WASD;
- understandable momentum after early release.
