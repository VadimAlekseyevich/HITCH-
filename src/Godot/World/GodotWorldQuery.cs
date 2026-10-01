using Godot;
using Hitch.Simulation.World;
using NumericsVector3 = System.Numerics.Vector3;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Godot/Jolt implementation of the simulation world-query boundary.
///
/// Must be called during Godot's physics step because direct physics-space access is only safe there.
/// </summary>
internal sealed class GodotWorldQuery : IWorldQuery
{
    private readonly Node3D _worldNode;
    private readonly CapsuleShape3D _capsuleShape = new();
    private readonly PhysicsShapeQueryParameters3D _shapeQuery = new()
    {
        CollideWithAreas = false,
        CollideWithBodies = true,
    };
    private readonly PhysicsRayQueryParameters3D _rayQuery = new()
    {
        CollideWithAreas = false,
        CollideWithBodies = true,
        HitFromInside = false,
    };

    public GodotWorldQuery(Node3D worldNode)
    {
        _worldNode = worldNode ?? throw new ArgumentNullException(nameof(worldNode));
        _shapeQuery.Shape = _capsuleShape;
    }

    public bool TryRaycast(in RayQuery query, out WorldHit hit)
    {
        var from = ToGodot(query.From);
        var to = ToGodot(query.To);

        _rayQuery.From = from;
        _rayQuery.To = to;
        _rayQuery.CollisionMask = query.CollisionMask;

        var result = _worldNode.GetWorld3D().DirectSpaceState.IntersectRay(_rayQuery);

        if (result.Count == 0)
        {
            hit = default;
            return false;
        }

        var point = result["position"].AsVector3();
        var normal = result["normal"].AsVector3();
        var fullDistance = from.DistanceTo(to);
        var travelFraction = fullDistance > 1e-6f
            ? Math.Clamp(from.DistanceTo(point) / fullDistance, 0f, 1f)
            : 0f;

        var collisionLayer = ReadCollisionLayer(result);

        hit = new WorldHit(
            ToNumerics(point),
            ToNumerics(normal),
            travelFraction,
            collisionLayer);

        return true;
    }

    public bool TrySweepCapsule(in CapsuleSweepQuery query, out WorldHit hit)
    {
        query.Validate();

        _capsuleShape.Radius = query.Radius;
        _capsuleShape.Height = (query.HalfSegmentLength + query.Radius) * 2f;

        var start = ToGodot(query.StartCenter);
        var motion = ToGodot(query.Displacement);

        _shapeQuery.Transform = new Transform3D(Basis.Identity, start);
        _shapeQuery.Motion = motion;
        _shapeQuery.Margin = query.Margin;
        _shapeQuery.CollisionMask = query.CollisionMask;

        var fractions = _worldNode.GetWorld3D().DirectSpaceState.CastMotion(_shapeQuery);

        if (fractions.Length < 2 || fractions[0] >= 1f)
        {
            hit = default;
            return false;
        }

        var safeFraction = Math.Clamp(fractions[0], 0f, 1f);
        var unsafeFraction = Math.Clamp(fractions[1], safeFraction, 1f);
        var unsafeCenter = start + (motion * unsafeFraction);

        // CastMotion reports travel fractions but not the contact normal in Godot 4.7.
        // Query the slightly intersecting unsafe position to obtain the nearest rest normal.
        _shapeQuery.Transform = new Transform3D(Basis.Identity, unsafeCenter);
        _shapeQuery.Motion = Vector3.Zero;

        var rest = _worldNode.GetWorld3D().DirectSpaceState.GetRestInfo(_shapeQuery);

        var normal = Vector3.Zero;
        var point = start + (motion * safeFraction);

        if (rest.Count > 0)
        {
            if (rest.ContainsKey("normal"))
            {
                normal = rest["normal"].AsVector3();
            }

            if (rest.ContainsKey("point"))
            {
                point = rest["point"].AsVector3();
            }
        }

        if (normal.IsZeroApprox())
        {
            // This should be rare; keeping a deterministic fallback is safer than returning NaNs.
            normal = motion.IsZeroApprox()
                ? Vector3.Up
                : -motion.Normalized();
        }

        hit = new WorldHit(
            ToNumerics(point),
            ToNumerics(normal.Normalized()),
            safeFraction,
            0u);

        return true;
    }

    private static uint ReadCollisionLayer(Godot.Collections.Dictionary result)
    {
        if (!result.TryGetValue("collider", out var colliderValue))
        {
            return 0u;
        }

        var collider = colliderValue.AsGodotObject();

        return collider switch
        {
            CollisionObject3D collisionObject => collisionObject.CollisionLayer,
            CsgShape3D csg => csg.CollisionLayer,
            _ => 0u,
        };
    }

    private static Vector3 ToGodot(NumericsVector3 value) =>
        new(value.X, value.Y, value.Z);

    private static NumericsVector3 ToNumerics(Vector3 value) =>
        new(value.X, value.Y, value.Z);
}
