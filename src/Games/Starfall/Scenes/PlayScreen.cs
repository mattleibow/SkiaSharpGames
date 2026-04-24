using SkiaSharp;
using SkiaSharp.Theatre;
using SkiaSharp.Theatre.Themes.Default;

using static SkiaSharpGames.Starfall.StarfallConstants;

namespace SkiaSharpGames.Starfall;

/// <summary>
/// Main gameplay scene. Manages waves of enemies, boss fights, collision,
/// power-ups, scoring, and transitions between stages.
/// </summary>
internal sealed class PlayScreen(StarfallGameState state, IDirector director) : Scene
{
    // ── Actor containers ─────────────────────────────────────────────────
    private readonly Starfield _starfield = new();
    private readonly PlayerShip _player = new();
    private readonly Actor _playerBullets = new() { Name = "playerBullets" };
    private readonly Actor _enemies = new() { Name = "enemies" };
    private readonly Actor _enemyBullets = new() { Name = "enemyBullets" };
    private readonly Actor _shockwaves = new() { Name = "shockwaves" };
    private readonly Actor _powerUps = new() { Name = "powerUps" };
    private readonly Actor _particles = new() { Name = "particles" };
    private readonly Actor _floatingTexts = new() { Name = "floatingTexts" };

    // ── HUD ──────────────────────────────────────────────────────────────
    private readonly HudLabel _scoreLabel = new()
    {
        FontSize = 22f, Color = SKColors.White, X = 15f, Y = 30f,
    };
    private readonly HudLabel _stageLabel = new()
    {
        FontSize = 16f, Color = HudDimColor, Align = TextAlign.Center,
        X = GameWidth / 2f, Y = 30f,
    };
    private readonly HudLabel _comboLabel = new()
    {
        FontSize = 16f, Color = YellowAccent, Align = TextAlign.Right,
        X = GameWidth - 15f, Y = 52f, Visible = false,
    };
    private readonly HudLabel _waveAnnounce = new()
    {
        FontSize = 40f, Color = CyanAccent, Align = TextAlign.Center,
        X = GameWidth / 2f, Y = GameHeight / 2f - 20f, Visible = false,
    };
    private readonly HudLabel _bossWarning = new()
    {
        Text = "⚠ WARNING — BOSS APPROACHING ⚠",
        FontSize = 28f, Color = RedAccent, Align = TextAlign.Center,
        X = GameWidth / 2f, Y = GameHeight / 2f, Visible = false,
    };

    // ── Wave system ──────────────────────────────────────────────────────
    private readonly List<WaveDefinition> _waves = [];
    private int _currentWaveIndex;
    private float _waveTimer;
    private int _spawnIndex;
    private float _spawnTimer;
    private bool _bossActive;
    private BossShip? _boss;
    private bool _stageComplete;
    private float _stageCompleteTimer;
    private float _announceTimer;

    // ── Combat state ─────────────────────────────────────────────────────
    private float _fireCooldown;
    private int _combo;
    private float _comboTimer;
    private float _bombFlashTimer;
    private float _screenShakeTimer;
    private float _screenShakeIntensity;
    private bool _gameOver;

    // ── Touch controls ───────────────────────────────────────────────────
    private bool _pointerActive;
    private static readonly SKRect BombBtnRect = SKRect.Create(GameWidth - 80f, GameHeight - 60f, 65f, 45f);

    // ── Boss HP bar (top of screen) ──────────────────────────────────────
    private float _bossHPDisplay;

    // ── Fog overlay for Stage 3 ──────────────────────────────────────────
    private float _fogTime;

    public override void OnActivating()
    {
        if (ChildCount == 0)
        {
            Children.Add(_starfield);
            Children.Add(_enemies);
            Children.Add(_shockwaves);
            Children.Add(_powerUps);
            Children.Add(_playerBullets);
            Children.Add(_enemyBullets);
            Children.Add(_player);
            Children.Add(_particles);
            Children.Add(_floatingTexts);
            Children.Add(_scoreLabel);
            Children.Add(_stageLabel);
            Children.Add(_comboLabel);
            Children.Add(_waveAnnounce);
            Children.Add(_bossWarning);
        }
    }

