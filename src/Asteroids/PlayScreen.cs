using SkiaSharp;
using SkiaSharpGames.GameEngine;
using SkiaSharpGames.GameEngine.UI;
using static SkiaSharpGames.Asteroids.AsteroidsConstants;

namespace SkiaSharpGames.Asteroids;

internal sealed class PlayScreen(AsteroidsGameState state, IScreenCoordinator coordinator, UiTheme themes) : GameScreen
{
    private static readonly SKPaint _starPaint = new() { Color = SKColors.White.WithAlpha(80), IsAntialias = true };
    private static readonly SKPaint _livesShipPaint = new() { Color = ShipColor, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };

    private readonly UiLabel _scoreText = new() { FontSize = 24f, Color = SKColors.White };
    private readonly UiLabel _levelText = new() { FontSize = 18f, Color = HudDimColor, Align = TextAlign.Center };
    private readonly UiLabel _controlsText = new() { Text = "ARROWS rotate/thrust  SPACE fire", FontSize = 16f, Color = HudDimColor, Align = TextAlign.Center };

    private readonly Ship _ship = new();
    private readonly Entity _bullets = new();
    private readonly Entity _asteroids = new();
    private readonly Entity _debris = new();
    private readonly List<SKPoint> _stars = [];

    // ── Touch control pad ────────────────────────────────────────────────
    private bool _touchActive;
    private bool _touchLeft, _touchRight, _touchThrust, _touchFire;

    private const float PadY = GameHeight - 80f;
    private const float PadBtnW = 70f;
    private const float PadBtnH = 50f;
    private const float PadGap = 12f;
    private static readonly float PadTotalW = PadBtnW * 4 + PadGap * 3;
    private static readonly float PadLeft = (GameWidth - PadTotalW) / 2f;
    private static readonly SKRect LeftBtnRect = SKRect.Create(PadLeft, PadY, PadBtnW, PadBtnH);
    private static readonly SKRect ThrustBtnRect = SKRect.Create(PadLeft + PadBtnW + PadGap, PadY, PadBtnW, PadBtnH);
    private static readonly SKRect FireBtnRect = SKRect.Create(PadLeft + 2 * (PadBtnW + PadGap), PadY, PadBtnW, PadBtnH);
    private static readonly SKRect RightBtnRect = SKRect.Create(PadLeft + 3 * (PadBtnW + PadGap), PadY, PadBtnW, PadBtnH);

    private bool _leftHeld, _rightHeld, _thrustHeld;
    private bool _endTriggered;
    private float _fireCooldown;
    private float _respawnTimer;
    private bool _shipAlive = true;

    public override void OnActivated()
    {
        if (_stars.Count == 0)
        {
            var random = new Random(815);
            for (int i = 0; i < 80; i++)
                _stars.Add(new SKPoint(random.NextSingle() * GameWidth, random.NextSingle() * GameHeight));
        }

        state.Score = 0;
        state.Lives = InitialLives;
        state.Level = 1;
        state.NextExtraLife = ExtraLifeScore;
        _leftHeld = false;
        _rightHeld = false;
        _thrustHeld = false;
        _touchActive = false;
        _touchLeft = false;
        _touchRight = false;
        _touchThrust = false;
        _touchFire = false;
        _endTriggered = false;
        _fireCooldown = 0f;
        _respawnTimer = 0f;
        _shipAlive = true;

        ClearChildren(_bullets);
        ClearChildren(_asteroids);
        ClearChildren(_debris);

        ResetShip();
        SpawnAsteroids(state.Level);
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerDown(float x, float y) => HandleTouch(x, y, true);
    public override void OnPointerMove(float x, float y) { if (_touchActive) HandleTouch(x, y, true); }

    public override void OnPointerUp(float x, float y)
    {
        _touchActive = false;
        _touchLeft = false;
        _touchRight = false;
        _touchThrust = false;
        _touchFire = false;
    }

    private void HandleTouch(float x, float y, bool down)
    {
        _touchActive = down;
        _touchLeft = down && LeftBtnRect.Contains(x, y);
        _touchRight = down && RightBtnRect.Contains(x, y);
        _touchThrust = down && ThrustBtnRect.Contains(x, y);
        bool wasFire = _touchFire;
        _touchFire = down && FireBtnRect.Contains(x, y);
        if (_touchFire && !wasFire)
            TryFire();
    }

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft" or "a" or "A":
                _leftHeld = true;
                break;
            case "ArrowRight" or "d" or "D":
                _rightHeld = true;
                break;
            case "ArrowUp" or "w" or "W":
                _thrustHeld = true;
                break;
            case " " or "Enter":
                TryFire();
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft" or "a" or "A":
                _leftHeld = false;
                break;
            case "ArrowRight" or "d" or "D":
                _rightHeld = false;
                break;
            case "ArrowUp" or "w" or "W":
                _thrustHeld = false;
                break;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        if (_endTriggered)
            return;

        _fireCooldown = MathF.Max(0f, _fireCooldown - deltaTime);

        if (_shipAlive)
        {
            UpdateShipInput(deltaTime);
            _ship.Update(deltaTime);
        }
        else
        {
            _respawnTimer -= deltaTime;
            if (_respawnTimer <= 0f)
            {
                if (state.Lives <= 0)
                {
                    TriggerGameOver();
                    return;
                }
                RespawnShip();
            }
        }

        _bullets.Update(deltaTime);
        _asteroids.Update(deltaTime);
        _debris.Update(deltaTime);

        CheckCollisions();
        CheckLevelComplete();

        _bullets.RemoveInactiveChildren();
        _asteroids.RemoveInactiveChildren();
        _debris.RemoveInactiveChildren();
    }

