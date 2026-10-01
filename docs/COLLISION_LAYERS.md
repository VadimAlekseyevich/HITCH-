# HITCH! — MVP Collision Layers

The movement prototype currently reserves two world collision layers.

| Layer | Bit | Meaning |
|---|---:|---|
| 1 | `1u` | Normal world geometry: collidable and grapplable |
| 2 | `2u` | Forbidden grapple geometry: collidable but not grapplable |

## Current masks

- `PlayerLocomotionConfig.WorldCollisionMask = 0b11u` — the player collides with both layers.
- `WinchConfig.GrappleCollisionMask = 1u` — the Stage 5 grapple only sees normal grapplable geometry.

This is an MVP convention, not a final layer taxonomy.

Do not special-case scene node names such as `NoGrappleBlock` inside gameplay logic. Surface eligibility is data-driven through collision layers/masks.