    public override void OnActivated()
    {
        _gameOver = false;
        _stageComplete = false;
        _stageCompleteTimer = 0;
        _fireCooldown = 0;
        _combo = 0;
        _comboTimer = 0;
        _bombFlashTimer = 0;
        _screenShakeTimer = 0;
        _bossActive = false;
        _boss = null;
        _bossHPDisplay = 0;
        _fogTime = 0;
        _announceTimer = 0;
        _pointerActive = false;

        ClearContainer(_playerBullets);
        ClearContainer(_enemies);
        ClearContainer(_enemyBullets);
        ClearContainer(_shockwaves);
        ClearContainer(_powerUps);
        ClearContainer(_particles);
        ClearContainer(_floatingTexts);

        _player.X = GameWidth / 2f;
        _player.Y = GameHeight * 0.8f;
        _player.TargetX = _player.X;
        _player.TargetY = _player.Y;
        _player.Active = true;
        _player.Visible = true;
        _player.InvincibleTimer = PlayerInvincibleDuration;
        _player.SpeedMultiplier = state.SpeedMultiplier;

        SetupStageWaves(state.CurrentStage);
        _currentWaveIndex = 0;
        _spawnIndex = 0;
        _spawnTimer = 0;
        _waveTimer = WaveDelay;

        _waveAnnounce.Visible = true;
        _waveAnnounce.Text = $"SECTOR {state.CurrentStage}";
        _announceTimer = 2f;
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerMove(float x, float y)
    {
        if (_pointerActive && !_gameOver)
        {
            _player.TargetX = x;
            _player.TargetY = y;
        }
    }

    public override void OnPointerDown(float x, float y)
    {
        if (_gameOver) return;

        // Check bomb button
        if (BombBtnRect.Contains(x, y))
        {
            ActivateBomb();
            return;
        }

        _pointerActive = true;
        _player.TargetX = x;
        _player.TargetY = y;
    }

    public override void OnPointerUp(float x, float y)
    {
        _pointerActive = false;
    }

    public override void OnKeyDown(string key)
    {
        if (_gameOver) return;

        switch (key)
        {
            case " " or "b" or "B":
                ActivateBomb();
                break;
            case "ArrowLeft" or "a" or "A":
                _player.TargetX = Math.Max(PlayerShipRadius, _player.TargetX - 40f);
                break;
            case "ArrowRight" or "d" or "D":
                _player.TargetX = Math.Min(GameWidth - PlayerShipRadius, _player.TargetX + 40f);
                break;
            case "ArrowUp" or "w" or "W":
                _player.TargetY = Math.Max(PlayerShipRadius * 2, _player.TargetY - 40f);
                break;
            case "ArrowDown" or "s" or "S":
                _player.TargetY = Math.Min(GameHeight - PlayerShipRadius, _player.TargetY + 40f);
                break;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────

    protected override void OnUpdate(float deltaTime)
    {
        if (_gameOver) return;

        // Timers
        _bombFlashTimer = MathF.Max(0, _bombFlashTimer - deltaTime);
        _screenShakeTimer = MathF.Max(0, _screenShakeTimer - deltaTime);
        _fogTime += deltaTime;

        // Power-up timers
        state.RapidFireTimer = MathF.Max(0, state.RapidFireTimer - deltaTime);
        state.SpreadShotTimer = MathF.Max(0, state.SpreadShotTimer - deltaTime);
        state.ShieldTimer = MathF.Max(0, state.ShieldTimer - deltaTime);

        // Combo decay
        if (_comboTimer > 0)
        {
            _comboTimer -= deltaTime;
            if (_comboTimer <= 0)
            {
                _combo = 0;
                _comboLabel.Visible = false;
            }
        }

        // Announcement timer
        if (_announceTimer > 0)
        {
            _announceTimer -= deltaTime;
            if (_announceTimer <= 0)
                _waveAnnounce.Visible = false;
        }

        // Auto-fire
        _fireCooldown -= deltaTime;
        if (_fireCooldown <= 0f && _player.Active)
        {
            _fireCooldown = state.EffectiveFireRate;
            FirePlayerBullet();
        }

        // Wave spawning
        UpdateWaveSpawning(deltaTime);

        // Update enemy targeting info
        UpdateEnemyTargets();

        // Collision
        CheckCollisions();

        // Boss HP display smooth animation
        if (_boss is { Active: true })
            _bossHPDisplay += (_boss.HPRatio - _bossHPDisplay) * 5f * deltaTime;

        // Stage completion
        if (_stageComplete)
        {
            _stageCompleteTimer -= deltaTime;
            if (_stageCompleteTimer <= 0f)
            {
                if (state.CurrentStage >= 3)
                    director.TransitionTo<VictoryScreen>(new FadeCurtain());
                else
                    director.TransitionTo<UpgradeScreen>(new DissolveCurtain());
            }
        }

        // Cleanup
        _playerBullets.Children.RemoveInactive();
        _enemies.Children.RemoveInactive();
        _enemyBullets.Children.RemoveInactive();
        _shockwaves.Children.RemoveInactive();
        _powerUps.Children.RemoveInactive();
        _particles.Children.RemoveInactive();
        _floatingTexts.Children.RemoveInactive();
    }

    private void FirePlayerBullet()
    {
        if (state.HasSpreadShot)
        {
            // Triple spread
            _playerBullets.Children.Add(new PlayerBullet(_player.X, _player.Y - PlayerShipRadius,
                0, -PlayerBulletSpeed, state.BulletDamage));
            _playerBullets.Children.Add(new PlayerBullet(_player.X, _player.Y - PlayerShipRadius,
                -PlayerBulletSpeed * 0.2f, -PlayerBulletSpeed * 0.98f, state.BulletDamage));
            _playerBullets.Children.Add(new PlayerBullet(_player.X, _player.Y - PlayerShipRadius,
                PlayerBulletSpeed * 0.2f, -PlayerBulletSpeed * 0.98f, state.BulletDamage));
        }
        else
        {
            _playerBullets.Children.Add(new PlayerBullet(_player.X, _player.Y - PlayerShipRadius,
                0, -PlayerBulletSpeed, state.BulletDamage));
        }
    }

    private void ActivateBomb()
    {
        if (state.Bombs <= 0) return;
        state.Bombs--;
        _bombFlashTimer = BombFlashDuration;
        _screenShakeTimer = 0.3f;
        _screenShakeIntensity = 8f;

        // Destroy all enemies and bullets on screen
        foreach (var child in _enemies.Children)
        {
            if (child is EnemyBase enemy && enemy.Active)
            {
                state.AddScore(enemy.ScoreValue);
                SpawnExplosion(enemy.X, enemy.Y, enemy.Radius);
                enemy.Active = false;
            }
        }
        foreach (var child in _enemyBullets.Children)
            child.Active = false;
        foreach (var child in _shockwaves.Children)
            child.Active = false;

        // Damage boss
        if (_boss is { Active: true })
        {
            _boss.TakeDamage(10);
            SpawnExplosion(_boss.X, _boss.Y, 30f);
        }
    }

    // ── Wave Management ──────────────────────────────────────────────────

    private void UpdateWaveSpawning(float deltaTime)
    {
        if (_stageComplete) return;

        // Wait between waves
        if (_waveTimer > 0)
        {
            _waveTimer -= deltaTime;
            return;
        }

        if (_currentWaveIndex >= _waves.Count)
        {
            // Check if all enemies are cleared
            bool anyActive = false;
            foreach (var child in _enemies.Children)
                if (child.Active) { anyActive = true; break; }

            if (!anyActive && !_bossActive)
            {
                // Stage complete!
                _stageComplete = true;
                _stageCompleteTimer = 2f;
                _waveAnnounce.Text = state.CurrentStage >= 3 ? "VICTORY!" : "SECTOR CLEARED!";
                _waveAnnounce.Color = GreenAccent;
                _waveAnnounce.Visible = true;
                _announceTimer = 2f;
            }
            return;
        }

        var wave = _waves[_currentWaveIndex];

        // Boss wave
        if (wave.IsBoss)
        {
            if (!_bossActive)
            {
                _bossActive = true;
                _bossWarning.Visible = true;
                _waveTimer = BossDelay;
                return;
            }

            if (_bossWarning.Visible)
            {
                _bossWarning.Visible = false;
                SpawnBoss(wave.BossType);
            }

            // Check if boss is dead
            if (_boss is { Active: false })
            {
                _bossActive = false;
                _boss = null;
                _currentWaveIndex++;
                _spawnIndex = 0;
                _waveTimer = WaveDelay;

                // Big explosion
                _screenShakeTimer = 0.5f;
                _screenShakeIntensity = 12f;
            }
            return;
        }

        // Normal wave: spawn enemies at intervals
        _spawnTimer -= deltaTime;
        if (_spawnTimer <= 0f && _spawnIndex < wave.Spawns.Count)
        {
            var spawn = wave.Spawns[_spawnIndex];
            SpawnEnemy(spawn);
            _spawnIndex++;
            _spawnTimer = wave.SpawnInterval;

            if (_spawnIndex >= wave.Spawns.Count)
            {
                _currentWaveIndex++;
                _spawnIndex = 0;
                _waveTimer = WaveDelay;

                // Announce next wave
                if (_currentWaveIndex < _waves.Count && !_waves[_currentWaveIndex].IsBoss)
                {
                    _waveAnnounce.Text = $"WAVE {_currentWaveIndex + 1}";
                    _waveAnnounce.Color = CyanAccent;
                    _waveAnnounce.Visible = true;
                    _announceTimer = 1.5f;
                }
            }
        }
    }

    private void SpawnEnemy(EnemySpawn spawn)
    {
        float x = spawn.X < 0 ? Random.Shared.NextSingle() * (GameWidth - 80f) + 40f : spawn.X;
        float y = spawn.Y;

        EnemyBase enemy = spawn.Type switch
        {
            EnemyType.Drone => new Drone(x, y, spawn.Speed > 0 ? spawn.Speed : DroneSpeed),
            EnemyType.Zigzagger => new Zigzagger(x, y),
            EnemyType.Shooter => CreateShooter(x, y),
            EnemyType.Bomber => CreateBomber(x, y),
            EnemyType.Charger => new Charger(x, y),
            EnemyType.Asteroid => new StarfallAsteroid(x, y,
                (Random.Shared.NextSingle() - 0.5f) * 60f,
                AsteroidMinSpeed + Random.Shared.NextSingle() * (AsteroidMaxSpeed - AsteroidMinSpeed)),
            _ => new Drone(x, y),
        };

        _enemies.Children.Add(enemy);
    }

    private Shooter CreateShooter(float x, float y)
    {
        var s = new Shooter(x, y);
        s.OnFireBullet = b => _enemyBullets.Children.Add(b);
        return s;
    }

    private Bomber CreateBomber(float x, float y)
    {
        var b = new Bomber(x, y);
        b.OnShockwave = (sx, sy) => _shockwaves.Children.Add(new Shockwave(sx, sy));
        return b;
    }

    private void SpawnBoss(BossType type)
    {
        _boss = new BossShip(type);
        _boss.OnFireBullet = b => _enemyBullets.Children.Add(b);
        _boss.OnShockwave = (sx, sy) => _shockwaves.Children.Add(new Shockwave(sx, sy));
        _enemies.Children.Add(_boss);
        _bossHPDisplay = 1f;
    }

    private void UpdateEnemyTargets()
    {
        foreach (var child in _enemies.Children)
        {
            if (!child.Active) continue;
            if (child is Shooter s) s.SetPlayerPosition(_player.X, _player.Y);
            if (child is Charger c) c.SetPlayerPosition(_player.X, _player.Y);
            if (child is BossShip b) b.SetPlayerPosition(_player.X, _player.Y);
        }

        // Update power-ups with player position for magnetism
        foreach (var child in _powerUps.Children)
        {
            if (child is PowerUp pu && pu.Active)
            {
                pu.PlayerX = _player.X;
                pu.PlayerY = _player.Y;
            }
        }
    }

    // ── Collision ─────────────────────────────────────────────────────────

    private void CheckCollisions()
    {
        if (!_player.Active) return;

        // Player bullets vs enemies
        var bullets = _playerBullets.Children;
        var enemies = _enemies.Children;

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            if (bullets[i] is not PlayerBullet bullet || !bullet.Active) continue;

            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                if (enemies[j] is not EnemyBase enemy || !enemy.Active) continue;

                if (bullet.Overlaps(enemy))
                {
                    bullet.Active = false;
                    bool destroyed = enemy.TakeDamage(bullet.Damage);
                    SpawnHitParticles(bullet.X, bullet.Y);

                    if (destroyed)
                    {
                        OnEnemyDestroyed(enemy);
                    }
                    break;
                }
            }
        }

        // Enemy bullets vs player
        if (!state.HasShield && !_player.IsInvincible)
        {
            var eBullets = _enemyBullets.Children;
            for (int i = eBullets.Count - 1; i >= 0; i--)
            {
                if (eBullets[i] is not EnemyBullet eb || !eb.Active) continue;
                if (_player.Overlaps(eb))
                {
                    eb.Active = false;
                    PlayerHit();
                    break;
                }
            }
        }

        // Shockwaves vs player
        if (!state.HasShield && !_player.IsInvincible)
        {
            foreach (var child in _shockwaves.Children)
            {
                if (child is not Shockwave sw || !sw.Active) continue;
                if (_player.Overlaps(sw))
                {
                    PlayerHit();
                    break;
                }
            }
        }

        // Enemies vs player (contact damage)
        if (!state.HasShield && !_player.IsInvincible)
        {
            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                if (enemies[j] is not EnemyBase enemy || !enemy.Active) continue;
                if (_player.Overlaps(enemy))
                {
                    PlayerHit();
                    if (enemy is not BossShip)
                    {
                        enemy.Active = false;
                        SpawnExplosion(enemy.X, enemy.Y, enemy.Radius);
                    }
                    break;
                }
            }
        }

        // Player vs power-ups
        var pups = _powerUps.Children;
        for (int i = pups.Count - 1; i >= 0; i--)
        {
            if (pups[i] is not PowerUp pu || !pu.Active) continue;
            if (_player.Overlaps(pu))
            {
                pu.Active = false;
                CollectPowerUp(pu);
            }
        }
    }

