using Godot;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Development-only vertical greybox movement laboratory.
/// Geometry intentionally favors long grapple lines, height changes, and retarget practice.
/// </summary>
public partial class MovementLabBuilder : Node3D
{
    public const float RoomHalfWidth = 400f;
    public const float RoomHalfDepth = 500f;
    public const float RoomHeight = 320f;

    private readonly StandardMaterial3D _floorMaterial = new()
    {
        AlbedoColor = new Color(0.12f, 0.14f, 0.18f),
        Roughness = 0.94f,
    };

    private readonly StandardMaterial3D _wallMaterial = new()
    {
        AlbedoColor = new Color(0.20f, 0.23f, 0.30f),
        Roughness = 0.90f,
    };

    private readonly StandardMaterial3D _ceilingMaterial = new()
    {
        AlbedoColor = new Color(0.16f, 0.18f, 0.24f),
        Roughness = 0.92f,
    };

    private readonly StandardMaterial3D _obstacleMaterial = new()
    {
        AlbedoColor = new Color(0.34f, 0.39f, 0.48f),
        Roughness = 0.84f,
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
                AmbientLightColor = new Color(0.46f, 0.52f, 0.64f),
                AmbientLightEnergy = 0.42f,
            },
        });

        AddChild(new DirectionalLight3D
        {
            Name = "KeyLight",
            RotationDegrees = new Vector3(-55f, -30f, 0f),
            LightColor = new Color(0.92f, 0.95f, 1.0f),
            LightEnergy = 1.10f,
            ShadowEnabled = true,
        });

        // Low-energy shadowless fill is only for readability inside the enclosed shell.
        // It must not flatten the lab into a uniformly white space.
        AddChild(new DirectionalLight3D
        {
            Name = "InteriorFill",
            RotationDegrees = new Vector3(-25f, 150f, 0f),
            LightColor = new Color(0.62f, 0.70f, 0.88f),
            LightEnergy = 0.20f,
            ShadowEnabled = false,
        });
    }

    private void BuildGeometry()
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

        // A small readable cluster around spawn, after which the space opens dramatically.
        AddBox("LowLedge", new Vector3(-24f, 3f, 70f), new Vector3(20f, 6f, 16f));
        AddBox("StarterTower", new Vector3(22f, 18f, 66f), new Vector3(8f, 36f, 8f));
        AddBox("StarterBeam", new Vector3(-4f, 28f, 54f), new Vector3(52f, 2f, 5f));

        // Major landmarks are hundreds of meters apart to support real sustained flight.
        AddBox("WestSpire", new Vector3(-250f, 105f, 30f), new Vector3(16f, 210f, 16f));
        AddBox("EastSpire", new Vector3(270f, 135f, -40f), new Vector3(16f, 270f, 16f));
        AddBox("NorthSpire", new Vector3(35f, 145f, -355f), new Vector3(18f, 290f, 18f));
        AddBox("SouthSpire", new Vector3(-50f, 120f, 390f), new Vector3(18f, 240f, 18f));
        AddBox("FarWestTower", new Vector3(-330f, 135f, -280f), new Vector3(20f, 270f, 20f));
        AddBox("FarEastTower", new Vector3(325f, 115f, 300f), new Vector3(20f, 230f, 20f));

        // Mid-distance route pieces prevent the huge room from becoming empty while still
        // leaving broad open volumes between them.
        AddBox("BridgeLow", new Vector3(-80f, 50f, 50f), new Vector3(130f, 3f, 7f));
        AddBox("BridgeMid", new Vector3(120f, 92f, -135f), new Vector3(155f, 3f, 7f));
        AddBox("BridgeHigh", new Vector3(-135f, 145f, -220f), new Vector3(145f, 3f, 7f));
        AddBox("SkyBarA", new Vector3(105f, 205f, -30f), new Vector3(175f, 3f, 6f));
        AddBox("SkyBarB", new Vector3(-150f, 255f, 170f), new Vector3(160f, 3f, 6f));

        // Progressive climb route.
        AddBox("StepAir01", new Vector3(-55f, 24f, -55f), new Vector3(14f, 2f, 14f));
        AddBox("StepAir02", new Vector3(-38f, 48f, -92f), new Vector3(14f, 2f, 14f));
        AddBox("StepAir03", new Vector3(-12f, 78f, -132f), new Vector3(14f, 2f, 14f));
        AddBox("StepAir04", new Vector3(28f, 115f, -165f), new Vector3(14f, 2f, 14f));
        AddBox("StepAir05", new Vector3(75f, 158f, -150f), new Vector3(14f, 2f, 14f));
        AddBox("StepAir06", new Vector3(125f, 205f, -105f), new Vector3(14f, 2f, 14f));

        // Narrow high-value targets for long-distance retargeting.
        AddBox("NeedleA", new Vector3(-315f, 120f, -70f), new Vector3(4f, 150f, 4f));
        AddBox("NeedleB", new Vector3(320f, 160f, 105f), new Vector3(4f, 180f, 4f));
        AddBox("NeedleC", new Vector3(95f, 215f, 350f), new Vector3(4f, 150f, 4f));
        AddBox("NeedleD", new Vector3(-185f, 245f, -380f), new Vector3(4f, 125f, 4f));

        // Long ground reference lane.
        AddBox("SpeedLaneWestRail", new Vector3(-14f, 1f, 330f), new Vector3(1f, 2f, 260f));
        AddBox("SpeedLaneEastRail", new Vector3(14f, 1f, 330f), new Vector3(1f, 2f, 260f));

        AddBox(
            "NoGrappleBlock",
            new Vector3(345f, 8f, 430f),
            new Vector3(16f, 16f, 16f),
            _forbiddenMaterial,
            collisionLayer: 2u);
    }

    private void AddBox(
        string name,
        Vector3 position,
        Vector3 size,
        Material? material = null,
        uint collisionLayer = 1u)
    {
        AddChild(new CsgBox3D
        {
            Name = name,
            Position = position,
            Size = size,
            Material = material ?? _obstacleMaterial,
            UseCollision = true,
            CollisionLayer = collisionLayer,
        });
    }
}
