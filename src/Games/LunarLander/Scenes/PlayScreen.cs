using SkiaSharp;
using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Themes.Default;

using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>
/// Main gameplay scene. The lander actor uses child actors for thruster flames
/// that rotate automatically with the parent — demonstrating the Actor parenting system.
/// </summary>
internal sealed class PlayScreen(LunarLanderGameState state, IDirector director) : Scene
{
    // ── Entities ──────────────────────────────────────────────────────────
    private readonly Lander _lander = new() { Name = "lander" };
    private readonly Terrain _terrain = new();

    // ── Input state ──────────────────────────────────────────────────────
    private bool _thrustInput;
    private bool _rotateLeftInput;
    private bool _rotateRightInput;

    // ── Touch control pad ────────────────────────────────────────────────
    private bool _touchActive;
    private static readonly float PadY = GameHeight - 90f;
    private static readonly float PadBtnW = 80f;
    private static readonly float PadBtnH = 60f;
    private static readonly float PadGap = 12f;

    // Three buttons: [← rotate] [▲ thrust] [rotate →]
    private static readonly float PadTotalW = PadBtnW * 3 + PadGap * 2;
    private static readonly float PadLeft = (GameWidth - PadTotalW) / 2f;
    private static readonly SKRect LeftBtnRect = SKRect.Create(PadLeft, PadY, PadBtnW, PadBtnH);
    private static readonly SKRect ThrustBtnRect = SKRect.Create(
        PadLeft + PadBtnW + PadGap,
        PadY,
        PadBtnW,
        PadBtnH
    );
    private static readonly SKRect RightBtnRect = SKRect.Create(
        PadLeft + 2 * (PadBtnW + PadGap),
        PadY,
        PadBtnW,
        PadBtnH
    );

    private bool _touchLeft,
        _touchThrust,
        _touchRight;

    // ── Stars ─────────────────────────────────────────────────────────────
    private readonly SKPoint[] _stars = new SKPoint[StarCount];
    private readonly float[] _starBrightness = new float[StarCount];

    // ── HUD text ──────────────────────────────────────────────────────────
    private readonly HudLabel _fuelText = new()
    {
        FontSize = 18f,
        Color = HudColor,
        X = 20f,
        Y = 46f,
    };
    private readonly HudLabel _speedText = new()
    {
        FontSize = 18f,
        Color = HudColor,
        Align = TextAlign.Right,
        X = GameWidth - 20f,
        Y = 30f,
    };
    private readonly HudLabel _altText = new()
    {
        FontSize = 18f,
        Color = HudColor,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = 30f,
    };

    // ── Paints ────────────────────────────────────────────────────────────
    private readonly SKPaint _fuelBarBg = new()
    {
        Color = new SKColor(0x33, 0x33, 0x44),
        Style = SKPaintStyle.Fill,
    };
    private readonly SKPaint _fuelBarFg = new() { Style = SKPaintStyle.Fill };

    private bool _gameOver;
    private Random _rng = new();