    private void OnEnemyDestroyed(EnemyBase enemy)
    {
        state.AddScore(enemy.ScoreValue);
        SpawnExplosion(enemy.X, enemy.Y, enemy.Radius);

        // Combo
        _combo++;
        _comboTimer = 2f;
        if (_combo > 1)
        {
            _comboLabel.Visible = true;
            _comboLabel.Text = $"x{_combo} COMBO";
            int bonusScore = _combo * 10;
            state.AddScore(bonusScore);
            _floatingTexts.Children.Add(new FloatingText(enemy.X, enemy.Y - 15f,
                $"+{enemy.ScoreValue + bonusScore}", YellowAccent));
        }
        else
        {
            _floatingTexts.Children.Add(new FloatingText(enemy.X, enemy.Y - 15f,
                $"+{enemy.ScoreValue}", SKColors.White));
        }

        // Power-up drop (weighted by what player needs)
        if (Random.Shared.NextSingle() < PowerUpDropChance && enemy is not BossShip)
        {
            var type = ChooseSmartPowerUp();
            _powerUps.Children.Add(new PowerUp(enemy.X, enemy.Y, type));
        }

        // Screen shake for bigger enemies
        if (enemy is BossShip or Bomber)
        {
            _screenShakeTimer = 0.3f;
            _screenShakeIntensity = 6f;
        }
    }

