using System.Globalization;
using System.Text;
using Godot;
using Hitch.GodotIntegration.World;
using Hitch.Simulation;

namespace Hitch.GodotIntegration.Debug;

/// <summary>
/// Stage 5 developer tuning window. Movement/winch feel values can be changed without
/// restarting or resetting simulation state, then copied as a compact block for discussion.
/// </summary>
public partial class RuntimeTuningPanel : Control
{
    private sealed record Parameter(
        string Section,
        string Label,
        string Key,
        double Minimum,
        double Maximum,
        double Step,
        string Unit,
        Func<SimulationConfig, float> Read,
        Func<SimulationConfig, float, SimulationConfig> Write);

    private readonly List<(Parameter Parameter, HSlider Slider, LineEdit Editor)> _rows = new();

    private SimulationConfig _config = new();
    private SimulationConfig _initialConfig = new();
    private Action<SimulationConfig>? _applyConfig;
    private Label _status = null!;
    private bool _syncing;

    public event Action? CloseRequested;

    public bool IsOpen => Visible;

    public override void _Ready()
    {
        BuildUi();
    }

    public void Initialize(
        SimulationConfig config,
        Action<SimulationConfig> applyConfig)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(applyConfig);

        _config = config;
        _initialConfig = config;
        _applyConfig = applyConfig;
        SyncFromConfig();
        Visible = false;
    }

    public void OpenPanel()
    {
        SyncFromConfig();
        Visible = true;
        _status.Text = "Изменения применяются сразу. F2 — закрыть.";
    }

    public void ClosePanel()
    {
        Visible = false;
    }

    private void BuildUi()
    {
        MouseFilter = MouseFilterEnum.Ignore;

        var panel = new PanelContainer
        {
            Name = "Panel",
            Position = new Vector2(18f, 18f),
            Size = new Vector2(690f, 880f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 14);
        margin.AddThemeConstantOverride("margin_right", 14);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        panel.AddChild(margin);

        var root = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        root.AddThemeConstantOverride("separation", 8);
        margin.AddChild(root);

        var title = new Label
        {
            Text = "HITCH! — настройка движения",
        };
        title.AddThemeFontSizeOverride("font_size", 22);
        root.AddChild(title);

        root.AddChild(new Label
        {
            Text =
                $"Город: {MovementLabBuilder.RoomHalfWidth * 2f:F0} × " +
                $"{MovementLabBuilder.RoomHalfDepth * 2f:F0} × " +
                $"{MovementLabBuilder.RoomHeight:F0} м | F2 — открыть/закрыть",
        });

        var scroll = new ScrollContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0f, 690f),
        };
        root.AddChild(scroll);

        var parametersRoot = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        parametersRoot.AddThemeConstantOverride("separation", 5);
        scroll.AddChild(parametersRoot);

        string? currentSection = null;

        foreach (var parameter in CreateParameters())
        {
            if (parameter.Section != currentSection)
            {
                if (currentSection is not null)
                {
                    parametersRoot.AddChild(new HSeparator());
                }

                currentSection = parameter.Section;
                var sectionLabel = new Label
                {
                    Text = currentSection,
                };
                sectionLabel.AddThemeFontSizeOverride("font_size", 18);
                parametersRoot.AddChild(sectionLabel);
            }

            AddParameterRow(parametersRoot, parameter);
        }

        root.AddChild(new HSeparator());

        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 8);
        root.AddChild(buttons);

        var copy = new Button
        {
            Text = "Скопировать параметры",
        };
        copy.Pressed += CopyParameters;
        buttons.AddChild(copy);

        var reset = new Button
        {
            Text = "Сбросить к стартовым",
        };
        reset.Pressed += ResetParameters;
        buttons.AddChild(reset);

        var close = new Button
        {
            Text = "Закрыть (F2)",
        };
        close.Pressed += () => CloseRequested?.Invoke();
        buttons.AddChild(close);

        _status = new Label
        {
            Text = "Изменения применяются сразу.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        root.AddChild(_status);
    }

    private void AddParameterRow(
        VBoxContainer parent,
        Parameter parameter)
    {
        var row = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        row.AddThemeConstantOverride("separation", 8);
        parent.AddChild(row);

        var label = new Label
        {
            Text = parameter.Label,
            CustomMinimumSize = new Vector2(255f, 0f),
        };
        row.AddChild(label);

        var slider = new HSlider
        {
            MinValue = parameter.Minimum,
            MaxValue = parameter.Maximum,
            Step = parameter.Step,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(250f, 0f),
        };
        row.AddChild(slider);

        var editor = new LineEdit
        {
            CustomMinimumSize = new Vector2(92f, 0f),
            SelectAllOnFocus = true,
        };
        row.AddChild(editor);

        var unit = new Label
        {
            Text = parameter.Unit,
            CustomMinimumSize = new Vector2(48f, 0f),
        };
        row.AddChild(unit);

        slider.ValueChanged += value =>
        {
            if (_syncing)
            {
                return;
            }

            editor.Text = FormatValue(value, parameter.Step);
            ApplyValue(parameter, (float)value);
        };

        editor.TextSubmitted += text =>
            ApplyEditor(parameter, slider, editor, text);

        editor.FocusExited += () =>
            ApplyEditor(parameter, slider, editor, editor.Text);

        _rows.Add((parameter, slider, editor));
    }

    private void ApplyEditor(
        Parameter parameter,
        HSlider slider,
        LineEdit editor,
        string text)
    {
        if (_syncing)
        {
            return;
        }

        var normalized = text.Trim().Replace(',', '.');

        if (!double.TryParse(
                normalized,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            _status.Text =
                $"Не удалось прочитать число: «{text}».";
            SyncFromConfig();
            return;
        }

        parsed = Math.Clamp(
            parsed,
            parameter.Minimum,
            parameter.Maximum);

        _syncing = true;
        slider.Value = parsed;
        editor.Text = FormatValue(parsed, parameter.Step);
        _syncing = false;

        ApplyValue(parameter, (float)parsed);
    }

    private void ApplyValue(
        Parameter parameter,
        float value)
    {
        try
        {
            var candidate =
                parameter.Write(_config, value);
            candidate.Validate();

            _config = candidate;
            _applyConfig?.Invoke(candidate);
            _status.Text =
                $"Применено: {parameter.Label} = " +
                $"{FormatValue(value, parameter.Step)} {parameter.Unit}".TrimEnd();
        }
        catch (Exception exception)
        {
            _status.Text =
                $"Значение не применено: {exception.Message}";
            SyncFromConfig();
        }
    }

    private void SyncFromConfig()
    {
        if (_rows.Count == 0)
        {
            return;
        }

        _syncing = true;

        foreach (var row in _rows)
        {
            var value =
                row.Parameter.Read(_config);
            row.Slider.Value = value;
            row.Editor.Text =
                FormatValue(value, row.Parameter.Step);
        }

        _syncing = false;
    }

    private void ResetParameters()
    {
        _config = _initialConfig;
        _applyConfig?.Invoke(_config);
        SyncFromConfig();
        _status.Text = "Возвращён стартовый профиль этой сборки.";
    }

    private void CopyParameters()
    {
        DisplayServer.ClipboardSet(
            BuildClipboardText(_config));
        _status.Text =
            "Параметры скопированы в буфер обмена.";
    }

    private static string BuildClipboardText(
        SimulationConfig config)
    {
        var l = config.Locomotion;
        var w = config.Winch;
        var sb = new StringBuilder();

        sb.AppendLine("HITCH_TUNING_V1");
        sb.AppendLine(
            $"город={MovementLabBuilder.RoomHalfWidth * 2f:F0}x" +
            $"{MovementLabBuilder.RoomHalfDepth * 2f:F0}x" +
            $"{MovementLabBuilder.RoomHeight:F0}м");
        Append(sb, "Гравитация", "gravity", l.Gravity);
        Append(sb, "Скорость бега", "groundMaxSpeed", l.GroundMaxSpeed);
        Append(sb, "Разгон на земле", "groundAcceleration", l.GroundAcceleration);
        Append(sb, "Торможение", "groundBraking", l.GroundBraking);
        Append(sb, "Прыжок", "jumpSpeed", l.JumpSpeed);
        Append(sb, "Воздушный прыжок", "airJumpSpeed", l.AirJumpSpeed);
        Append(sb, "Управление в воздухе", "airAcceleration", l.AirAcceleration);
        Append(sb, "Air-control max", "airControlMaxSpeed", l.AirControlMaxSpeed);
        Append(sb, "Макс. длина троса", "maxRopeLength", w.MaxRopeLength);
        Append(sb, "Ускорение стяжки", "pullRadialAcceleration", w.PullRadialAcceleration);
        Append(sb, "Стяжка ближняя", "pullTargetInwardSpeed", w.PullTargetInwardSpeed);
        Append(sb, "Стяжка дальняя", "pullLongRangeInwardSpeed", w.PullLongRangeInwardSpeed);
        Append(sb, "Дистанция полной стяжки", "pullLongRangeDistance", w.PullLongRangeDistance);
        Append(sb, "Стартовый множитель", "pullLaunchInitialMultiplier", w.PullLaunchInitialMultiplier);
        Append(sb, "Пиковый множитель", "pullLaunchPeakMultiplier", w.PullLaunchPeakMultiplier);
        Append(sb, "Время до пика", "pullLaunchPeakSeconds", w.PullLaunchPeakSeconds);
        Append(sb, "Спад после пика", "pullLaunchDecaySeconds", w.PullLaunchDecaySeconds);
        Append(sb, "Газ ускорение", "gasAcceleration", w.GasAcceleration);
        Append(sb, "Газ полная тяга до", "gasFullAccelerationSpeed", w.GasFullAccelerationSpeed);
        Append(sb, "Газ выключен после", "gasCutoffSpeed", w.GasCutoffSpeed);
        Append(sb, "Два троса: сила каждого", "dualCableMotorScale", w.DualCableMotorScale);
        Append(sb, "Коррекция натяжения", "ropeConstraintCorrectionSpeed", w.RopeConstraintCorrectionSpeed);
        Append(sb, "Допуск натяжения", "ropeTautTolerance", w.RopeTautTolerance);
        Append(sb, "Захват точки", "arrivalSurfaceCaptureRadius", w.ArrivalSurfaceCaptureRadius);
        Append(sb, "Допуск контакта", "arrivalContactTolerance", w.ArrivalContactTolerance);
        Append(sb, "Отступ bend", "ropeContactSurfaceOffset", w.RopeContactSurfaceOffset);
        Append(sb, "Допуск конца троса", "ropeEndpointTolerance", w.RopeEndpointTolerance);
        Append(sb, "Мин. шаг bend", "ropeMinimumContactSpacing", w.RopeMinimumContactSpacing);
        Append(sb, "Поиск края", "ropeContactEdgeSearchDistance", w.RopeContactEdgeSearchDistance);

        return sb.ToString().TrimEnd();
    }

    private static void Append(
        StringBuilder sb,
        string russianName,
        string key,
        float value)
    {
        sb.Append(russianName)
            .Append(" (")
            .Append(key)
            .Append(")=")
            .AppendLine(
                value.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture));
    }

    private static string FormatValue(
        double value,
        double step)
    {
        var decimals = step switch
        {
            >= 1d => 0,
            >= 0.1d => 1,
            >= 0.01d => 2,
            _ => 3,
        };

        return value.ToString(
            $"F{decimals}",
            CultureInfo.InvariantCulture);
    }

    private static IReadOnlyList<Parameter> CreateParameters() =>
        new Parameter[]
        {
            new("Персонаж", "Гравитация", "gravity", 5, 25, 0.1, "м/с²",
                c => c.Locomotion.Gravity,
                (c, v) => c with { Locomotion = c.Locomotion with { Gravity = v } }),
            new("Персонаж", "Скорость бега", "groundMaxSpeed", 3, 16, 0.1, "м/с",
                c => c.Locomotion.GroundMaxSpeed,
                (c, v) => c with { Locomotion = c.Locomotion with { GroundMaxSpeed = v } }),
            new("Персонаж", "Разгон на земле", "groundAcceleration", 10, 160, 1, "м/с²",
                c => c.Locomotion.GroundAcceleration,
                (c, v) => c with { Locomotion = c.Locomotion with { GroundAcceleration = v } }),
            new("Персонаж", "Торможение", "groundBraking", 10, 220, 1, "м/с²",
                c => c.Locomotion.GroundBraking,
                (c, v) => c with { Locomotion = c.Locomotion with { GroundBraking = v } }),
            new("Персонаж", "Прыжок", "jumpSpeed", 2, 12, 0.1, "м/с",
                c => c.Locomotion.JumpSpeed,
                (c, v) => c with { Locomotion = c.Locomotion with { JumpSpeed = v } }),
            new("Персонаж", "Воздушный прыжок", "airJumpSpeed", 1, 10, 0.1, "м/с",
                c => c.Locomotion.AirJumpSpeed,
                (c, v) => c with { Locomotion = c.Locomotion with { AirJumpSpeed = v } }),
            new("Персонаж", "Управление в воздухе", "airAcceleration", 0, 30, 0.5, "м/с²",
                c => c.Locomotion.AirAcceleration,
                (c, v) => c with { Locomotion = c.Locomotion with { AirAcceleration = v } }),
            new("Персонаж", "Air-control max", "airControlMaxSpeed", 2, 16, 0.1, "м/с",
                c => c.Locomotion.AirControlMaxSpeed,
                (c, v) => c with { Locomotion = c.Locomotion with { AirControlMaxSpeed = v } }),

            new("Стяжка / УПМ", "Макс. длина троса", "maxRopeLength", 30, 300, 1, "м",
                c => c.Winch.MaxRopeLength,
                (c, v) => c with { Winch = c.Winch with { MaxRopeLength = v } }),
            new("Стяжка / УПМ", "Ускорение стяжки", "pullRadialAcceleration", 20, 300, 1, "м/с²",
                c => c.Winch.PullRadialAcceleration,
                (c, v) => c with { Winch = c.Winch with { PullRadialAcceleration = v } }),
            new("Стяжка / УПМ", "Стяжка: ближняя", "pullTargetInwardSpeed", 5, 50, 0.5, "м/с",
                c => c.Winch.PullTargetInwardSpeed,
                (c, v) => c with { Winch = c.Winch with { PullTargetInwardSpeed = v } }),
            new("Стяжка / УПМ", "Стяжка: дальняя", "pullLongRangeInwardSpeed", 10, 70, 0.5, "м/с",
                c => c.Winch.PullLongRangeInwardSpeed,
                (c, v) => c with { Winch = c.Winch with { PullLongRangeInwardSpeed = v } }),
            new("Стяжка / УПМ", "Дистанция полной стяжки", "pullLongRangeDistance", 20, 220, 1, "м",
                c => c.Winch.PullLongRangeDistance,
                (c, v) => c with { Winch = c.Winch with { PullLongRangeDistance = v } }),
            new("Стяжка / УПМ", "Стартовый множитель", "pullLaunchInitialMultiplier", 1, 2.5, 0.01, "×",
                c => c.Winch.PullLaunchInitialMultiplier,
                (c, v) => c with { Winch = c.Winch with { PullLaunchInitialMultiplier = v } }),
            new("Стяжка / УПМ", "Пиковый множитель", "pullLaunchPeakMultiplier", 1, 3, 0.01, "×",
                c => c.Winch.PullLaunchPeakMultiplier,
                (c, v) => c with { Winch = c.Winch with { PullLaunchPeakMultiplier = v } }),
            new("Стяжка / УПМ", "Время до пика", "pullLaunchPeakSeconds", 0.03, 0.40, 0.01, "с",
                c => c.Winch.PullLaunchPeakSeconds,
                (c, v) => c with { Winch = c.Winch with { PullLaunchPeakSeconds = v } }),
            new("Стяжка / УПМ", "Спад после пика", "pullLaunchDecaySeconds", 0.10, 1.50, 0.01, "с",
                c => c.Winch.PullLaunchDecaySeconds,
                (c, v) => c with { Winch = c.Winch with { PullLaunchDecaySeconds = v } }),
            new("Стяжка / УПМ", "Газ: ускорение", "gasAcceleration", 0, 30, 0.5, "м/с²",
                c => c.Winch.GasAcceleration,
                (c, v) => c with { Winch = c.Winch with { GasAcceleration = v } }),
            new("Стяжка / УПМ", "Газ: полная тяга до", "gasFullAccelerationSpeed", 5, 50, 0.5, "м/с",
                c => c.Winch.GasFullAccelerationSpeed,
                (c, v) => c with { Winch = c.Winch with { GasFullAccelerationSpeed = v } }),
            new("Стяжка / УПМ", "Газ: тяга = 0 после", "gasCutoffSpeed", 10, 90, 0.5, "м/с",
                c => c.Winch.GasCutoffSpeed,
                (c, v) => c with { Winch = c.Winch with { GasCutoffSpeed = v } }),
            new("Стяжка / УПМ", "Сила каждого при 2 тросах", "dualCableMotorScale", 0.35, 1, 0.01, "×",
                c => c.Winch.DualCableMotorScale,
                (c, v) => c with { Winch = c.Winch with { DualCableMotorScale = v } }),

            new("Геометрия троса", "Коррекция натяжения", "ropeConstraintCorrectionSpeed", 2, 45, 0.5, "м/с",
                c => c.Winch.RopeConstraintCorrectionSpeed,
                (c, v) => c with { Winch = c.Winch with { RopeConstraintCorrectionSpeed = v } }),
            new("Геометрия троса", "Допуск натяжения", "ropeTautTolerance", 0, 0.5, 0.01, "м",
                c => c.Winch.RopeTautTolerance,
                (c, v) => c with { Winch = c.Winch with { RopeTautTolerance = v } }),
            new("Геометрия троса", "Радиус захвата точки", "arrivalSurfaceCaptureRadius", 0.5, 5, 0.05, "м",
                c => c.Winch.ArrivalSurfaceCaptureRadius,
                (c, v) => c with { Winch = c.Winch with { ArrivalSurfaceCaptureRadius = v } }),
            new("Геометрия троса", "Допуск контакта", "arrivalContactTolerance", 0, 0.5, 0.01, "м",
                c => c.Winch.ArrivalContactTolerance,
                (c, v) => c with { Winch = c.Winch with { ArrivalContactTolerance = v } }),
            new("Геометрия троса", "Отступ bend-точки", "ropeContactSurfaceOffset", 0.02, 0.30, 0.01, "м",
                c => c.Winch.RopeContactSurfaceOffset,
                (c, v) => c with { Winch = c.Winch with { RopeContactSurfaceOffset = v } }),
            new("Геометрия троса", "Допуск конца сегмента", "ropeEndpointTolerance", 0.05, 0.80, 0.01, "м",
                c => c.Winch.RopeEndpointTolerance,
                (c, v) => c with { Winch = c.Winch with { RopeEndpointTolerance = v } }),
            new("Геометрия троса", "Мин. шаг bend", "ropeMinimumContactSpacing", 0.10, 1.50, 0.01, "м",
                c => c.Winch.RopeMinimumContactSpacing,
                (c, v) => c with { Winch = c.Winch with { RopeMinimumContactSpacing = v } }),
            new("Геометрия троса", "Поиск края", "ropeContactEdgeSearchDistance", 2, 40, 0.5, "м",
                c => c.Winch.RopeContactEdgeSearchDistance,
                (c, v) => c with { Winch = c.Winch with { RopeContactEdgeSearchDistance = v } }),
        };
}