    private void UpdateShipInput(float deltaTime)
    {
        bool rotateLeft = _leftHeld || _touchLeft;
        bool rotateRight = _rightHeld || _touchRight;
        bool thrust = _thrustHeld || _touchThrust;

        if (rotateLeft ^ rotateRight)
        {
            float direction = rotateLeft ? -1f : 1f;
            _ship.Rotation += direction * ShipRotationSpeed * deltaTime;
        }

        _ship.Thrusting = thrust;
        if (thrust)
        {
            float angle = _ship.Rotation - MathF.PI / 2f;
            _ship.VelocityX += MathF.Cos(angle) * ShipThrustAccel * deltaTime;
            _ship.VelocityY += MathF.Sin(angle) * ShipThrustAccel * deltaTime;
        }
    }

    private void TryFire()
    {
        if (!_shipAlive || _fireCooldown > 0f || _bullets.ChildCount >= MaxBullets || _endTriggered)
            return;

        _fireCooldown = FireCooldown;
        float angle = _ship.Rotation - MathF.PI / 2f;
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        var bullet = new Bullet(
            _ship.X + cos * ShipRadius,
            _ship.Y + sin * ShipRadius,
            cos * BulletSpeed + _ship.VelocityX * 0.3f,
            sin * BulletSpeed + _ship.VelocityY * 0.3f);

        _bullets.AddChild(bullet);
    }

    private void CheckCollisions()
    {
        var asteroidList = _asteroids.Children;
        var bulletList = _bullets.Children;

        // Bullets vs asteroids
        for (int i = bulletList.Count - 1; i >= 0; i--)
        {
            var bullet = bulletList[i];
            if (!bullet.Active) continue;

            for (int j = asteroidList.Count - 1; j >= 0; j--)
            {
                var ast = (Asteroid)asteroidList[j];
                if (!ast.Active) continue;

                if (bullet.Overlaps(ast))
                {
                    bullet.Active = false;
                    DestroyAsteroid(ast);
                    break;
                }
            }
        }

        // Ship vs asteroids
        if (_shipAlive && !_ship.IsInvincible)
        {
            for (int j = asteroidList.Count - 1; j >= 0; j--)
            {
                var ast = (Asteroid)asteroidList[j];
                if (!ast.Active) continue;

                if (_ship.Overlaps(ast))
                {
                    DestroyShip();
                    DestroyAsteroid(ast);
                    break;
                }
            }
        }
    }

    private void DestroyAsteroid(Asteroid ast)
    {
        ast.Active = false;
        state.Score += ast.ScoreValue;

        // Check for extra life
        if (state.Score >= state.NextExtraLife)
        {
            state.Lives++;
            state.NextExtraLife += ExtraLifeScore;
        }

        // Spawn debris
        SpawnDebris(ast.X, ast.Y, DebrisCount);

        // Split into smaller asteroids
        if (ast.Size != AsteroidSize.Small)
        {
            var newSize = ast.Size == AsteroidSize.Large ? AsteroidSize.Medium : AsteroidSize.Small;
            for (int k = 0; k < 2; k++)
            {
                float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
                float speed = newSize switch
                {
                    AsteroidSize.Medium => AsteroidMediumMinSpeed + Random.Shared.NextSingle() * (AsteroidMediumMaxSpeed - AsteroidMediumMinSpeed),
                    _ => AsteroidSmallMinSpeed + Random.Shared.NextSingle() * (AsteroidSmallMaxSpeed - AsteroidSmallMinSpeed),
                };
                float vx = MathF.Cos(angle) * speed;
                float vy = MathF.Sin(angle) * speed;
                var child = new Asteroid(newSize, ast.X, ast.Y, vx, vy, Random.Shared.Next());
                _asteroids.AddChild(child);
            }
        }
    }

    private void DestroyShip()
    {
        _shipAlive = false;
        state.Lives--;
        _respawnTimer = 2f;

        SpawnDebris(_ship.X, _ship.Y, 10);
    }

