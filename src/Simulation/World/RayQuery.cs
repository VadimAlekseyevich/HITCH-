using System.Numerics;

namespace Hitch.Simulation.World;

public readonly record struct RayQuery(
    Vector3 From,
    Vector3 To,
    uint CollisionMask);
