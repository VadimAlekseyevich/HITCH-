using Godot;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Tiny development-only dynamic line renderer.
///
/// Call BeginFrame(), DrawLine(...) zero or more times, then Commit().
/// The segment buffer is reused to avoid per-frame collection allocations after warmup.
/// </summary>
public partial class DebugLineDrawer3D : MeshInstance3D
{
    private readonly ImmediateMesh _immediateMesh = new();
    private readonly List<DebugLine> _lines = new(capacity: 16);

    private readonly StandardMaterial3D _material = new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        VertexColorUseAsAlbedo = true,
        DisableFog = true,
        NoDepthTest = false,
    };

    public override void _Ready()
    {
        Mesh = _immediateMesh;
        CastShadow = ShadowCastingSetting.Off;
    }

    public void BeginFrame()
    {
        _lines.Clear();
    }

    public void DrawLine(Vector3 from, Vector3 to, Color color)
    {
        _lines.Add(new DebugLine(from, to, color));
    }

    public void Commit()
    {
        _immediateMesh.ClearSurfaces();

        if (_lines.Count == 0)
        {
            return;
        }

        _immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, _material);

        foreach (var line in _lines)
        {
            _immediateMesh.SurfaceSetColor(line.Color);
            _immediateMesh.SurfaceAddVertex(line.From);
            _immediateMesh.SurfaceAddVertex(line.To);
        }

        _immediateMesh.SurfaceEnd();
    }

    public void Clear()
    {
        _lines.Clear();
        _immediateMesh.ClearSurfaces();
    }

    private readonly record struct DebugLine(
        Vector3 From,
        Vector3 To,
        Color Color);
}
