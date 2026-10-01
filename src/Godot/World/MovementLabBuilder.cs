using Godot;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Development-only enclosed city movement lab.
///
/// Stage 5 iteration 15 intentionally uses human-readable urban scale instead of a giant
/// abstract traversal volume. Buildings remain simple greybox geometry so movement feel,
/// sightlines, rooftop routes, and street gaps are easy to judge without art noise.
/// </summary>
public partial class MovementLabBuilder : Node3D
{
    public const float RoomHalfWidth = 320f;
    public const float RoomHalfDepth = 400f;
    public const float RoomHeight = 180f;

    private readonly StandardMaterial3D _floorMaterial = new()
    {
        AlbedoColor = new Color(0.10f, 0.11f, 0.14f),
        Roughness = 0.96f,
    };

    private readonly StandardMaterial3D _wallMaterial = new()
    {
        AlbedoColor = new Color(0.16f, 0.18f, 0.23f),
        Roughness = 0.92f,
    };

    private readonly StandardMaterial3D _ceilingMaterial = new()
    {
        AlbedoColor = new Color(0.12f, 0.14f, 0.19f),
        Roughness = 0.94f,
    };

    private readonly StandardMaterial3D _buildingMaterialA = new()
    {
        AlbedoColor = new Color(0.30f, 0.34f, 0.40f),
        Roughness = 0.88f,
    };

    private readonly StandardMaterial3D _buildingMaterialB = new()
    {
        AlbedoColor = new Color(0.25f, 0.29f, 0.36f),
        Roughness = 0.90f,
    };

    private readonly StandardMaterial3D _buildingMaterialC = new()
    {
        AlbedoColor = new Color(0.36f, 0.33f, 0.31f),
        Roughness = 0.90f,
    };

    private readonly StandardMaterial3D _roofMaterial = new()
    {
        AlbedoColor = new Color(0.43f, 0.47f, 0.54f),
        Roughness = 0.82f,
    };

    private readonly StandardMaterial3D _roadMarkerMaterial = new()
    {
        AlbedoColor = new Color(0.34f, 0.36f, 0.41f),
        Roughness = 0.95f,
    };

    private readonly StandardMaterial3D _forbiddenMaterial = new()
    {
        AlbedoColor = new Color(0.62f, 0.08f, 0.08f),
        Roughness = 0.82f,
    };

    public override void _Ready()
    {
        AddEnvironment();
        BuildGeometry();
    }

    private void AddEnvironment()
    {
        AddChild(new WorldEnvironment
        {
            Name = "Environment",
            Environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = new Color(0.025f, 0.032f, 0.05f),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color(0.48f, 0.53f, 0.62f),
                AmbientLightEnergy = 0.46f,
            },
        });

        AddChild(new DirectionalLight3D
        {
            Name = "KeyLight",
            RotationDegrees = new Vector3(-58f, -35f, 0f),
            LightColor = new Color(0.94f, 0.96f, 1.0f),
            LightEnergy = 1.05f,
            ShadowEnabled = true,
        });

