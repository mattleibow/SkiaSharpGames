using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.LunarLander.LunarLanderConstants;

namespace SkiaSharpGames.LunarLander;

/// <summary>
/// Main gameplay screen. The lander entity uses child entities for thruster flames
/// that rotate automatically with the parent — demonstrating the Entity parenting system.
/// </summary>
internal sealed class PlayScreen(LunarLanderGameState state, IScreenCoordinator coordinator) : GameScreen
{
    // ── Entities ──────────────────────────────────────────────────────────
    private readonly Lander _lander = new();
    private readonly Terrain _terrain = new();

    // ── Input state ──────────────────────────────────────────────────────
    private bool _thrustInput;
    private bool _rotateLeftInput;
    private bool _rotateRightInput;
    private bool _touchThrust;

    // ── Stars ─────────────────────────────────────────────────────────────
    private readonly SKPoint[] _stars = new SKPoint[StarCount];
    private readonly float[] _starBrightness = new float[StarCount];

    // ── HUD text ──────────────────────────────────────────────────────────
    private readonly TextSprite _fuelText = new() { Size = 18f, Color = HudColor };
    private readonly TextSprite _speedText = new() { Size = 18f, Color = HudColor, Align = TextAlign.Right };
    private readonly TextSprite _altText = new() { Size = 18f, Color = HudColor, Align = TextAlign.Center };

    // ── Paints ────────────────────────────────────────────────────────────
    private readonly SKPaint _fuelBarBg = new() { Color = new SKColor(0x33, 0x33, 0x44), Style = SKPaintStyle.Fill };
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

    public override void OnPointerDown(float x, float y)
    {
        _touchThrust = true;

        // Touch on left/right side of screen for rotation
        if (x < GameWidth * 0.33f)
            _rotateLeftInput = true;
        else if (x > GameWidth * 0.67f)
            _rotateRightInput = true;
    }

    public override void OnPointerUp(float x, float y)
    {
        _touchThrust = false;
        _rotateLeftInput = false;
        _rotateRightInput = false;
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        if (_gameOver) return;

        // Rotation
        if (_rotateLeftInput)
            _lander.Rotation -= RotationSpeed * deltaTime;
        if (_rotateRightInput)
            _lander.Rotation += RotationSpeed * deltaTime;

        // Thrust
        bool thrusting = (_thrustInput || _touchThrust) && state.Fuel > 0f;
        if (thrusting)
        {
            float rot = _lander.Rotation;
            float thrustX = -MathF.Sin(rot) * ThrustForce * deltaTime;
            float thrustY = -MathF.Cos(rot) * ThrustForce * deltaTime;
            _lander.Rigidbody.AddVelocity(thrustX, thrustY);
            state.Fuel = MathF.Max(0f, state.Fuel - FuelBurnRate * deltaTime);
        }

        // Gravity
        _lander.Rigidbody.AddVelocity(0, Gravity * deltaTime);

        // Flame visibility — child entities rotate with the lander automatically!
        _lander.MainFlame.Visible = thrusting;
        _lander.LeftFlame.Visible = _rotateRightInput;
        _lander.RightFlame.Visible = _rotateLeftInput;

        // Update lander (rigidbody step + sprite + children)
        _lander.Update(deltaTime);

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
        coordinator.PushOverlay<GameOverScreen>();
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > MathF.PI) angle -= MathF.Tau;
        while (angle < -MathF.PI) angle += MathF.Tau;
        return angle;
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
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

        // Terrain
        _terrain.Draw(canvas);

        // Lander — one call draws hull + all flame children automatically!
        _lander.Draw(canvas);

        // HUD
        DrawHud(canvas);
    }

    private void DrawHud(SKCanvas canvas)
    {
        // Fuel bar
        float barX = 20f, barY = 16f, barW = 120f, barH = 14f;
        canvas.DrawRect(barX, barY, barW, barH, _fuelBarBg);
        float fuelFrac = state.Fuel / FuelMax;
        _fuelBarFg.Color = fuelFrac > 0.3f ? PadColor : DangerColor;
        canvas.DrawRect(barX, barY, barW * fuelFrac, barH, _fuelBarFg);
        _fuelText.Text = $"FUEL {state.Fuel:F0}";
        canvas.Save(); canvas.Translate(barX, barY + barH + 16f); _fuelText.Draw(canvas); canvas.Restore();

        // Speed
        float speed = _lander.Rigidbody.Speed;
        _speedText.Color = speed > MaxLandingSpeed ? DangerColor : HudColor;
        _speedText.Text = $"SPD {speed:F0}";
        canvas.Save(); canvas.Translate(GameWidth - 20f, 30f); _speedText.Draw(canvas); canvas.Restore();

        // Altitude
        float altitude = MathF.Max(0f, _terrain.GetHeightAt(_lander.X) - _lander.Y - LanderHeight / 2f);
        _altText.Text = $"ALT {altitude:F0}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 30f); _altText.Draw(canvas); canvas.Restore();
    }
}
