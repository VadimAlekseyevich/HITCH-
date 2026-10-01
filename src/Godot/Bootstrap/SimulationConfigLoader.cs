using System.Text.Json;
using Godot;
using Hitch.Simulation;

namespace Hitch.GodotIntegration.Bootstrap;

internal static class SimulationConfigLoader
{
    public const string DefaultPath = "res://config/mvp_tuning.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static SimulationConfig Load(string path = DefaultPath)
    {
        var json = FileAccess.GetFileAsString(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException(
                $"Simulation tuning file '{path}' is missing or empty.");
        }

        SimulationConfig? config;

        try
        {
            config = JsonSerializer.Deserialize<SimulationConfig>(
                json,
                JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"Simulation tuning file '{path}' contains invalid JSON.",
                exception);
        }

        if (config is null)
        {
            throw new InvalidOperationException(
                $"Simulation tuning file '{path}' did not produce a configuration.");
        }

        config.Validate();
        return config;
    }
}
