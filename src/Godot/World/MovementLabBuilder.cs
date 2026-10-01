using Godot;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Development-only vertical greybox movement laboratory.
/// Geometry intentionally favors long grapple lines, height changes, and retarget practice.
/// </summary>
public partial class MovementLabBuilder : Node3D
{
    private readonly StandardMaterial3D _floorMaterial = new()
    {
        AlbedoColor = new Color(0.28f, 0.30f, 0.34f),
        Roughness = 0.92f,
    };

    private readonly StandardMaterial3D _obstacleMaterial = new()
    {
        AlbedoColor = new Color(0.48f, 0.50f, 0.55f),
        Roughness = 0.82f,
    };

    private readonly StandardMaterial3D _forbiddenMaterial = new()
    {
        AlbedoColor = new Color(0.72f, 0.12f, 0.12f),
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
                BackgroundColor = new Color(0.035f, 0.045f, 0.06f),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color(0.56f, 0.60f, 0.68f),
                AmbientLightEnergy = 0.55f,
            },
        });

        AddChild(new DirectionalLight3D
        {
            Name = "Sun",
            RotationDegrees = new Vector3(-58f, -32f, 0f),
            LightEnergy = 1.25f,
            ShadowEnabled = true,
        });
    }

    private void BuildGeometry()
    {
        // Large base so long grapple experiments do not immediately leave the test space.
        AddBox("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(120f, 1f, 150f), _floorMaterial);

        // Low perimeter reference walls. The useful vertical gameplay comes from the internal towers.
        AddBox("WestWall", new Vector3(-60f, 6f, 0f), new Vector3(1f, 12f, 150f));
        AddBox("EastWall", new Vector3(60f, 6f, 0f), new Vector3(1f, 12f, 150f));
        AddBox("BackWall", new Vector3(0f, 6f, -75f), new Vector3(120f, 12f, 1f));
        AddBox("FrontWallLeft", new Vector3(-40f, 5f, 75f), new Vector3(40f, 10f, 1f));
        AddBox("FrontWallRight", new Vector3(40f, 5f, 75f), new Vector3(40f, 10f, 1f));

        // Nearby onboarding geometry.
        AddBox("LowLedge", new Vector3(-16f, 2f, 40f), new Vector3(14f, 4f, 10f));
        AddBox("StarterTower", new Vector3(12f, 10f, 34f), new Vector3(5f, 20f, 5f));
        AddBox("StarterBeam", new Vector3(0f, 15f, 30f), new Vector3(28f, 2f, 3f));

        // Main vertical playground.
        AddBox("WestSpire", new Vector3(-34f, 24f, 4f), new Vector3(7f, 48f, 7f));
        AddBox("EastSpire", new Vector3(34f, 30f, -5f), new Vector3(7f, 60f, 7f));
        AddBox("NorthSpire", new Vector3(4f, 36f, -44f), new Vector3(8f, 72f, 8f));
        AddBox("SouthSpire", new Vector3(-6f, 22f, 58f), new Vector3(8f, 44f, 8f));

        // Mid-air targets for chaining LMB -> RMB -> LMB transitions.
        AddBox("BridgeLow", new Vector3(-12f, 20f, 8f), new Vector3(40f, 2f, 4f));
        AddBox("BridgeMid", new Vector3(17f, 34f, -25f), new Vector3(38f, 2f, 4f));
        AddBox("BridgeHigh", new Vector3(-14f, 50f, -36f), new Vector3(32f, 2f, 4f));
        AddBox("SkyBar", new Vector3(0f, 62f, -8f), new Vector3(46f, 2f, 3f));

        // Vertical stepping route: progressively higher small targets rather than one solid wall.
        AddBox("StepAir01", new Vector3(-24f, 10f, -18f), new Vector3(9f, 2f, 9f));
        AddBox("StepAir02", new Vector3(-14f, 18f, -28f), new Vector3(9f, 2f, 9f));
        AddBox("StepAir03", new Vector3(-4f, 27f, -36f), new Vector3(9f, 2f, 9f));
        AddBox("StepAir04", new Vector3(8f, 38f, -42f), new Vector3(9f, 2f, 9f));
        AddBox("StepAir05", new Vector3(20f, 50f, -35f), new Vector3(9f, 2f, 9f));

        // Thin hanging targets give precise long-range grapple points.
        AddBox("NeedleA", new Vector3(-46f, 32f, -30f), new Vector3(2f, 36f, 2f));
        AddBox("NeedleB", new Vector3(46f, 40f, 26f), new Vector3(2f, 44f, 2f));
        AddBox("NeedleC", new Vector3(16f, 54f, 38f), new Vector3(2f, 28f, 2f));

        // Long runway for checking momentum after cancelling/replacing a cable.
        AddBox("SpeedLaneWestRail", new Vector3(-9f, 1f, 53f), new Vector3(1f, 2f, 38f));
        AddBox("SpeedLaneEastRail", new Vector3(9f, 1f, 53f), new Vector3(1f, 2f, 38f));

        // Collision layer 2: solid but intentionally not grapplable.
        AddBox(
            "NoGrappleBlock",
            new Vector3(48f, 5f, 54f),
            new Vector3(10f, 10f, 10f),
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