    private void PlayerHit()
    {
        state.HP--;
        _player.InvincibleTimer = PlayerInvincibleDuration;
        _screenShakeTimer = 0.2f;
        _screenShakeIntensity = 5f;
        SpawnHitParticles(_player.X, _player.Y);

        // Reset combo on hit
        _combo = 0;
        _comboTimer = 0;
        _comboLabel.Visible = false;

        if (state.HP <= 0)
        {
            _gameOver = true;
            SpawnExplosion(_player.X, _player.Y, 20f);
            _player.Active = false;
            _player.Visible = false;

            // Delay before game over screen
            _stageComplete = true;
            _stageCompleteTimer = 1.5f;
            _waveAnnounce.Text = "DESTROYED";
            _waveAnnounce.Color = RedAccent;
            _waveAnnounce.Visible = true;
            _announceTimer = 1.5f;
        }
    }

    private void CollectPowerUp(PowerUp pu)
    {
        string text;
        SKColor color;

        switch (pu.Type)
        {
            case PowerUpType.Health:
                state.HP = Math.Min(state.HP + 1, state.MaxHP);
                text = "HP +1";
                color = GreenAccent;
                break;
            case PowerUpType.RapidFire:
                state.RapidFireTimer = PowerUpDuration;
                text = "RAPID FIRE!";
                color = YellowAccent;
                break;
            case PowerUpType.SpreadShot:
                state.SpreadShotTimer = PowerUpDuration;
                text = "SPREAD SHOT!";
                color = CyanAccent;
                break;
            case PowerUpType.Bomb:
                state.Bombs = Math.Min(state.Bombs + 1, state.MaxBombs + 2);
                text = "BOMB +1";
                color = MagentaAccent;
                break;
            case PowerUpType.Shield:
                state.ShieldTimer = ShieldDuration;
                text = "SHIELD!";
                color = PowerUpShieldColor;
                break;
            default:
                return;
        }

        _floatingTexts.Children.Add(new FloatingText(pu.X, pu.Y - 10f, text, color));
    }

