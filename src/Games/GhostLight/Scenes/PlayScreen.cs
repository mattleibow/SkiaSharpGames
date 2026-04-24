using SkiaSharp;
using SkiaSharp.Theatre;

using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>
/// Main gameplay scene showcasing cascading alpha and the scene tree.
///
/// Scene tree:
///   PlayScreen
///   ├── Actor "darkness" (Alpha 0.7) — SaveLayer cascades to children
///   │   ├── FogLayer "fog1" (Alpha 0.4) — nested layer inside darkness
///   │   │   ├── ShadowBlob enemies (Alpha 0.6-0.9, animated)
///   │   │   └── Spirit "spirit" (Alpha 1.0, glowing orb)
///   │   └── FogLayer "fog2" (Alpha 0.3) — second fog layer, parallax
///   │       └── FogParticle ambient particles
///   ├── HudLabel "score" (outside darkness — always fully visible)
///   ├── HudLabel "instructions"
///   └── Actor "pauseOverlay" (Alpha 0, becomes 0.7 when paused)
///       └── HudLabel "paused text"
/// </summary>
internal sealed class PlayScreen(GhostLightState state, IDirector director) : Scene
{
    private static readonly Random Rng = new(42);

    // ── Scene tree structure ──────────────────────────────────────────────

    // Darkness container — everything inside has cascading 70% opacity
    private readonly Actor _darkness = new() { Name = "darkness", Alpha = 0.7f };

    // Primary fog layer (Alpha 0.4) — holds enemies + player
    private readonly FogLayer _fog1 = new(0.4f) { Name = "fog1" };

    // Secondary fog layer (Alpha 0.3) — parallax ambient fog
    private readonly FogLayer _fog2 = new(0.3f, breathPhase: 2f) { Name = "fog2" };

    // Player
    private readonly Spirit _spirit = new() { X = GameWidth / 2f, Y = GameHeight / 2f };

    // HUD (outside darkness — fully visible)
    private readonly HudLabel _scoreLabel = new()
    {
        Name = "score",
        Text = "Time: 0.0",
        FontSize = 24f,
        Color = SKColors.White,
        X = 20f,
        Y = 35f,
    };
    private readonly HudLabel _instructionsLabel = new()
    {
        Name = "instructions",
        Text = "Arrow keys / WASD to move — avoid the shadows!",
        FontSize = 16f,
        Color = new SKColor(0x88, 0x99, 0xAA),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = GameHeight - 20f,
    };

    // Pause overlay
    private readonly Actor _pauseOverlay = new() { Name = "pauseOverlay", Alpha = 0f };
    private readonly HudLabel _pausedText = new()
    {
        Name = "paused text",
        Text = "PAUSED",
        FontSize = 48f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = GameHeight / 2f,
    };

    // ── State ──────────────────────────────────────────────────────────────