    private void RespawnShip()
    {
        _shipAlive = true;
        ResetShip();
        _ship.InvincibleTimer = ShipInvincibleDuration;
    }

    private void ResetShip()
    {
        _ship.X = GameWidth / 2f;
        _ship.Y = GameHeight / 2f;
        _ship.Rotation = 0f;
        _ship.VelocityX = 0f;
        _ship.VelocityY = 0f;
        _ship.Thrusting = false;
        _ship.Active = true;
    }

    private void SpawnDebris(float x, float y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
            float speed = DebrisSpeed * (0.3f + Random.Shared.NextSingle() * 0.7f);
            var d = new Debris(x, y, MathF.Cos(angle) * speed, MathF.Sin(angle) * speed);
            _debris.AddChild(d);
        }
    }

    private void SpawnAsteroids(int level)
    {
        int count = InitialAsteroidCount + (level - 1);
        for (int i = 0; i < count; i++)
        {
            // Spawn away from ship center
            float x, y;
            do
            {
                x = Random.Shared.NextSingle() * GameWidth;
                y = Random.Shared.NextSingle() * GameHeight;
            }
            while (MathF.Sqrt((x - GameWidth / 2f) * (x - GameWidth / 2f) +
                              (y - GameHeight / 2f) * (y - GameHeight / 2f)) < 150f);

            float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
            float speed = AsteroidLargeMinSpeed + Random.Shared.NextSingle() * (AsteroidLargeMaxSpeed - AsteroidLargeMinSpeed);
            float vx = MathF.Cos(angle) * speed;
            float vy = MathF.Sin(angle) * speed;
            var ast = new Asteroid(AsteroidSize.Large, x, y, vx, vy, Random.Shared.Next());
            _asteroids.AddChild(ast);
        }
    }

    private void CheckLevelComplete()
    {
        if (_endTriggered) return;

        var asteroidList = _asteroids.Children;
        bool anyActive = false;
        for (int i = 0; i < asteroidList.Count; i++)
        {
            if (asteroidList[i].Active) { anyActive = true; break; }
        }

        if (!anyActive && _shipAlive)
        {
            state.Level++;
            SpawnAsteroids(state.Level);
        }
    }

    private void TriggerGameOver()
    {
        if (_endTriggered) return;
        _endTriggered = true;
        coordinator.TransitionTo<GameOverScreen>(new DissolveTransition());
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        // Stars
        foreach (var star in _stars)
            canvas.DrawRect(star.X, star.Y, 1.5f, 1.5f, _starPaint);

        // Game entities
        _asteroids.Draw(canvas);
        _bullets.Draw(canvas);
        _debris.Draw(canvas);

        if (_shipAlive)
        {
            _ship.Draw(canvas);
        }

        // HUD
        _scoreText.Text = $"{state.Score:00000}";
        canvas.Save(); canvas.Translate(20f, 34f); _scoreText.Draw(canvas); canvas.Restore();

        DrawLivesIndicator(canvas);

        _levelText.Text = $"LEVEL {state.Level}";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 34f); _levelText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, GameHeight - 10f); _controlsText.Draw(canvas); canvas.Restore();

        DrawControlPad(canvas);
    }

    private void DrawLivesIndicator(SKCanvas canvas)
    {
        float startX = GameWidth - 30f;
        float y = 28f;

        using var path = new SKPath();
        float r = 8f;
        path.MoveTo(0, -r);
        path.LineTo(-r * 0.7f, r * 0.7f);
        path.LineTo(0, r * 0.35f);
        path.LineTo(r * 0.7f, r * 0.7f);
        path.Close();

        for (int i = 0; i < state.Lives; i++)
        {
            canvas.Save();
            canvas.Translate(startX - i * 22f, y);
            canvas.DrawPath(path, _livesShipPaint);
            canvas.Restore();
        }
    }

    private void DrawControlPad(SKCanvas canvas)
    {
        var appearance = (UiButtonAppearance)themes.Button with
        {
            CornerRadius = 8f,
            BorderWidth = 1.5f,
            BevelSize = 1.5f,
        };
        appearance.DrawDirect(canvas, LeftBtnRect, "<", _touchLeft, fontSize: 20f);
        appearance.DrawDirect(canvas, ThrustBtnRect, "^", _touchThrust, fontSize: 20f);
        appearance.DrawDirect(canvas, FireBtnRect, "FIRE", _touchFire, fontSize: 20f);
        appearance.DrawDirect(canvas, RightBtnRect, ">", _touchRight, fontSize: 20f);
    }

    private static void ClearChildren(Entity parent)
    {
        while (parent.ChildCount > 0)
            parent.RemoveChild(parent.Children[^1]);
    }
}