    // ── Particles & Effects ──────────────────────────────────────────────

    private PowerUpType ChooseSmartPowerUp()
    {
        // Weight power-ups by what the player actually needs
        var candidates = new List<(PowerUpType type, float weight)>();

        // Health: high weight when low HP, zero at full
        if (state.HP < state.MaxHP)
            candidates.Add((PowerUpType.Health, state.HP <= state.MaxHP / 2 ? 4f : 2f));

        // Rapid fire: always useful unless already active
        candidates.Add((PowerUpType.RapidFire, state.HasRapidFire ? 0.5f : 2f));

        // Spread shot: always useful
        candidates.Add((PowerUpType.SpreadShot, state.HasSpreadShot ? 0.5f : 2f));

        // Bomb: useful if not at cap
        if (state.Bombs < state.MaxBombs + 2)
            candidates.Add((PowerUpType.Bomb, 1.5f));

        // Shield: always useful
        candidates.Add((PowerUpType.Shield, state.HasShield ? 0.5f : 1.5f));

        // Fallback: if somehow no candidates, return RapidFire
        if (candidates.Count == 0)
            return PowerUpType.RapidFire;

        float totalWeight = 0;
        foreach (var c in candidates) totalWeight += c.weight;
        float roll = Random.Shared.NextSingle() * totalWeight;
        float cumulative = 0;
        foreach (var c in candidates)
        {
            cumulative += c.weight;
            if (roll < cumulative) return c.type;
        }
        return candidates[^1].type;
    }