    public override void OnActivating()
    {
        _gameOver = false;
        state.Fuel = FuelMax;
        state.Landed = false;
        state.Crashed = false;

        _rng = new Random();

        // Generate terrain
        _terrain.Generate(_rng);

        // Position lander at top-centre
        _lander.X = GameWidth / 2f;
        _lander.Y = 60f;
        _lander.Rotation = 0f;
        _lander.Rigidbody.SetVelocity(0, 0);
        _lander.MainFlame.Visible = false;
        _lander.LeftFlame.Visible = false;
        _lander.RightFlame.Visible = false;

        // Generate stars
        for (int i = 0; i < StarCount; i++)
        {
            _stars[i] = new SKPoint(_rng.Next(0, GameWidth), _rng.Next(0, GameHeight));
            _starBrightness[i] = 0.3f + (float)_rng.NextDouble() * 0.7f;
        }

        if (ChildCount == 0)
        {
            Children.Add(_lander);
            Children.Add(_fuelText);
            Children.Add(_speedText);
            Children.Add(_altText);
        }
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowUp" or " ":
                _thrustInput = true;
                break;
            case "ArrowLeft":
                _rotateLeftInput = true;
                break;
            case "ArrowRight":
                _rotateRightInput = true;
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowUp" or " ":
                _thrustInput = false;
                break;
            case "ArrowLeft":
                _rotateLeftInput = false;
                break;
            case "ArrowRight":
                _rotateRightInput = false;
                break;
        }
    }

    public override void OnPointerDown(float x, float y) => HandleTouch(x, y, true);

    public override void OnPointerMove(float x, float y)
    {
        if (_touchActive)
            HandleTouch(x, y, true);
    }

    public override void OnPointerUp(float x, float y)
    {
        _touchActive = false;
        _touchLeft = false;
        _touchThrust = false;
        _touchRight = false;
    }

    private void HandleTouch(float x, float y, bool down)
    {
        _touchActive = down;
        _touchLeft = down && LeftBtnRect.Contains(x, y);
        _touchThrust = down && ThrustBtnRect.Contains(x, y);
        _touchRight = down && RightBtnRect.Contains(x, y);
    }

    // ── Update ────────────────────────────────────────────────────────────

    protected override void OnUpdate(float deltaTime)
    {
        if (_gameOver)
            return;

        // Combine keyboard + touch inputs
        bool wantLeft = _rotateLeftInput || _touchLeft;
        bool wantRight = _rotateRightInput || _touchRight;
        bool wantThrust = _thrustInput || _touchThrust;

        // Rotation (consumes fuel via side thrusters)
        if (wantLeft && state.Fuel > 0f)
        {
            _lander.Rotation -= RotationSpeed * deltaTime;
            state.Fuel = MathF.Max(0f, state.Fuel - FuelBurnRate * 0.3f * deltaTime);
        }
        if (wantRight && state.Fuel > 0f)
        {
            _lander.Rotation += RotationSpeed * deltaTime;
            state.Fuel = MathF.Max(0f, state.Fuel - FuelBurnRate * 0.3f * deltaTime);
        }

        // Thrust
        bool thrusting = wantThrust && state.Fuel > 0f;
        if (thrusting)
        {
            float rot = _lander.Rotation;
            float thrustX = MathF.Sin(rot) * ThrustForce * deltaTime;
            float thrustY = -MathF.Cos(rot) * ThrustForce * deltaTime;
            _lander.Rigidbody.AddVelocity(thrustX, thrustY);
            state.Fuel = MathF.Max(0f, state.Fuel - FuelBurnRate * deltaTime);
        }

        // Gravity
        _lander.Rigidbody.AddVelocity(0, Gravity * deltaTime);

        // Flame visibility — child actors rotate with the lander automatically!
        bool hasFuel = state.Fuel > 0f;
        _lander.MainFlame.Visible = thrusting;
        _lander.LeftFlame.Visible = wantRight && hasFuel;
        _lander.RightFlame.Visible = wantLeft && hasFuel;

        // Update HUD labels
        _fuelText.Text = $"FUEL {state.Fuel:F0}";
        float speed = _lander.Rigidbody.Speed;
        _speedText.Color = speed > MaxLandingSpeed ? DangerColor : HudColor;
        _speedText.Text = $"SPD {speed:F0}";
        float altitude = MathF.Max(
            0f,
            _terrain.GetHeightAt(_lander.X) - _lander.Y - LanderHeight / 2f
        );
        _altText.Text = $"ALT {altitude:F0}";

        // Landing / crash detection
        CheckLanding();
    }

    private void CheckLanding()
    {
        float landerBottom = _lander.Y + LanderHeight / 2f + 6f; // include legs
        float terrainY = _terrain.GetHeightAt(_lander.X);
        bool overPad = _terrain.IsOverPad(_lander.X);
        float padTop = _terrain.PadY - LandingPadHeight;

        float contactY = overPad ? padTop : terrainY;

        if (landerBottom < contactY)
            return; // still in the air

        float speed = _lander.Rigidbody.Speed;
        float angle = MathF.Abs(NormalizeAngle(_lander.Rotation));

        if (overPad && speed < MaxLandingSpeed && angle < MaxLandingAngle)
        {
            // Safe landing!
            state.Landed = true;
            _lander.Rigidbody.Stop();
            _lander.Y = contactY - LanderHeight / 2f - 6f;
            _lander.Rotation = 0f;
        }
        else
        {
            // Crash!
            state.Crashed = true;
            _lander.Rigidbody.Stop();
        }

        _lander.MainFlame.Visible = false;
        _lander.LeftFlame.Visible = false;
        _lander.RightFlame.Visible = false;
        _gameOver = true;
        director.PushScene<GameOverScreen>();
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > MathF.PI)
            angle -= MathF.Tau;
        while (angle < -MathF.PI)
            angle += MathF.Tau;
        return angle;
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);
        DrawGameContent(canvas);
    }

    internal void DrawGameContent(SKCanvas canvas)
    {
        // Stars
        using var starPaint = new SKPaint { IsAntialias = true };
        for (int i = 0; i < StarCount; i++)
        {
            byte a = (byte)(255 * _starBrightness[i]);
            starPaint.Color = SKColors.White.WithAlpha(a);
            canvas.DrawCircle(_stars[i].X, _stars[i].Y, 1.2f, starPaint);
        }

        // Terrain (not a SceneNode — draw manually)
        _terrain.Draw(canvas);

        // HUD fuel bar (labels auto-draw as children)
        DrawHud(canvas);
        DrawControlPad(canvas);
    }

    private void DrawHud(SKCanvas canvas)
    {
        // Fuel bar (labels auto-draw as children)
        float barX = 20f,
            barY = 16f,
            barW = 120f,
            barH = 14f;
        canvas.DrawRect(barX, barY, barW, barH, _fuelBarBg);
        float fuelFrac = state.Fuel / FuelMax;
        _fuelBarFg.Color = fuelFrac > 0.3f ? PadColor : DangerColor;
        canvas.DrawRect(barX, barY, barW * fuelFrac, barH, _fuelBarFg);
    }

    private void DrawControlPad(SKCanvas canvas)
    {
        var appearance = (
            (ResolvedHudTheme?.Button ?? DefaultButtonAppearance.Default) as DefaultButtonAppearance
            ?? DefaultButtonAppearance.Default
        ) with
        {
            CornerRadius = 8f,
            BorderWidth = 1.5f,
            BevelSize = 1.5f,
        };
        appearance.DrawDirect(canvas, LeftBtnRect, "<", _touchLeft, fontSize: 22f);
        appearance.DrawDirect(canvas, ThrustBtnRect, "^", _touchThrust, fontSize: 22f);
        appearance.DrawDirect(canvas, RightBtnRect, ">", _touchRight, fontSize: 22f);
    }
}
