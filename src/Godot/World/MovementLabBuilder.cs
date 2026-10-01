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
    public const float RoomHalfWidth = 100f;
    public const float RoomHalfDepth = 125f;
    public const float RoomHeight = 80f;

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
        // Flat, non-colliding-looking visual strips built as ultra-thin colliders.
        // They make the city scale immediately readable from ground level and rooftops.
        AddBox(
            "MainAvenue",
            new Vector3(0f, 0.015f, 0f),
            new Vector3(13f, 0.03f, 230f),
            _roadMarkerMaterial,
            useCollision: false);
        AddBox(
            "CrossStreetNorth",
            new Vector3(0f, 0.02f, -48f),
            new Vector3(188f, 0.04f, 11f),
            _roadMarkerMaterial,
            useCollision: false);
        AddBox(
            "CrossStreetCenter",
            new Vector3(0f, 0.02f, 12f),
            new Vector3(188f, 0.04f, 11f),
            _roadMarkerMaterial,
            useCollision: false);
        AddBox(
            "CrossStreetSouth",
            new Vector3(0f, 0.02f, 72f),
            new Vector3(188f, 0.04f, 11f),
            _roadMarkerMaterial,
            useCollision: false);
    }

    private void BuildCity()
    {
        // West side: dense mid-rise blocks with narrow alleys.
        AddBuilding("W01", -76f, -98f, 24f, 20f, 22f, _buildingMaterialA);
        AddBuilding("W02", -48f, -98f, 18f, 22f, 36f, _buildingMaterialB);
        AddBuilding("W03", -76f, -66f, 22f, 20f, 48f, _buildingMaterialC);
        AddBuilding("W04", -48f, -67f, 19f, 19f, 26f, _buildingMaterialA);

        AddBuilding("W05", -76f, -20f, 23f, 28f, 58f, _buildingMaterialB);
        AddBuilding("W06", -48f, -23f, 18f, 24f, 32f, _buildingMaterialC);
        AddBuilding("W07", -75f, 43f, 25f, 26f, 42f, _buildingMaterialA);
        AddBuilding("W08", -46f, 44f, 20f, 25f, 64f, _buildingMaterialB);
        AddBuilding("W09", -76f, 101f, 24f, 28f, 30f, _buildingMaterialC);
        AddBuilding("W10", -47f, 103f, 18f, 24f, 50f, _buildingMaterialA);

        // East side: more varied footprints and heights for rooftop retargeting.
        AddBuilding("E01", 47f, -101f, 18f, 24f, 46f, _buildingMaterialC);
        AddBuilding("E02", 76f, -98f, 24f, 27f, 28f, _buildingMaterialA);
        AddBuilding("E03", 48f, -69f, 19f, 19f, 60f, _buildingMaterialB);
        AddBuilding("E04", 77f, -66f, 23f, 21f, 38f, _buildingMaterialC);

        AddBuilding("E05", 49f, -20f, 20f, 28f, 34f, _buildingMaterialA);
        AddBuilding("E06", 77f, -18f, 24f, 31f, 68f, _buildingMaterialB);
        AddBuilding("E07", 47f, 42f, 18f, 24f, 54f, _buildingMaterialC);
        AddBuilding("E08", 76f, 43f, 24f, 25f, 40f, _buildingMaterialA);
        AddBuilding("E09", 48f, 101f, 20f, 27f, 24f, _buildingMaterialB);
        AddBuilding("E10", 77f, 101f, 24f, 28f, 56f, _buildingMaterialC);

        // Inner blocks close to the main avenue create fast street-canyon decisions.
        AddBuilding("InnerNW", -24f, -82f, 19f, 30f, 44f, _buildingMaterialA);
        AddBuilding("InnerNE", 24f, -82f, 19f, 30f, 30f, _buildingMaterialB);
        AddBuilding("InnerW", -24f, -20f, 18f, 27f, 34f, _buildingMaterialC);
        AddBuilding("InnerE", 24f, -18f, 18f, 31f, 52f, _buildingMaterialA);
        AddBuilding("InnerSW", -24f, 45f, 18f, 26f, 28f, _buildingMaterialB);
        AddBuilding("InnerSE", 24f, 45f, 18f, 26f, 46f, _buildingMaterialC);
        AddBuilding("InnerSouthW", -24f, 100f, 18f, 25f, 38f, _buildingMaterialA);
        AddBuilding("InnerSouthE", 24f, 101f, 18f, 26f, 62f, _buildingMaterialB);

        // A few rooftop masses break perfectly rectangular silhouettes and provide short anchors.
        AddRooftopBox("RoofUnitW05", -76f, -20f, 11f, 9f, 58f);
        AddRooftopBox("RoofUnitE06", 77f, -18f, 12f, 10f, 68f);
        AddRooftopBox("RoofUnitInnerE", 24f, -18f, 9f, 8f, 52f);
        AddRooftopBox("RoofUnitInnerSouthE", 24f, 101f, 8f, 9f, 62f);
    }

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
            "NoGrappleBillboard",
            new Vector3(89f, 9f, 111f),
            new Vector3(7f, 18f, 2f),
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