    private void SpawnExplosion(float x, float y, float radius)
    {
        int count = radius > 20f ? ExplosionParticleCount : SmallExplosionParticleCount;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
            float speed = ParticleSpeed * (0.3f + Random.Shared.NextSingle() * 0.7f);
            float size = 2f + Random.Shared.NextSingle() * 3f;
            var color = i % 3 == 0 ? ExplosionColor : (i % 3 == 1 ? OrangeAccent : RedAccent);
            _particles.Children.Add(new Particle(x, y,
                MathF.Cos(angle) * speed, MathF.Sin(angle) * speed,
                color, ParticleLifetime, size));
        }
    }

    private void SpawnHitParticles(float x, float y)
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
            float speed = 80f + Random.Shared.NextSingle() * 60f;
            _particles.Children.Add(new Particle(x, y,
                MathF.Cos(angle) * speed, MathF.Sin(angle) * speed,
                CyanAccent, 0.3f, 2f));
        }
    }

    // ── Draw ──────────────────────────────────────────────────────────────
    // Override Draw to control render ordering: gameplay → effects → HUD.
    // SceneNode.Draw calls OnDraw then children, but we need overlays
    // (fog, shield, bomb flash) between gameplay actors and HUD labels.

    public override void Draw(SKCanvas canvas)
    {
        if (!Active) return;

        canvas.Clear(BackgroundColor);

        // Screen shake wraps gameplay actors only
        bool shaking = _screenShakeTimer > 0;
        if (shaking)
        {
            float shakeFactor = MathF.Min(_screenShakeTimer / 0.3f, 1f);
            float dx = (Random.Shared.NextSingle() - 0.5f) * 2f * _screenShakeIntensity * shakeFactor;
            float dy = (Random.Shared.NextSingle() - 0.5f) * 2f * _screenShakeIntensity * shakeFactor;
            canvas.Save();
            canvas.Translate(dx, dy);
        }

        // Draw gameplay actors in order
        _starfield.Draw(canvas);
        _enemies.Draw(canvas);
        _shockwaves.Draw(canvas);
        _powerUps.Draw(canvas);
        _playerBullets.Draw(canvas);
        _enemyBullets.Draw(canvas);
        _player.Draw(canvas);
        _particles.Draw(canvas);
        _floatingTexts.Draw(canvas);

        // Shield bubble around player (drawn over player)
        if (state.HasShield && _player.Active)
        {
            float pulse = 0.6f + 0.4f * MathF.Sin(_fogTime * 8f);
            using var shieldPaint = new SKPaint
            {
                Color = PlayerShieldColor.WithAlpha((byte)(100 * pulse)),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
            };
            canvas.DrawCircle(_player.X, _player.Y, PlayerShipRadius + 8f, shieldPaint);
            shieldPaint.Style = SKPaintStyle.Fill;
            shieldPaint.Color = PlayerShieldColor.WithAlpha((byte)(30 * pulse));
            canvas.DrawCircle(_player.X, _player.Y, PlayerShipRadius + 8f, shieldPaint);
        }

        // Bomb flash (over gameplay)
        if (_bombFlashTimer > 0)
        {
            float alpha = _bombFlashTimer / BombFlashDuration;
            using var flashPaint = new SKPaint { Color = BombFlashColor.WithAlpha((byte)(200 * alpha)) };
            canvas.DrawRect(0, 0, GameWidth, GameHeight, flashPaint);
        }

        if (shaking)
            canvas.Restore();

        // Fog overlay (over gameplay, under HUD)
        if (state.CurrentStage >= 3)
            DrawFogOverlay(canvas);

        // HUD (drawn last, outside screen shake)
        _scoreLabel.Draw(canvas);
        _stageLabel.Draw(canvas);
        if (_comboLabel.Visible) _comboLabel.Draw(canvas);
        if (_waveAnnounce.Visible) _waveAnnounce.Draw(canvas);
        if (_bossWarning.Visible) _bossWarning.Draw(canvas);
        DrawHUD(canvas);
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        // Intentionally empty — all rendering handled in Draw override
    }

    private void DrawFogOverlay(SKCanvas canvas)
    {
        // Dark overlay with a spotlight around the player
        float breathe = 0.85f + 0.15f * MathF.Sin(_fogTime * 0.8f);
        float spotlightR = 120f * breathe;

        using var fogPaint = new SKPaint { IsAntialias = true };
        fogPaint.Shader = SKShader.CreateRadialGradient(
            new SKPoint(_player.X, _player.Y),
            spotlightR,
            [SKColors.Transparent, BackgroundColor.WithAlpha(180)],
            [0.4f, 1f],
            SKShaderTileMode.Clamp);
        canvas.DrawRect(0, 0, GameWidth, GameHeight, fogPaint);
    }

    private void DrawHUD(SKCanvas canvas)
    {
        _scoreLabel.Text = $"{state.Score:N0}";

        string stageName = state.CurrentStage switch
        {
            1 => "ASTEROID BELT",
            2 => "PIRATE TERRITORY",
            _ => "THE VOID",
        };
        _stageLabel.Text = $"SECTOR {state.CurrentStage} — {stageName}";

        // HP icons
        DrawHPIcons(canvas);

        // Bomb indicator
        DrawBombIndicator(canvas);

        // Boss HP bar
        if (_bossActive && _boss is { Active: true })
            DrawBossHPBar(canvas);

        // Active power-up indicators
        DrawPowerUpTimers(canvas);
    }

    private void DrawHPIcons(SKCanvas canvas)
    {
        float startX = GameWidth - 20f;
        float y = 28f;
        using var paint = new SKPaint { IsAntialias = true };

        for (int i = 0; i < state.MaxHP; i++)
        {
            float x = startX - i * 18f;
            bool filled = i < state.HP;
            paint.Color = filled ? GreenAccent : new SKColor(0x33, 0x44, 0x33);
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawCircle(x, y, 6f, paint);

            if (filled)
            {
                paint.Color = GreenAccent.WithAlpha(60);
                paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f);
                canvas.DrawCircle(x, y, 8f, paint);
                paint.MaskFilter = null;
            }
        }
    }

    private void DrawBombIndicator(SKCanvas canvas)
    {
        // Bomb button area
        var appearance = (
            (ResolvedHudTheme?.Button ?? DefaultButtonAppearance.Default) as DefaultButtonAppearance
            ?? DefaultButtonAppearance.Default
        ) with
        {
            CornerRadius = 6f,
            BorderWidth = 1.5f,
            BevelSize = 1.5f,
        };

        string bombText = state.Bombs > 0 ? $"💣{state.Bombs}" : "---";
        appearance.DrawDirect(canvas, BombBtnRect, bombText, false, fontSize: 16f);
    }

    private void DrawBossHPBar(SKCanvas canvas)
    {
        float barW = BossBarWidth;
        float barH = BossBarHeight;
        float barX = (GameWidth - barW) / 2f;
        float barY = BossBarY + 35f;

        using var bgPaint = new SKPaint { Color = new SKColor(0x22, 0x22, 0x33) };
        using var fgPaint = new SKPaint();

        canvas.DrawRoundRect(barX, barY, barW, barH, 3f, 3f, bgPaint);

        SKColor barColor = _bossHPDisplay > 0.5f ? BossColor
            : _bossHPDisplay > 0.25f ? OrangeAccent
            : RedAccent;

        fgPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(barX, barY),
            new SKPoint(barX + barW * _bossHPDisplay, barY),
            [barColor, barColor.WithAlpha(180)],
            SKShaderTileMode.Clamp);

        canvas.DrawRoundRect(barX, barY, barW * _bossHPDisplay, barH, 3f, 3f, fgPaint);

        // Boss name
        string bossName = _boss switch
        {
            { } b when b.HPRatio > 0 => state.CurrentStage switch
            {
                1 => "THE COLOSSUS",
                2 => "THE MARAUDER",
                _ => "THE DREADNOUGHT",
            },
            _ => "",
        };
        using var textPaint = new SKPaint { Color = BossColor, IsAntialias = true };
        var font = new SKFont { Size = 14f };
        float textW = font.MeasureText(bossName);
        canvas.DrawText(bossName, (GameWidth - textW) / 2f, barY - 4f, font, textPaint);
    }

    private void DrawPowerUpTimers(SKCanvas canvas)
    {
        float y = 55f;
        float x = 15f;
        using var paint = new SKPaint { IsAntialias = true };
        var font = new SKFont { Size = 13f };

        if (state.HasRapidFire)
        {
            paint.Color = YellowAccent;
            canvas.DrawText($"RAPID {state.RapidFireTimer:F1}s", x, y, font, paint);
            y += 16f;
        }
        if (state.HasSpreadShot)
        {
            paint.Color = CyanAccent;
            canvas.DrawText($"SPREAD {state.SpreadShotTimer:F1}s", x, y, font, paint);
            y += 16f;
        }
        if (state.HasShield)
        {
            paint.Color = PowerUpShieldColor;
            canvas.DrawText($"SHIELD {state.ShieldTimer:F1}s", x, y, font, paint);
        }
    }

    // ── Wave Definitions ─────────────────────────────────────────────────

    private void SetupStageWaves(int stage)
    {
        _waves.Clear();

        switch (stage)
        {
            case 1: SetupStage1(); break;
            case 2: SetupStage2(); break;
            case 3: SetupStage3(); break;
        }
    }

    private void SetupStage1()
    {
        // Wave 1: Small drone group
        _waves.Add(CreateWave(0.4f,
            Spawn(EnemyType.Drone, 200), Spawn(EnemyType.Drone, 400), Spawn(EnemyType.Drone, 600)));

        // Wave 2: Asteroids + drones
        _waves.Add(CreateWave(0.5f,
            Spawn(EnemyType.Asteroid), Spawn(EnemyType.Drone, 300),
            Spawn(EnemyType.Asteroid), Spawn(EnemyType.Drone, 500)));

        // Wave 3: Zigzaggers debut
        _waves.Add(CreateWave(0.5f,
            Spawn(EnemyType.Zigzagger, 200), Spawn(EnemyType.Zigzagger, 600),
            Spawn(EnemyType.Drone, 400)));

        // Wave 4: Mixed
        _waves.Add(CreateWave(0.4f,
            Spawn(EnemyType.Drone, 150), Spawn(EnemyType.Zigzagger, 350),
            Spawn(EnemyType.Drone, 550), Spawn(EnemyType.Zigzagger, 650),
            Spawn(EnemyType.Asteroid)));

        // Wave 5: Dense drones + asteroids
        _waves.Add(CreateWave(0.3f,
            Spawn(EnemyType.Drone), Spawn(EnemyType.Drone), Spawn(EnemyType.Drone),
            Spawn(EnemyType.Asteroid), Spawn(EnemyType.Asteroid),
            Spawn(EnemyType.Zigzagger), Spawn(EnemyType.Zigzagger)));

        // Boss
        _waves.Add(WaveDefinition.BossWave(BossType.Colossus));
    }

    private void SetupStage2()
    {
        // Wave 1: Shooters debut
        _waves.Add(CreateWave(0.5f,
            Spawn(EnemyType.Drone, 200), Spawn(EnemyType.Shooter, 400), Spawn(EnemyType.Drone, 600)));

        // Wave 2: Zigzaggers + shooters
        _waves.Add(CreateWave(0.4f,
            Spawn(EnemyType.Zigzagger, 150), Spawn(EnemyType.Shooter, 400),
            Spawn(EnemyType.Zigzagger, 650), Spawn(EnemyType.Shooter, 250)));

        // Wave 3: Bomber debut + escort
        _waves.Add(CreateWave(0.5f,
            Spawn(EnemyType.Drone, 200), Spawn(EnemyType.Bomber, 400),
            Spawn(EnemyType.Drone, 600), Spawn(EnemyType.Zigzagger, 300)));

        // Wave 4: Heavy assault
        _waves.Add(CreateWave(0.35f,
            Spawn(EnemyType.Shooter, 150), Spawn(EnemyType.Shooter, 650),
            Spawn(EnemyType.Bomber, 400), Spawn(EnemyType.Zigzagger, 300),
            Spawn(EnemyType.Zigzagger, 500), Spawn(EnemyType.Drone)));

        // Wave 5: Swarm
        _waves.Add(CreateWave(0.25f,
            Spawn(EnemyType.Drone), Spawn(EnemyType.Drone), Spawn(EnemyType.Drone),
            Spawn(EnemyType.Drone), Spawn(EnemyType.Shooter, 400),
            Spawn(EnemyType.Zigzagger), Spawn(EnemyType.Zigzagger),
            Spawn(EnemyType.Bomber, 400)));

        // Boss
        _waves.Add(WaveDefinition.BossWave(BossType.Marauder));
    }

    private void SetupStage3()
    {
        // Wave 1: Chargers debut
        _waves.Add(CreateWave(0.5f,
            Spawn(EnemyType.Charger, 200), Spawn(EnemyType.Charger, 600),
            Spawn(EnemyType.Drone, 400)));

        // Wave 2: Full mix
        _waves.Add(CreateWave(0.4f,
            Spawn(EnemyType.Shooter, 150), Spawn(EnemyType.Charger, 400),
            Spawn(EnemyType.Zigzagger, 650), Spawn(EnemyType.Bomber, 300)));

        // Wave 3: Heavy charger assault
        _waves.Add(CreateWave(0.35f,
            Spawn(EnemyType.Charger, 100), Spawn(EnemyType.Charger, 300),
            Spawn(EnemyType.Charger, 500), Spawn(EnemyType.Charger, 700),
            Spawn(EnemyType.Shooter, 400)));

        // Wave 4: Mega swarm
        _waves.Add(CreateWave(0.2f,
            Spawn(EnemyType.Drone), Spawn(EnemyType.Drone), Spawn(EnemyType.Drone),
            Spawn(EnemyType.Zigzagger), Spawn(EnemyType.Zigzagger),
            Spawn(EnemyType.Shooter), Spawn(EnemyType.Shooter),
            Spawn(EnemyType.Bomber), Spawn(EnemyType.Charger), Spawn(EnemyType.Charger)));

        // Wave 5: Everything + asteroids
        _waves.Add(CreateWave(0.25f,
            Spawn(EnemyType.Charger), Spawn(EnemyType.Bomber), Spawn(EnemyType.Bomber),
            Spawn(EnemyType.Shooter), Spawn(EnemyType.Shooter),
            Spawn(EnemyType.Asteroid), Spawn(EnemyType.Asteroid),
            Spawn(EnemyType.Zigzagger), Spawn(EnemyType.Drone), Spawn(EnemyType.Drone),
            Spawn(EnemyType.Charger)));

        // Final Boss
        _waves.Add(WaveDefinition.BossWave(BossType.Dreadnought));
    }

    private static EnemySpawn Spawn(EnemyType type, float x = -1, float y = -30f, float speed = 0)
        => new(type, x, y, speed);

    private static WaveDefinition CreateWave(float interval, params EnemySpawn[] spawns)
        => new([.. spawns], interval);

    private static void ClearContainer(Actor parent)
    {
        while (parent.ChildCount > 0)
            parent.Children.Remove(parent.Children[^1]);
    }
}

// ── Wave data structures ─────────────────────────────────────────────────

internal enum EnemyType { Drone, Zigzagger, Shooter, Bomber, Charger, Asteroid }

internal record struct EnemySpawn(EnemyType Type, float X, float Y, float Speed = 0);

internal sealed class WaveDefinition
{
    public List<EnemySpawn> Spawns { get; init; } = [];
    public float SpawnInterval { get; init; } = 0.5f;
    public bool IsBoss { get; init; }
    public BossType BossType { get; init; }

    public WaveDefinition(List<EnemySpawn> spawns, float interval)
    {
        Spawns = spawns;
        SpawnInterval = interval;
    }

    public static WaveDefinition BossWave(BossType type) => new([], 0)
    {
        IsBoss = true,
        BossType = type,
    };
}
