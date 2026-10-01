using Godot;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Development-only vertical greybox movement laboratory.
/// Geometry intentionally favors long grapple lines, height changes, and retarget practice.
/// </summary>
public partial class MovementLabBuilder : Node3D
{
    public const float RoomHalfWidth = 120f;
    public const float RoomHalfDepth = 160f;
    public const float RoomHeight = 150f;

    private readonly StandardMaterial3D _floorMaterial = new()
    {
        AlbedoColor = new Color(0.42f, 0.45f, 0.52f),
        Roughness = 0.90f,
    };

    private readonly StandardMaterial3D _obstacleMaterial = new()
    {
        AlbedoColor = new Color(0.62f, 0.66f, 0.74f),
        Roughness = 0.78f,
    };

    private readonly StandardMaterial3D _forbiddenMaterial = new()
    {
        AlbedoColor = new Color(0.82f, 0.16f, 0.14f),
        Roughness = 0.78f,
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
                BackgroundColor = new Color(0.16f, 0.19f, 0.25f),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color(0.78f, 0.82f, 0.90f),
                AmbientLightEnergy = 0.88f,
            },
        });

        // Key light keeps strong shape definition where the room shell permits it.
        AddChild(new DirectionalLight3D
        {
            Name = "KeyLight",
            RotationDegrees = new Vector3(-58f, -32f, 0f),
            LightEnergy = 1.55f,
            ShadowEnabled = true,
        });

        // The movement lab is deliberately enclosed, so a shadowless fill prevents the
        // ceiling and tall structures from turning large regions into unreadable darkness.
        AddChild(new DirectionalLight3D
        {
            Name = "InteriorFill",
            RotationDegrees = new Vector3(-35f, 145f, 0f),
            LightEnergy = 0.85f,
            ShadowEnabled = false,
        });
    }

    private void BuildGeometry()
    {
        var roomWidth = RoomHalfWidth * 2f;
        var roomDepth = RoomHalfDepth * 2f;
        var wallCenterY = RoomHeight * 0.5f;

        // Stage 5 iteration 9 deliberately uses a much larger volume so sustained high-speed
        // traversal has room to breathe before we introduce damage or combat pressure.
        AddBox(
            "Floor",
            new Vector3(0f, -0.5f, 0f),
            new Vector3(roomWidth, 1f, roomDepth),
            _floorMaterial);

        AddBox(
            "WestWall",
            new Vector3(-RoomHalfWidth, wallCenterY, 0f),
            new Vector3(1f, RoomHeight, roomDepth));
        AddBox(
            "EastWall",
            new Vector3(RoomHalfWidth, wallCenterY, 0f),
            new Vector3(1f, RoomHeight, roomDepth));
        AddBox(
            "BackWall",
            new Vector3(0f, wallCenterY, -RoomHalfDepth),
            new Vector3(roomWidth, RoomHeight, 1f));
        AddBox(
            "FrontWall",
            new Vector3(0f, wallCenterY, RoomHalfDepth),
            new Vector3(roomWidth, RoomHeight, 1f));
        AddBox(
            "Ceiling",
            new Vector3(0f, RoomHeight + 0.5f, 0f),
            new Vector3(roomWidth, 1f, roomDepth),
            _floorMaterial);

        // Nearby onboarding geometry remains close enough to understand the grapple immediately.
        AddBox("LowLedge", new Vector3(-20f, 3f, 58f), new Vector3(18f, 6f, 14f));
        AddBox("StarterTower", new Vector3(16f, 15f, 52f), new Vector3(7f, 30f, 7f));
        AddBox("StarterBeam", new Vector3(-2f, 23f, 44f), new Vector3(40f, 2f, 4f));

        // Widely separated major landmarks. These are intentionally farther apart than iteration 8
        // so a good line can turn into a long, fast traversal rather than another short hop.
        AddBox("WestSpire", new Vector3(-74f, 48f, 8f), new Vector3(10f, 96f, 10f));
        AddBox("EastSpire", new Vector3(78f, 62f, -18f), new Vector3(10f, 124f, 10f));
        AddBox("NorthSpire", new Vector3(12f, 68f, -104f), new Vector3(12f, 136f, 12f));
        AddBox("SouthSpire", new Vector3(-18f, 52f, 118f), new Vector3(12f, 104f, 12f));
        AddBox("FarWestTower", new Vector3(-100f, 58f, -82f), new Vector3(12f, 116f, 12f));
        AddBox("FarEastTower", new Vector3(100f, 50f, 92f), new Vector3(12f, 100f, 12f));

        // Air routes give the player multiple height bands to chain without prescribing one path.
        AddBox("BridgeLow", new Vector3(-24f, 30f, 18f), new Vector3(62f, 2f, 5f));
        AddBox("BridgeMid", new Vector3(38f, 55f, -48f), new Vector3(72f, 2f, 5f));
        AddBox("BridgeHigh", new Vector3(-36f, 84f, -74f), new Vector3(68f, 2f, 5f));
        AddBox("SkyBarA", new Vector3(28f, 108f, -10f), new Vector3(82f, 2f, 4f));
        AddBox("SkyBarB", new Vector3(-44f, 128f, 56f), new Vector3(70f, 2f, 4f));

        // Ascending targets support controlled practice before committing to the longest lines.
        AddBox("StepAir01", new Vector3(-34f, 14f, -24f), new Vector3(11f, 2f, 11f));
        AddBox("StepAir02", new Vector3(-22f, 27f, -42f), new Vector3(11f, 2f, 11f));
        AddBox("StepAir03", new Vector3(-6f, 43f, -60f), new Vector3(11f, 2f, 11f));
        AddBox("StepAir04", new Vector3(16f, 62f, -70f), new Vector3(11f, 2f, 11f));
        AddBox("StepAir05", new Vector3(42f, 84f, -58f), new Vector3(11f, 2f, 11f));
        AddBox("StepAir06", new Vector3(66f, 108f, -38f), new Vector3(11f, 2f, 11f));

        // Thin targets reward accurate long-range retargeting.
        AddBox("NeedleA", new Vector3(-96f, 48f, -20f), new Vector3(3f, 60f, 3f));
        AddBox("NeedleB", new Vector3(96f, 70f, 34f), new Vector3(3f, 76f, 3f));
        AddBox("NeedleC", new Vector3(28f, 92f, 112f), new Vector3(3f, 64f, 3f));
        AddBox("NeedleD", new Vector3(-54f, 110f, -118f), new Vector3(3f, 54f, 3f));

        // Long ground lane remains useful for checking carried momentum after a retarget/miss.
        AddBox("SpeedLaneWestRail", new Vector3(-12f, 1f, 112f), new Vector3(1f, 2f, 82f));
        AddBox("SpeedLaneEastRail", new Vector3(12f, 1f, 112f), new Vector3(1f, 2f, 82f));

        // Collision layer 2: solid but intentionally not grapplable.
        AddBox(
            "NoGrappleBlock",
            new Vector3(102f, 7f, 128f),
            new Vector3(14f, 14f, 14f),
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