        AddChild(new DirectionalLight3D
        {
            Name = "InteriorFill",
            RotationDegrees = new Vector3(-28f, 145f, 0f),
            LightColor = new Color(0.64f, 0.70f, 0.84f),
            LightEnergy = 0.18f,
            ShadowEnabled = false,
        });
    }

    private void BuildGeometry()
    {
        BuildEnclosedShell();
        BuildStreetMarkers();
        BuildCity();
        BuildSpecialTraversalTargets();
    }

    private void BuildEnclosedShell()
    {
        var roomWidth = RoomHalfWidth * 2f;
        var roomDepth = RoomHalfDepth * 2f;
        var wallCenterY = RoomHeight * 0.5f;

        AddBox(
            "Floor",
            new Vector3(0f, -0.5f, 0f),
            new Vector3(roomWidth, 1f, roomDepth),
            _floorMaterial);

        AddBox(
            "WestWall",
            new Vector3(-RoomHalfWidth, wallCenterY, 0f),
            new Vector3(1f, RoomHeight, roomDepth),
            _wallMaterial);
        AddBox(
            "EastWall",
            new Vector3(RoomHalfWidth, wallCenterY, 0f),
            new Vector3(1f, RoomHeight, roomDepth),
            _wallMaterial);
        AddBox(
            "BackWall",
            new Vector3(0f, wallCenterY, -RoomHalfDepth),
            new Vector3(roomWidth, RoomHeight, 1f),
            _wallMaterial);
        AddBox(
            "FrontWall",
            new Vector3(0f, wallCenterY, RoomHalfDepth),
            new Vector3(roomWidth, RoomHeight, 1f),
            _wallMaterial);
        AddBox(
            "Ceiling",
            new Vector3(0f, RoomHeight + 0.5f, 0f),
            new Vector3(roomWidth, 1f, roomDepth),
            _ceilingMaterial);
    }

    private void BuildStreetMarkers()
    {
        // Wide visual streets keep scale readable across the ~10x larger city footprint.
        AddRoad("MainAvenue", 0f, 0f, 24f, 760f);
        AddRoad("WestAvenue", -168f, 0f, 16f, 760f);
        AddRoad("EastAvenue", 168f, 0f, 16f, 760f);

        AddRoad("CrossStreetNorth", 0f, -290f, 600f, 16f);
        AddRoad("CrossStreetCenter", 0f, 0f, 600f, 16f);
        AddRoad("CrossStreetSouth", 0f, 290f, 600f, 16f);
    }

    private void AddRoad(
        string name,
        float x,
        float z,
        float width,
        float depth)
    {
        AddBox(
            name,
            new Vector3(x, 0.015f, z),
            new Vector3(width, 0.03f, depth),
            _roadMarkerMaterial,
            useCollision: false);
    }

    private void BuildCity()
    {
        // Deterministic procedural greybox city. The footprint is ~10.2x the iteration-18
        // compact city area, but local street/building scale stays similar so motion still reads.
        var columnIndex = 0;

        for (var x = -280f; x <= 280f; x += 56f)
        {
            if (IsNorthSouthAvenue(x))
            {
                columnIndex++;
                continue;
            }

            var rowIndex = 0;

            for (var z = -350f; z <= 350f; z += 58f)
            {
                if (IsCrossStreet(z))
                {
                    rowIndex++;
                    continue;
                }

                var hash = Math.Abs(
                    ((columnIndex + 17) * 73856093)
                    ^ ((rowIndex + 31) * 19349663));

                var width =
                    27f + (hash % 14);
                var depth =
                    29f + ((hash / 17) % 17);
                var height =
                    24f + ((hash / 113) % 82);

                var material =
                    (hash % 3) switch
                    {
                        0 => _buildingMaterialA,
                        1 => _buildingMaterialB,
                        _ => _buildingMaterialC,
                    };

                var name =
                    $"City_{columnIndex:D2}_{rowIndex:D2}";

                AddBuilding(
                    name,
                    x,
                    z,
                    width,
                    depth,
                    height,
                    material);

                if (hash % 5 == 0)
                {
                    AddRooftopBox(
                        $"{name}_Utility",
                        x,
                        z,
                        MathF.Min(12f, width * 0.38f),
                        MathF.Min(10f, depth * 0.34f),
                        height);
                }

                rowIndex++;
            }

            columnIndex++;
        }
    }

    private static bool IsNorthSouthAvenue(float x) =>
        MathF.Abs(x) < 20f
        || MathF.Abs(x + 168f) < 12f
        || MathF.Abs(x - 168f) < 12f;

    private static bool IsCrossStreet(float z) =>
        MathF.Abs(z) < 12f
        || MathF.Abs(z + 290f) < 12f
        || MathF.Abs(z - 290f) < 12f;

    private void BuildSpecialTraversalTargets()
    {
        // Central plaza objects are intentionally much smaller than the buildings so the player
        // always has an immediate sense of human scale after spawning.
        AddBuilding(
            "PlazaTower",
            0f,
            40f,
            9f,
            9f,
            32f,
            _roofMaterial);

        // Thin wrap-test columns in open street space. These deliberately expose the
        // piecewise rope behavior: the cable should catch successive sides as the player circles.
        AddBox(
            "WrapPostA",
            new Vector3(-10f, 9f, 24f),
            new Vector3(2.4f, 18f, 2.4f),
            _buildingMaterialC);
        AddBox(
            "WrapPostB",
            new Vector3(11f, 13f, -37f),
            new Vector3(3f, 26f, 3f),
            _buildingMaterialB);
        AddBox(
            "WrapPostC",
            new Vector3(-9f, 7f, -100f),
            new Vector3(2f, 14f, 2f),
            _roofMaterial);

        AddBox(
            "SkyBridgeWest",
            new Vector3(-35f, 27f, 12f),
            new Vector3(28f, 2f, 4f),
            _roofMaterial);
        AddBox(
            "SkyBridgeEast",
            new Vector3(35f, 36f, -48f),
            new Vector3(30f, 2f, 4f),
            _roofMaterial);

        AddBox(
            "NorthAntenna",
            new Vector3(-24f, 60f, -82f),
            new Vector3(2f, 28f, 2f),
            _roofMaterial);
        AddBox(
            "SouthAntenna",
            new Vector3(24f, 69f, 101f),
            new Vector3(2f, 14f, 2f),
            _roofMaterial);

        AddBox(
            "FarNorthSpire",
            new Vector3(-250f, 70f, -345f),
            new Vector3(8f, 140f, 8f),
            _roofMaterial);
        AddBox(
            "FarSouthSpire",
            new Vector3(250f, 62f, 345f),
            new Vector3(8f, 124f, 8f),
            _roofMaterial);

        AddBox(
            "NoGrappleBillboard",
            new Vector3(300f, 12f, 370f),
            new Vector3(10f, 24f, 3f),
            _forbiddenMaterial,
            collisionLayer: 2u);
    }

    private void AddBuilding(
        string name,
        float x,
        float z,
        float width,
        float depth,
        float height,
        Material material)
    {
        AddBox(
            name,
            new Vector3(x, height * 0.5f, z),
            new Vector3(width, height, depth),
            material);

        AddBox(
            $"{name}_Roof",
            new Vector3(x, height + 0.15f, z),
            new Vector3(width + 0.25f, 0.30f, depth + 0.25f),
            _roofMaterial);
    }

    private void AddRooftopBox(
        string name,
        float x,
        float z,
        float width,
        float depth,
        float roofHeight)
    {
        const float unitHeight = 3f;

        AddBox(
            name,
            new Vector3(x, roofHeight + (unitHeight * 0.5f), z),
            new Vector3(width, unitHeight, depth),
            _roofMaterial);
    }

    private void AddBox(
        string name,
        Vector3 position,
        Vector3 size,
        Material? material = null,
        uint collisionLayer = 1u,
        bool useCollision = true)
    {
        AddChild(new CsgBox3D
        {
            Name = name,
            Position = position,
            Size = size,
            Material = material ?? _buildingMaterialA,
            UseCollision = useCollision,
            CollisionLayer = collisionLayer,
        });
    }
}
