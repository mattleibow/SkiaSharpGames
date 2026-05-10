using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.GhostLight.GhostLightConstants;

namespace SkiaSharpGames.GhostLight;

/// <summary>
/// Main gameplay scene showcasing cascading alpha and the scene tree.
///
/// Scene tree:
///   PlayScreen
///   ├── Actor "world" (Alpha 0.65) — darkness SaveLayer, everything game-related inside
///   │   ├── FogLayer "fog" (draws 4 large white circles, each with alpha 0.08-0.12)
///   │   ├── Actor "enemies" — container for ShadowBlobs
///   │   │   └── ShadowBlob children (chase player, alpha based on proximity)
///   │   └── Spirit "spirit" (core circle + radial gradient glow)
///   ├── HudLabel "score" (OUTSIDE world — fully visible)
///   ├── HudLabel "time"
///   ├── HudLabel "instructions"
///   └── Actor "pauseOverlay" (Alpha 0 normally, 0.7 when paused)
///       └── HudLabel "paused"
/// </summary>
internal sealed class PlayScreen(GhostLightState state, IDirector director) : Scene
{
    // World container IS the darkness (Alpha 0.65)
    private readonly Actor _world = new() { Name = "world", Alpha = DarknessAlpha };
    private readonly FogLayer _fog = new();
    private readonly Actor _enemies = new() { Name = "enemies" };
    private readonly Spirit _spirit = new() { X = GameWidth / 2f, Y = GameHeight / 2f };

    // HUD (outside world — always bright)
    private readonly HudLabel _scoreLabel = new()
    {
        Name = "score",
        Text = "Score: 0",
        FontSize = 22f,
        Color = SKColors.White,
        X = 20f,
        Y = 30f,
    };
    private readonly HudLabel _timeLabel = new()
    {
        Name = "time",
        FontSize = 18f,
        Color = new SKColor(0x88, 0xAA, 0xCC),
        Align = TextAlign.Right,
        X = GameWidth - 20f,
        Y = 30f,
    };
    private readonly HudLabel _instructions = new()
    {
        Text = "WASD / Arrows to move \u2022 Avoid the shadows",
        FontSize = 14f,
        Color = new SKColor(0x66, 0x88, 0xAA),
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = GameHeight - 20f,
    };

    // Pause overlay
    private readonly Actor _pauseOverlay = new() { Name = "pauseOverlay", Alpha = 0f };
    private readonly HudLabel _pauseText = new()
    {
        Text = "PAUSED",
        FontSize = 48f,
        Color = SKColors.White,
        Align = TextAlign.Center,
        X = GameWidth / 2f,
        Y = GameHeight / 2f,
    };

    // Game state
    private float _spawnTimer;
    private bool _leftHeld,
        _rightHeld,
        _upHeld,
        _downHeld;
    private bool _started;

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            // Build tree: world contains fog + enemies + player
            _world.Children.Add(_fog);
            _world.Children.Add(_enemies);
            _world.Children.Add(_spirit);

            // Pause overlay has pause text
            _pauseOverlay.Children.Add(_pauseText);

            // Scene children: world (dimmed) + HUD (bright) + pause overlay
            Children.Add(_world);
            Children.Add(_scoreLabel);
            Children.Add(_timeLabel);
            Children.Add(_instructions);
            Children.Add(_pauseOverlay);
        }

        // Reset
        state.TimeSurvived = 0f;
        state.IsGameOver = false;
        _spirit.X = GameWidth / 2f;
        _spirit.Y = GameHeight / 2f;
        _started = false;
        _spawnTimer = 0f;

        // Clear enemies
        foreach (var e in _enemies.Children.ToArray())
            _enemies.Children.Remove(e);

        // Spawn initial enemies
        for (int i = 0; i < 4; i++)
            SpawnEnemy();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (!_started)
            return;

        state.TimeSurvived += deltaTime;

        // Player movement
        float speed = PlayerSpeed;
        if (_upHeld)
            _spirit.Y -= speed * deltaTime;
        if (_downHeld)
            _spirit.Y += speed * deltaTime;
        if (_leftHeld)
            _spirit.X -= speed * deltaTime;
        if (_rightHeld)
            _spirit.X += speed * deltaTime;

        // Clamp to bounds
        _spirit.X = Math.Clamp(_spirit.X, PlayerRadius, GameWidth - PlayerRadius);
        _spirit.Y = Math.Clamp(_spirit.Y, PlayerRadius, GameHeight - PlayerRadius);

        // Spawn enemies
        _spawnTimer += deltaTime;
        if (_spawnTimer >= EnemySpawnInterval && _enemies.ChildCount < MaxEnemies)
        {
            SpawnEnemy();
            _spawnTimer = 0f;
        }

        // Collision with enemies
        foreach (var child in _enemies.Children)
        {
            if (child is ShadowBlob blob && blob.Active && _spirit.Overlaps(blob))
            {
                state.IsGameOver = true;
                director.PushScene<GameOverScreen>();
                return;
            }
        }

        // HUD
        _scoreLabel.Text = $"Score: {(int)(state.TimeSurvived * 10)}";
        _timeLabel.Text = $"Time: {state.TimeSurvived:F1}s";
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(SKColors.Black);
    }

    // Input: start on first press
    public override void OnPointerDown(float x, float y) => _started = true;

    public override void OnKeyDown(string key)
    {
        _started = true;
        switch (key)
        {
            case "ArrowUp" or "w":
                _upHeld = true;
                break;
            case "ArrowDown" or "s":
                _downHeld = true;
                break;
            case "ArrowLeft" or "a":
                _leftHeld = true;
                break;
            case "ArrowRight" or "d":
                _rightHeld = true;
                break;
            case "F1":
                _fog.Visible = !_fog.Visible;
                break;
            case "F2":
                _world.Alpha = _world.Alpha > 0.1f ? 1f : DarknessAlpha;
                break;
            case "F3":
                foreach (var e in _enemies.Children)
                    if (e is ShadowBlob b)
                        b.Alpha = b.Alpha > 0f ? 0f : 0.5f;
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowUp" or "w":
                _upHeld = false;
                break;
            case "ArrowDown" or "s":
                _downHeld = false;
                break;
            case "ArrowLeft" or "a":
                _leftHeld = false;
                break;
            case "ArrowRight" or "d":
                _rightHeld = false;
                break;
        }
    }

    private void SpawnEnemy()
    {
        // Spawn at random edge
        float x,
            y;
        if (Random.Shared.Next(2) == 0)
        {
            x = Random.Shared.Next(2) == 0 ? -30f : GameWidth + 30f;
            y = Random.Shared.NextSingle() * GameHeight;
        }
        else
        {
            x = Random.Shared.NextSingle() * GameWidth;
            y = Random.Shared.Next(2) == 0 ? -30f : GameHeight + 30f;
        }

        float radius =
            EnemyMinRadius + Random.Shared.NextSingle() * (EnemyMaxRadius - EnemyMinRadius);
        var blob = new ShadowBlob(radius)
        {
            X = x,
            Y = y,
            Target = _spirit,
        };
        _enemies.Children.Add(blob);
    }
}
