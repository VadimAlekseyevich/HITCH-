using System.Numerics;

namespace Hitch.Simulation.World;

/// <summary>
/// Engine-independent result of a world query.
///
/// TravelFraction is expected to be in [0, 1] where 0 is the query start
/// and 1 is its full requested travel.
/// </summary>
public readonly record struct WorldHit(
    Vector3 Position,
    Vector3 Normal,
    float TravelFraction,
    uint CollisionLayer);
