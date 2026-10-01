using Godot;

namespace Hitch.GodotIntegration.World;

/// <summary>
/// Development-only greybox movement laboratory.
///
/// Geometry is created from simple CSG boxes so Stage 4/5 iteration can change dimensions quickly
/// without introducing an art pipeline or final level-design assumptions.
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
        AddBox("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(60f, 1f, 80f), _floorMaterial);

        AddBox("WestWall", new Vector3(-30f, 4f, 0f), new Vector3(1f, 8f, 80f));
        AddBox("EastWall", new Vector3(30f, 4f, 0f), new Vector3(1f, 8f, 80f));
        AddBox("BackWall", new Vector3(0f, 5f, -40f), new Vector3(60f, 10f, 1f));
        AddBox("FrontWallLeft", new Vector3(-20f, 3f, 40f), new Vector3(20f, 6f, 1f));
        AddBox("FrontWallRight", new Vector3(20f, 3f, 40f), new Vector3(20f, 6f, 1f));

        AddBox("LowLedge", new Vector3(-14f, 1.5f, -14f), new Vector3(12f, 3f, 8f));
        AddBox("HighLedge", new Vector3(15f, 4f, -20f), new Vector3(12f, 8f, 9f));
        AddBox("StepBlock", new Vector3(-20f, 0.75f, 12f), new Vector3(6f, 1.5f, 6f));

        AddBox("TallPillarA", new Vector3(-10f, 7f, 5f), new Vector3(3f, 14f, 3f));
        AddBox("TallPillarB", new Vector3(11f, 9f, 2f), new Vector3(3f, 18f, 3f));
        AddBox("TallPillarC", new Vector3(20f, 6f, 15f), new Vector3(4f, 12f, 4f));

        AddBox("OverheadBeamCenter", new Vector3(0f, 11f, 2f), new Vector3(24f, 1.5f, 2f));
        AddBox("OverheadBeamBack", new Vector3(9f, 14f, -25f), new Vector3(26f, 1.5f, 2f));

        // A long clear lane with a deliberate crash wall for future high-speed collision testing.
        AddBox("SpeedLaneWestRail", new Vector3(-7f, 1f, 22f), new Vector3(1f, 2f, 28f));
        AddBox("SpeedLaneEastRail", new Vector3(7f, 1f, 22f), new Vector3(1f, 2f, 28f));
        AddBox("SpeedLaneCrashWall", new Vector3(0f, 3f, 8f), new Vector3(15f, 6f, 1f));
    }

    private void AddBox(
        string name,
        Vector3 position,
        Vector3 size,
        Material? material = null)
    {
        AddChild(new CsgBox3D
        {
            Name = name,
            Position = position,
            Size = size,
            Material = material ?? _obstacleMaterial,
            UseCollision = true,
        });
    }
}