    private float _spawnTimer;
    private bool _leftHeld,
        _rightHeld,
        _upHeld,
        _downHeld;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    public override void OnActivating()
    {
        state.TimeSurvived = 0f;
        state.IsGameOver = false;
        _spawnTimer = 0f;

        if (ChildCount == 0)
        {
            // Build the scene tree
            _fog1.Children.Add(_spirit);
            _darkness.Children.Add(_fog1);
            _darkness.Children.Add(_fog2);

            _pauseOverlay.Children.Add(_pausedText);

            Children.Add(_darkness);
            Children.Add(_scoreLabel);
            Children.Add(_instructionsLabel);
            Children.Add(_pauseOverlay);

            // Seed some ambient fog particles in fog2
            for (int i = 0; i < 8; i++)
            {
                float radius = 20f + Rng.NextSingle() * 40f;
                float vx = (Rng.NextSingle() - 0.5f) * 15f;
                float vy = (Rng.NextSingle() - 0.5f) * 10f;
                var particle = new FogParticle(radius, vx, vy)
                {
                    X = Rng.NextSingle() * GameWidth,
                    Y = Rng.NextSingle() * GameHeight,
                };
                _fog2.Children.Add(particle);
            }
        }
        else
        {
            // Reset for replay
            _spirit.X = GameWidth / 2f;
            _spirit.Y = GameHeight / 2f;

            // Remove old enemies
            foreach (var child in _fog1.Children.ToArray())
            {
                if (child is ShadowBlob)
                    _fog1.Children.Remove(child);
            }
        }
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft" or "a":
                _leftHeld = true;
                break;
            case "ArrowRight" or "d":
                _rightHeld = true;
                break;
            case "ArrowUp" or "w":
                _upHeld = true;
                break;
            case "ArrowDown" or "s":
                _downHeld = true;
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft" or "a":
                _leftHeld = false;
                break;
            case "ArrowRight" or "d":
                _rightHeld = false;
                break;
            case "ArrowUp" or "w":
                _upHeld = false;
                break;
            case "ArrowDown" or "s":
                _downHeld = false;
                break;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────

    protected override void OnUpdate(float deltaTime)
    {
        if (state.IsGameOver)
            return;

        // Score
        state.TimeSurvived += deltaTime;
        _scoreLabel.Text = $"Time: {state.TimeSurvived:F1}";

        // Player movement
        float dx = 0f,
            dy = 0f;
        if (_leftHeld)
            dx -= 1f;
        if (_rightHeld)
            dx += 1f;
        if (_upHeld)
            dy -= 1f;
        if (_downHeld)
            dy += 1f;

        if (dx != 0f || dy != 0f)
        {
            float len = MathF.Sqrt(dx * dx + dy * dy);
            dx /= len;
            dy /= len;
            _spirit.X = Math.Clamp(
                _spirit.X + dx * PlayerSpeed * deltaTime,
                PlayerRadius,
                GameWidth - PlayerRadius
            );
            _spirit.Y = Math.Clamp(
                _spirit.Y + dy * PlayerSpeed * deltaTime,
                PlayerRadius,
                GameHeight - PlayerRadius
            );
        }

        // Spawn enemies
        _spawnTimer += deltaTime;
        int enemyCount = _fog1.Children.Count(c => c is ShadowBlob);
        if (_spawnTimer >= EnemySpawnInterval && enemyCount < MaxEnemies)
        {
            _spawnTimer = 0f;
            SpawnEnemy();
        }

        // Collision detection — check each enemy
        foreach (var child in _fog1.Children)
        {
            if (child is ShadowBlob blob && blob.Active && blob.Alpha > 0.3f)
            {
                if (_spirit.Overlaps(blob))
                {
                    state.IsGameOver = true;
                    director.PushScene<GameOverScreen>();
                    return;
                }
            }
        }

        // Hide instructions after a few seconds
        if (state.TimeSurvived > 5f)
            _instructionsLabel.Visible = false;
    }

    private void SpawnEnemy()
    {
        float radius = EnemyMinRadius + Rng.NextSingle() * (EnemyMaxRadius - EnemyMinRadius);
        float speed = EnemyMinSpeed + Rng.NextSingle() * (EnemyMaxSpeed - EnemyMinSpeed);

        // Spawn from a random edge
        float x,
            y,
            vx,
            vy;
        int edge = Rng.Next(4);
        switch (edge)
        {
            case 0: // top
                x = Rng.NextSingle() * GameWidth;
                y = -radius;
                vx = (Rng.NextSingle() - 0.5f) * speed;
                vy = Rng.NextSingle() * speed * 0.5f + speed * 0.3f;
                break;
            case 1: // bottom
                x = Rng.NextSingle() * GameWidth;
                y = GameHeight + radius;
                vx = (Rng.NextSingle() - 0.5f) * speed;
                vy = -(Rng.NextSingle() * speed * 0.5f + speed * 0.3f);
                break;
            case 2: // left
                x = -radius;
                y = Rng.NextSingle() * GameHeight;
                vx = Rng.NextSingle() * speed * 0.5f + speed * 0.3f;
                vy = (Rng.NextSingle() - 0.5f) * speed;
                break;
            default: // right
                x = GameWidth + radius;
                y = Rng.NextSingle() * GameHeight;
                vx = -(Rng.NextSingle() * speed * 0.5f + speed * 0.3f);
                vy = (Rng.NextSingle() - 0.5f) * speed;
                break;
        }

        var blob = new ShadowBlob(radius, vx, vy) { X = x, Y = y };
        _fog1.Children.Add(blob);
    }

    // ── Draw ──────────────────────────────────────────────────────────────

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(new SKColor(0x08, 0x06, 0x12));
    }
}
