using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Between-stage upgrade selection screen. Player picks 1 of 3 random upgrades.
/// </summary>
internal sealed class UpgradeScreen(StarfallGameState state, IDirector director) : Scene
{
    private readonly Starfield _starfield = new();
    private readonly List<UpgradeOption> _options = [];
    private int _selectedIndex = -1;
    private float _time;
    private bool _chosen;

    private readonly HudLabel _title = new()
    {
        Text = "CHOOSE AN UPGRADE",
        FontSize = 42f,
        Color = CyanAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 100f,
    };
    private readonly HudLabel _stageComplete = new()
    {
        FontSize = 20f,
        Color = GreenAccent,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 58f,
    };
    private readonly HudLabel _scoreDisplay = new()
    {
        FontSize = 18f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 135f,
    };

    private static readonly UpgradeOption[] AllUpgrades =
    [
        new("MAX HP +1", "Increase maximum health by 1", MagentaAccent, s => { s.MaxHP = Math.Min(s.MaxHP + 1, PlayerMaxHP); s.HP = Math.Min(s.HP + 1, s.MaxHP); }),
        new("FIRE RATE +15%", "Permanently increase fire rate", YellowAccent, s => s.FireRateMultiplier *= 1.15f),
        new("BULLET DMG +1", "Each bullet deals 1 extra damage", RedAccent, s => s.BulletDamage++),
        new("SHIP SPEED +10%", "Move faster to dodge attacks", CyanAccent, s => s.SpeedMultiplier *= 1.10f),
        new("BOMB CAPACITY +1", "Carry one more bomb", MagentaAccent, s => { s.MaxBombs++; s.Bombs = Math.Min(s.Bombs + 1, s.MaxBombs); }),
        new("SCORE x0.5 BOOST", "Permanently increase score multiplier", GreenAccent, s => s.ScoreMultiplier += 0.5f),
        new("FULL REPAIR", "Restore all HP to max", GreenAccent, s => s.HP = s.MaxHP),
        new("RAPID FIRE", "Start next stage with rapid fire", YellowAccent, s => s.RapidFireTimer = PowerUpDuration),
        new("SPREAD SHOT", "Start next stage with spread shot", CyanAccent, s => s.SpreadShotTimer = PowerUpDuration),
    ];

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_starfield);
            Children.Add(_title);
            Children.Add(_stageComplete);
            Children.Add(_scoreDisplay);
        }
    }

    public override void OnActivated()
    {
        _chosen = false;
        _selectedIndex = -1;
        _time = 0;

        _stageComplete.Text = $"SECTOR {state.CurrentStage} CLEARED!";
        _scoreDisplay.Text = $"Score: {state.Score:N0}";

        // Pick 3 random unique upgrades
        _options.Clear();
        var available = AllUpgrades.ToList();
        for (int i = 0; i < Math.Min(UpgradeChoices, available.Count); i++)
        {
            int idx = Random.Shared.Next(available.Count);
            _options.Add(available[idx]);
            available.RemoveAt(idx);
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;
    }

    public override void OnPointerDown(float x, float y)
    {
        if (_chosen) return;

        // Check which upgrade card was clicked
        for (int i = 0; i < _options.Count; i++)
        {
            var rect = GetCardRect(i);
            if (rect.Contains(x, y))
            {
                SelectUpgrade(i);
                return;
            }
        }
    }

    public override void OnKeyDown(string key)
    {
        if (_chosen) return;

        switch (key)
        {
            case "1":
                if (_options.Count > 0) SelectUpgrade(0);
                break;
            case "2":
                if (_options.Count > 1) SelectUpgrade(1);
                break;
            case "3":
                if (_options.Count > 2) SelectUpgrade(2);
                break;
        }
    }

    private void SelectUpgrade(int index)
    {
        if (_chosen || index < 0 || index >= _options.Count) return;
        _chosen = true;
        _selectedIndex = index;

        // Apply upgrade
        _options[index].Apply(state);

        // Advance stage
        state.CurrentStage++;

        // Reset power-up timers (except those granted by upgrade)
        if (_options[index].Name != "RAPID FIRE")
            state.RapidFireTimer = 0;
        if (_options[index].Name != "SPREAD SHOT")
            state.SpreadShotTimer = 0;
        state.ShieldTimer = 0;

        // Transition to next stage
        director.TransitionTo<PlayScreen>(new FadeCurtain());
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        // Draw upgrade cards
        for (int i = 0; i < _options.Count; i++)
        {
            DrawCard(canvas, i, _options[i]);
        }
    }

    private SKRect GetCardRect(int index)
    {
        float cardW = 200f;
        float cardH = 160f;
        float gap = 30f;
        float totalW = _options.Count * cardW + (_options.Count - 1) * gap;
        float startX = (GameWidth - totalW) / 2f;
        float y = 200f;
        return SKRect.Create(startX + index * (cardW + gap), y, cardW, cardH);
    }

    private void DrawCard(SKCanvas canvas, int index, UpgradeOption option)
    {
        var rect = GetCardRect(index);
        bool selected = _selectedIndex == index;
        float pulse = 0.7f + 0.3f * MathF.Sin(_time * 3f + index);

        // Card background
        using var bgPaint = new SKPaint
        {
            Color = selected ? option.Color.WithAlpha(40) : new SKColor(0x11, 0x11, 0x22, 200),
            IsAntialias = true,
        };
        canvas.DrawRoundRect(rect, 10f, 10f, bgPaint);

        // Border
        using var borderPaint = new SKPaint
        {
            Color = selected ? option.Color : option.Color.WithAlpha((byte)(120 * pulse)),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = selected ? 2.5f : 1.5f,
        };
        canvas.DrawRoundRect(rect, 10f, 10f, borderPaint);

        // Glow on selected
        if (selected)
        {
            borderPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f);
            borderPaint.Color = option.Color.WithAlpha(60);
            canvas.DrawRoundRect(rect, 10f, 10f, borderPaint);
        }

        // Key number
        using var numPaint = new SKPaint { Color = HudDimColor, IsAntialias = true };
        var numFont = new SKFont { Size = 14f };
        canvas.DrawText($"[{index + 1}]", rect.Left + 10f, rect.Top + 22f, numFont, numPaint);

        // Title
        using var titlePaint = new SKPaint { Color = option.Color, IsAntialias = true };
        var titleFont = new SKFont { Size = 18f };
        float titleW = titleFont.MeasureText(option.Name);
        canvas.DrawText(option.Name, rect.MidX - titleW / 2f, rect.Top + 55f, titleFont, titlePaint);

        // Description (word wrap manually at card width)
        using var descPaint = new SKPaint { Color = HudDimColor, IsAntialias = true };
        var descFont = new SKFont { Size = 14f };
        float maxW = rect.Width - 20f;
        DrawWrappedText(canvas, option.Description, descFont, descPaint, rect.Left + 10f, rect.Top + 85f, maxW);
    }

    private static void DrawWrappedText(SKCanvas canvas, string text, SKFont font, SKPaint paint, float x, float y, float maxWidth)
    {
        var words = text.Split(' ');
        float lineY = y;
        string line = "";

        foreach (var word in words)
        {
            string test = line.Length == 0 ? word : line + " " + word;
            if (font.MeasureText(test) > maxWidth && line.Length > 0)
            {
                canvas.DrawText(line, x, lineY, font, paint);
                line = word;
                lineY += 18f;
            }
            else
            {
                line = test;
            }
        }
        if (line.Length > 0)
            canvas.DrawText(line, x, lineY, font, paint);
    }
}

internal sealed record UpgradeOption(string Name, string Description, SKColor Color, Action<StarfallGameState> Apply);
