using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.CastleAttack;

/// <summary>Active gameplay screen for Castle Attack.</summary>
internal sealed class PlayScreen(CastleAttackGameState state, IScreenCoordinator coordinator) : GameScreen
{
    // ── State ─────────────────────────────────────────────────────────────
    private readonly List<Wall> _walls = [];
    private readonly List<Enemy> _enemies = [];
    private readonly List<Arrow> _arrows = [];
    private readonly List<Boulder> _boulders = [];
    private readonly List<FloatText> _texts = [];

    private int _archerCount = 3;
    private int _workerCount = 3;
    private float _keepProgress;
    private float _lordHP = 10f;
    private bool _lordActive;
    private float _lordAttackTimer;

    private float _aimAngle = AimDefault;
    private bool _touchAimLeft, _touchAimRight;
    private CountdownTimer _arrowCooldown;

    private bool _oilAvail = true;
    private bool _mangonelAvail = true;
    private bool _logsAvail = true;

    private bool _leftHeld, _rightHeld;

    private float _spawnTimer = 3.5f;
    private float _spawnInterval = 5f;
    private float _levelTime;

    private bool _cowSpawned;
    private float _cowSpawnTime = 25f;

    private int _accuracyMult = 1;
    private int _consecutiveHits;
    private CountdownTimer _accuracyResetTimer;

    private CountdownTimer[] _wallFlash = new CountdownTimer[3];

    // ── Sprites ───────────────────────────────────────────────────────────
    private readonly ArcherSprite _archerSprite = new();
    private readonly WorkerSprite _workerSprite = new();
    private readonly LordSprite _lordSprite = new();
    private readonly WallBlockSprite _wallBlockSprite = new();
    private readonly ButtonSprite _buttonSprite = new();

    // ── HUD text sprites ─────────────────────────────────────────────────
    private readonly TextSprite _scoreText = new() { Size = 20f, Color = ColHud };
    private readonly TextSprite _levelText = new() { Size = 20f, Color = ColHud, Align = TextAlign.Right };
    private readonly TextSprite _archerText = new() { Size = 16f, Color = ColArcher };
    private readonly TextSprite _workerText = new() { Size = 16f, Color = ColWorker };
    private readonly TextSprite _keepLabel = new() { Text = "Keep", Size = 11f, Color = ColDim };
    private readonly TextSprite _aimText = new() { Size = 15f, Color = ColDim, Align = TextAlign.Center };
    private readonly TextSprite _cooldownDot = new() { Text = "●", Size = 12f, Align = TextAlign.Center };
    private readonly TextSprite _lordHpText = new() { Size = 16f };
    private readonly TextSprite _accuracyText = new() { Size = 15f, Color = ColGold, Align = TextAlign.Center };
    private readonly TextSprite _keepProgressText = new() { Size = 13f, Align = TextAlign.Center };

    // ── Cached paints (background / keep / HUD / aim) ────────────────────
    private static readonly SKPaint SkyPaint = new();
    private static readonly SKPaint GroundPaint = new() { Color = ColGround };
    private static readonly SKPaint GroundEdgePaint = new() { Color = ColGroundEdge };
    private static readonly SKPaint HillPaint = new() { Color = new SKColor(0x2A, 0x20, 0x12), IsAntialias = true };
    private static readonly SKPath HillPath;

    private static readonly SKPaint KeepFoundPaint = new() { Color = ColKeepBase, IsAntialias = true };
    private static readonly SKPaint KeepFillPaint = new() { Color = ColKeepDone };
    private static readonly SKPaint KeepBorderPaint = new() { Color = ColStoneDmg, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
    private static readonly SKPaint KeepMerlonPaint = new() { Color = ColKeepBase };

    private static readonly SKPaint HudBarBg = new() { Color = new SKColor(0x30, 0x30, 0x30) };
    private static readonly SKPaint HudBarFg = new() { Color = ColKeepProg };
    private static readonly SKPaint BtnStripPaint = new() { Color = new SKColor(0x00, 0x00, 0x00, 140) };

    private static readonly SKPaint AimDotPaint = new()
    {
        Color = new SKColor(0xFF, 0xFF, 0xFF, 100),
        StrokeWidth = 1.5f,
        IsAntialias = true,
        PathEffect = SKPathEffect.CreateDash([4f, 6f], 0f)
    };
    private static readonly SKPaint AimLandPaint = new() { Color = new SKColor(0xFF, 0xFF, 0x00, 180), IsAntialias = true };

    static PlayScreen()
    {
        // Build the sky shader once
        var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, GroundY),
            [ColSky, ColHorizon], [0f, 1f], SKShaderTileMode.Clamp);
        SkyPaint.Shader = skyShader;

        // Build the hill path once
        HillPath = new SKPath();
        HillPath.MoveTo(0, GroundY);
        HillPath.CubicTo(200, GroundY - 60, 400, GroundY - 80, 600, GroundY - 40);
        HillPath.CubicTo(800, GroundY, 900, GroundY - 50, 1100, GroundY - 30);
        HillPath.LineTo(GameWidth, GroundY);
        HillPath.LineTo(0, GroundY);
        HillPath.Close();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    public override void OnActivating()
    {
        state.Score = 0;
        state.Level = 1;
        _archerCount = 3;
        _workerCount = 3;
        _keepProgress = 0f;
        _lordHP = 10f;
        _lordActive = false;
        _lordAttackTimer = 0f;
        _oilAvail = true;
        _mangonelAvail = true;
        _logsAvail = true;
        _leftHeld = false;
        _rightHeld = false;
        _touchAimLeft = false;
        _touchAimRight = false;
        _arrowCooldown = default;
        _aimAngle = AimDefault;
        _spawnTimer = 3.5f;
        _spawnInterval = 5f;
        _levelTime = 0f;
        _cowSpawned = false;
        _cowSpawnTime = 25f;
        _accuracyMult = 1;
        _consecutiveHits = 0;
        _accuracyResetTimer = default;
        _wallFlash = new CountdownTimer[3];
        _enemies.Clear();
        _arrows.Clear();
        _boulders.Clear();
        _texts.Clear();
        InitWalls();
    }

    private void InitWalls()
    {
        _walls.Clear();
        _walls.Add(new Wall(InnerWallX + BlockW / 2f, 6, hasArcher: true));
        _walls.Add(new Wall(MiddleWallX + BlockW / 2f, 4, hasArcher: true));
        _walls.Add(new Wall(OuterWallX + BlockW / 2f, 2, hasArcher: true));
    }

    // ── Input ─────────────────────────────────────────────────────────────

    public override void OnPointerDown(float x, float y)
    {
        if (BtnAimLeft.Contains(x, y)) { _touchAimLeft = true; _aimAngle = Math.Max(AimMin, _aimAngle - AimStep); return; }
        if (BtnAimRight.Contains(x, y)) { _touchAimRight = true; _aimAngle = Math.Min(AimMax, _aimAngle + AimStep); return; }
        if (BtnA2W.Contains(x, y)) { ConvertArcherToWorker(); return; }
        if (BtnW2A.Contains(x, y)) { ConvertWorkerToArcher(); return; }
        if (BtnFire.Contains(x, y)) { FireVolley(); return; }
        if (BtnOil.Contains(x, y)) { UseOil(); return; }
        if (BtnCannon.Contains(x, y)) { UseMangonel(); return; }
        if (BtnLogs.Contains(x, y)) { UseLogs(); return; }

        if (y < BtnY && x > KeepRight + 20f)
        {
            AimAtPoint(x, y);
            FireVolley();
        }
    }

    public override void OnPointerUp(float x, float y)
    {
        _touchAimLeft = false;
        _touchAimRight = false;
    }

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft": _leftHeld = true; break;
            case "ArrowRight": _rightHeld = true; break;
            case " ": FireVolley(); break;
            case "ArrowDown": ConvertArcherToWorker(); break;
            case "ArrowUp": ConvertWorkerToArcher(); break;
            case "z": case "Z": UseOil(); break;
            case "x": case "X": UseMangonel(); break;
            case "c": case "C": UseLogs(); break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft": _leftHeld = false; break;
            case "ArrowRight": _rightHeld = false; break;
        }
    }

    // ── Archer ↔ Worker conversion ────────────────────────────────────────

    private void ConvertArcherToWorker()
    {
        if (_workerCount <= 1 || _archerCount <= 0) return;
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            if (_walls[i].HasArcher)
            {
                _walls[i].HasArcher = false;
                _archerCount--;
                _workerCount++;
                SpawnText("Archer → Worker", _walls[i].CenterX, _walls[i].TopY - 40f, ColWorker);
                return;
            }
        }
    }

    private void ConvertWorkerToArcher()
    {
        if (_workerCount <= 1 || _archerCount >= _walls.Count) return;
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            if (!_walls[i].IsDestroyed && !_walls[i].HasArcher)
            {
                _walls[i].HasArcher = true;
                _archerCount++;
                _workerCount--;
                SpawnText("Worker → Archer", _walls[i].CenterX, _walls[i].TopY - 40f, ColArcher);
                return;
            }
        }
    }

    // ── Special weapons ───────────────────────────────────────────────────

    private void UseOil()
    {
        if (!_oilAvail) return;
        _oilAvail = false;
        float maxRange = OuterWallX + BlockW + 80f;
        foreach (var e in _enemies)
        {
            if (!e.Active || e.X > maxRange) continue;
            if (e.Type is EnemyType.Catapult or EnemyType.Ram) continue;
            KillEnemy(e, 0f, 0f);
        }
        SpawnText("BOILING OIL!", GameWidth / 2f, 120f, ColOil);
    }

    private void UseMangonel()
    {
        if (!_mangonelAvail) return;
        _mangonelAvail = false;
        float minX = OuterWallX + BlockW + 200f;
        foreach (var e in _enemies)
        {
            if (!e.Active || e.X < minX) continue;
            KillEnemy(e, 0f, 0f);
        }
        SpawnText("MANGONEL FIRES!", GameWidth / 2f, 120f, ColBoulder);
    }

    private void UseLogs()
    {
        if (!_logsAvail) return;
        _logsAvail = false;
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            KillEnemy(e, 0f, 0f);
        }
        SpawnText("FLAMING LOGS!", GameWidth / 2f, 120f, ColFire);
    }

    // ── Arrow firing ──────────────────────────────────────────────────────

    private void FireVolley()
    {
        if (_arrowCooldown.Active || _archerCount == 0) return;
        _arrowCooldown.Set(ArrowCooldownTime);

        float rad = _aimAngle * MathF.PI / 180f;
        float vx = ArrowSpeed * MathF.Cos(rad);
        float vy = -ArrowSpeed * MathF.Sin(rad);

        foreach (var wall in _walls)
        {
            if (!wall.HasArcher || wall.IsDestroyed) continue;
            float ax = wall.ArcherCenterX;
            float ay = wall.ArcherBaseY - ArcherH / 2f;
            var arrow = new Arrow { X = ax, Y = ay };
            arrow.Rigidbody.VelocityX = vx;
            arrow.Rigidbody.VelocityY = vy;
            _arrows.Add(arrow);
        }
    }

    private void AimAtPoint(float tx, float ty)
    {
        Wall? refWall = null;
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            if (!_walls[i].IsDestroyed && _walls[i].HasArcher)
            { refWall = _walls[i]; break; }
        }
        float ax = refWall?.ArcherCenterX ?? (InnerWallX + BlockW / 2f);
        float ay = refWall != null ? refWall.ArcherBaseY - ArcherH / 2f : GroundY - ArcherH;

        float dx = tx - ax;
        float dy = ay - ty;
        float angle = dx > 0 ? MathF.Atan2(dy, dx) * 180f / MathF.PI : AimMin;
        _aimAngle = Math.Clamp(angle, AimMin, AimMax);
    }

    // ── Update ────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        _levelTime += deltaTime;
        _arrowCooldown.Tick(deltaTime);

        if (_leftHeld || _touchAimLeft) _aimAngle = Math.Max(AimMin, _aimAngle - AimSpeed * deltaTime);
        if (_rightHeld || _touchAimRight) _aimAngle = Math.Min(AimMax, _aimAngle + AimSpeed * deltaTime);

        if (_accuracyResetTimer.Tick(deltaTime))
        {
            _accuracyMult = 1;
            _consecutiveHits = 0;
        }

        // Keep construction
        if (_workerCount > 0 && _keepProgress < 1f)
        {
            _keepProgress = Math.Min(1f, _keepProgress + _workerCount * WorkerBuildRate * deltaTime);
            if (_keepProgress >= 1f)
                coordinator.PushOverlay<VictoryScreen>();
        }

        if (!_cowSpawned && _levelTime >= _cowSpawnTime)
        {
            SpawnCow();
            _cowSpawned = true;
        }

        UpdateSpawning(deltaTime);

        for (int i = 0; i < _wallFlash.Length; i++)
            _wallFlash[i].Tick(deltaTime);

        UpdateEnemies(deltaTime);

        if (!_lordActive && _walls.All(w => w.IsDestroyed))
        {
            _lordActive = true;
            SpawnText("THE LORD DEFENDS!", GameWidth / 2f, 200f, ColGold);
        }

        if (_lordActive) UpdateLord(deltaTime);
        UpdateArrows(deltaTime);
        UpdateBoulders(deltaTime);

        for (int i = _texts.Count - 1; i >= 0; i--)
        {
            _texts[i].Life -= deltaTime;
            _texts[i].Rigidbody.Step(_texts[i], deltaTime);
            if (_texts[i].Life <= 0f) _texts.RemoveAt(i);
        }
    }

    // ── Spawning ──────────────────────────────────────────────────────────

    private void UpdateSpawning(float deltaTime)
    {
        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f) return;
        _spawnInterval = Math.Max(1.5f, 5f - _levelTime * 0.02f);
        _spawnTimer = _spawnInterval + Random.Shared.NextSingle() * 1.5f;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        float t = _levelTime;
        EnemyType type;
        double r = Random.Shared.NextDouble();

        if (t < 15f)
            type = r < 0.7 ? EnemyType.Spearman : EnemyType.Swordsman;
        else if (t < 35f)
            type = r < 0.4 ? EnemyType.Spearman : r < 0.7 ? EnemyType.Swordsman : EnemyType.Berserker;
        else if (t < 60f)
            type = r < 0.25 ? EnemyType.Spearman : r < 0.45 ? EnemyType.Swordsman
                 : r < 0.65 ? EnemyType.Berserker : r < 0.80 ? EnemyType.Crossbowman
                 : EnemyType.Catapult;
        else
            type = r < 0.15 ? EnemyType.Spearman : r < 0.30 ? EnemyType.Swordsman
                 : r < 0.50 ? EnemyType.Berserker : r < 0.65 ? EnemyType.Crossbowman
                 : r < 0.78 ? EnemyType.Catapult : EnemyType.Ram;

        _enemies.Add(CreateEnemy(type));
    }

    private void SpawnCow() => _enemies.Add(CreateEnemy(EnemyType.Cow));

    private static Enemy CreateEnemy(EnemyType type)
    {
        var e = type switch
        {
            EnemyType.Spearman   => new Enemy { Type = type, X = SpawnX, HP = 1f,  MaxHP = 1f,  Speed = 95f,  Collider = new() { Width = 12f, Height = 26f }, AttackInterval = 0.5f, AttackDamage = 8f,  AttackRange = BlockW + 2f },
            EnemyType.Swordsman  => new Enemy { Type = type, X = SpawnX, HP = 4f,  MaxHP = 4f,  Speed = 45f,  Collider = new() { Width = 14f, Height = 28f }, AttackInterval = 1.0f, AttackDamage = 12f, AttackRange = BlockW + 2f },
            EnemyType.Berserker  => new Enemy { Type = type, X = SpawnX, HP = 2f,  MaxHP = 2f,  Speed = 110f, Collider = new() { Width = 13f, Height = 24f }, AttackInterval = 0.4f, AttackDamage = 10f, AttackRange = BlockW + 2f },
            EnemyType.Crossbowman => new Enemy { Type = type, X = SpawnX, HP = 1f, MaxHP = 1f,  Speed = 65f,  Collider = new() { Width = 12f, Height = 26f }, AttackInterval = 0f,   AttackDamage = 0f,  AttackRange = 250f, FireInterval = 3f },
            EnemyType.Catapult   => new Enemy { Type = type, X = SpawnX, HP = 5f,  MaxHP = 5f,  Speed = 35f,  Collider = new() { Width = 36f, Height = 26f }, AttackInterval = 4f,   AttackDamage = 0f,  AttackRange = 180f },
            EnemyType.Ram        => new Enemy { Type = type, X = SpawnX, HP = 10f, MaxHP = 10f, Speed = 28f,  Collider = new() { Width = 40f, Height = 22f } },
            EnemyType.Cow        => new Enemy { Type = type, X = SpawnX, HP = 1f,  MaxHP = 1f,  Speed = 55f,  Collider = new() { Width = 28f, Height = 20f } },
            _                    => new Enemy { Type = type, X = SpawnX, HP = 1f,  MaxHP = 1f,  Speed = 50f,  Collider = new() { Width = 12f, Height = 24f } }
        };
        e.Y = GroundY - e.Collider.Height / 2f;
        // Sync sprite
        e.Sprite.Type = e.Type;
        e.Sprite.Collider = e.Collider;
        e.Sprite.HP = e.HP;
        e.Sprite.MaxHP = e.MaxHP;
        return e;
    }

    // ── Enemy update ──────────────────────────────────────────────────────

    private void UpdateEnemies(float deltaTime)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var e = _enemies[i];
            if (!e.Active) { _enemies.RemoveAt(i); continue; }
            UpdateEnemy(e, deltaTime);
        }
    }

    private void UpdateEnemy(Enemy e, float dt)
    {
        int tgtWall = FindTargetWall(e.X);
        switch (e.Type)
        {
            case EnemyType.Cow:
                e.Rigidbody.VelocityX = -e.Speed;
                e.Rigidbody.Step(e, dt);
                if (e.X < -e.Collider.Width) e.Active = false;
                return;
            case EnemyType.Ram: UpdateRam(e, dt, tgtWall); return;
            case EnemyType.Catapult: UpdateCatapult(e, dt, tgtWall); return;
            case EnemyType.Crossbowman: UpdateCrossbowman(e, dt, tgtWall); return;
            default: UpdateMeleeEnemy(e, dt, tgtWall); return;
        }
    }

    private int FindTargetWall(float enemyX)
    {
        for (int i = _walls.Count - 1; i >= 0; i--)
            if (!_walls[i].IsDestroyed) return i;
        return -1;
    }

    private void UpdateMeleeEnemy(Enemy e, float dt, int tgtWall)
    {
        if (tgtWall < 0)
        {
            float targetX = _lordActive ? LordStandX : KeepRight;
            if (e.X - e.Collider.Width / 2f > targetX + LordW + 5f)
            {
                e.State = EnemyState.Walking;
                e.Rigidbody.VelocityX = -e.Speed;
                e.Rigidbody.Step(e, dt);
            }
            else
            {
                e.State = EnemyState.AttackingLord;
                e.AttackTimer += dt;
                if (e.AttackTimer >= e.AttackInterval)
                {
                    e.AttackTimer = 0f;
                    _lordHP -= e.AttackDamage * 0.5f;
                    if (_lordHP <= 0f) coordinator.PushOverlay<GameOverScreen>();
                }
            }
            return;
        }

        var wall = _walls[tgtWall];
        float wallFace = wall.LeftX - 2f;

        if (e.X - e.Collider.Width / 2f > wallFace + e.AttackRange)
        {
            e.State = EnemyState.Walking;
            e.Rigidbody.VelocityX = -e.Speed;
            e.Rigidbody.Step(e, dt);
        }
        else
        {
            e.State = EnemyState.AttackingWall;
            e.AttackTimer += dt;
            if (e.AttackTimer >= e.AttackInterval)
            {
                e.AttackTimer = 0f;
                wall.TakeDamage(e.AttackDamage);
                if (tgtWall < _wallFlash.Length) _wallFlash[tgtWall].Set(0.15f);
                if (wall.IsDestroyed)
                {
                    wall.HasArcher = false;
                    UpdateArcherCount();
                }
            }
        }
    }

    private void UpdateRam(Enemy e, float dt, int tgtWall)
    {
        if (tgtWall < 0)
        {
            if (e.X - e.Collider.Width / 2f > KeepRight + 5f)
            {
                e.Rigidbody.VelocityX = -e.Speed;
                e.Rigidbody.Step(e, dt);
            }
            else
            {
                _lordHP -= 5f * dt;
                if (_lordHP <= 0f) coordinator.PushOverlay<GameOverScreen>();
            }
            return;
        }

        var wall = _walls[tgtWall];
        if (e.X - e.Collider.Width / 2f > wall.LeftX + 1f)
        {
            e.State = EnemyState.Walking;
            e.Rigidbody.VelocityX = -e.Speed;
            e.Rigidbody.Step(e, dt);
        }
        else
        {
            wall.Demolish();
            if (tgtWall < _wallFlash.Length) _wallFlash[tgtWall].Set(0.3f);
            UpdateArcherCount();
            SpawnText("WALL BREACHED!", wall.CenterX, wall.TopY - 50f, ColRed);
            e.Rigidbody.VelocityX = -e.Speed * 0.5f;
            e.Rigidbody.Step(e, dt);
        }
    }

    private void UpdateCatapult(Enemy e, float dt, int tgtWall)
    {
        if (tgtWall < 0) { e.Rigidbody.VelocityX = -e.Speed; e.Rigidbody.Step(e, dt); return; }

        var wall = _walls[tgtWall];
        float stopX = wall.LeftX + BlockW + e.AttackRange + e.Collider.Width / 2f;

        if (e.X > stopX)
        {
            e.State = EnemyState.Walking;
            e.Rigidbody.VelocityX = -e.Speed;
            e.Rigidbody.Step(e, dt);
        }
        else
        {
            e.State = EnemyState.Idle;
            e.AttackTimer += dt;
            if (e.AttackTimer >= e.AttackInterval)
            {
                e.AttackTimer = 0f;
                FireBoulder(e, tgtWall);
            }
        }
    }

    private void UpdateCrossbowman(Enemy e, float dt, int tgtWall)
    {
        int archerWallIdx = -1;
        for (int i = _walls.Count - 1; i >= 0; i--)
            if (!_walls[i].IsDestroyed && _walls[i].HasArcher) { archerWallIdx = i; break; }

        if (archerWallIdx < 0) { UpdateMeleeEnemy(e, dt, tgtWall); return; }

        var targetWall = _walls[archerWallIdx];
        float stopX = targetWall.LeftX - e.AttackRange;

        if (e.X > stopX)
        {
            e.State = EnemyState.Walking;
            e.Rigidbody.VelocityX = -e.Speed;
            e.Rigidbody.Step(e, dt);
        }
        else
        {
            e.State = EnemyState.Shooting;
            e.FireCooldown -= dt;
            if (e.FireCooldown <= 0f)
            {
                e.FireCooldown = e.FireInterval;
                FireCrossbowBolt(e, archerWallIdx);
            }
        }
    }

    private void FireBoulder(Enemy catapult, int targetWallIdx)
    {
        var wall = _walls[targetWallIdx];
        float dx = wall.CenterX - catapult.X;
        float dy = wall.TopY - (GroundY - 30f);
        float T = Math.Abs(dx) / 260f;
        float vx = dx / T;
        float vy = (dy - 0.5f * ArrowGravity * T * T) / T;
        var boulder = new Boulder { X = catapult.X, Y = GroundY - 30f, TargetWallIdx = targetWallIdx };
        boulder.Rigidbody.VelocityX = vx;
        boulder.Rigidbody.VelocityY = vy;
        _boulders.Add(boulder);
    }

    private void FireCrossbowBolt(Enemy crossbow, int archerWallIdx)
    {
        var wall = _walls[archerWallIdx];
        float ay = wall.ArcherBaseY - ArcherH / 2f;
        var bolt = new Arrow { X = crossbow.X - crossbow.Collider.Width / 2f, Y = ay, IsEnemy = true, EnemyTargetWall = archerWallIdx };
        bolt.Rigidbody.VelocityX = -320f;
        _arrows.Add(bolt);
    }

    // ── Lord ──────────────────────────────────────────────────────────────

    private void UpdateLord(float dt)
    {
        _lordAttackTimer += dt;
        if (_lordAttackTimer < LordAttackInterval) return;
        _lordAttackTimer = 0f;

        Enemy? target = null;
        float minDist = float.MaxValue;
        foreach (var e in _enemies)
        {
            if (!e.Active || e.Type == EnemyType.Cow) continue;
            float dist = MathF.Abs(e.X - LordStandX);
            if (dist < minDist && dist < LordMeleeRange + 60f) { minDist = dist; target = e; }
        }

        if (target != null)
        {
            target.HP -= LordAttackDamage;
            if (target.HP <= 0f) KillEnemy(target, target.X, GroundY - target.Collider.Height);
        }
    }

    // ── Arrows & boulders ─────────────────────────────────────────────────

    private void UpdateArrows(float dt)
    {
        for (int i = _arrows.Count - 1; i >= 0; i--)
        {
            var a = _arrows[i];
            if (!a.Active) { _arrows.RemoveAt(i); continue; }

            a.Rigidbody.VelocityY += ArrowGravity * dt;
            a.Rigidbody.Step(a, dt);

            if (a.X > GameWidth + 20f || a.X < -20f || a.Y > GroundY + 20f)
            {
                if (!a.IsEnemy) { _consecutiveHits = 0; _accuracyMult = 1; _accuracyResetTimer.Reset(); }
                a.Active = false;
                _arrows.RemoveAt(i);
                continue;
            }

            if (a.IsEnemy) ResolveBoltHitsArcher(a);
            else ResolveArrowHitsEnemy(a);
        }
    }

    private void ResolveArrowHitsEnemy(Arrow a)
    {
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            if (!CollisionResolver.TryGetHit(a.X, a.Y, a.Collider, e.X, e.Y, e.Collider, out _)) continue;

            float pts = PointsForEnemy(e.Type);
            e.HP -= 1f;

            if (_archerCount == 1)
            {
                _consecutiveHits++;
                _accuracyResetTimer.Set(3f);
                if (_consecutiveHits % 3 == 0) _accuracyMult = Math.Min(8, _accuracyMult + 1);
                pts += 25f * _accuracyMult;
            }

            if (e.HP <= 0f) KillEnemy(e, a.X, a.Y);
            else SpawnText($"-{1}", e.X, GroundY - e.Collider.Height - 5f, ColDim);

            a.Active = false;
            return;
        }
    }

    private void ResolveBoltHitsArcher(Arrow bolt)
    {
        int idx = bolt.EnemyTargetWall;
        if (idx < 0 || idx >= _walls.Count) { bolt.Active = false; return; }
        var wall = _walls[idx];
        if (!wall.HasArcher || wall.IsDestroyed) { bolt.Active = false; return; }

        float ax = wall.ArcherCenterX;
        float ay = wall.ArcherBaseY - ArcherH;
        if (MathF.Abs(bolt.X - ax) < ArcherW && bolt.Y >= ay && bolt.Y <= ay + ArcherH)
        {
            wall.HasArcher = false;
            _archerCount = Math.Max(0, _archerCount - 1);
            SpawnText("Archer down!", ax, ay - 15f, ColRed);
            bolt.Active = false;
        }
    }

    private void UpdateBoulders(float dt)
    {
        for (int i = _boulders.Count - 1; i >= 0; i--)
        {
            var b = _boulders[i];
            if (!b.Active) { _boulders.RemoveAt(i); continue; }

            b.Rigidbody.VelocityY += ArrowGravity * dt;
            b.Rigidbody.Step(b, dt);

            if (b.X < -40f || b.X > GameWidth + 40f || b.Y > GroundY + 20f)
            { b.Active = false; _boulders.RemoveAt(i); continue; }

            if (b.TargetWallIdx >= 0 && b.TargetWallIdx < _walls.Count)
            {
                var wall = _walls[b.TargetWallIdx];
                if (!wall.IsDestroyed && b.X >= wall.LeftX - 10f && b.X <= wall.LeftX + BlockW + 10f)
                {
                    wall.TakeDamage(50f);
                    if (b.TargetWallIdx < _wallFlash.Length) _wallFlash[b.TargetWallIdx].Set(0.2f);
                    if (wall.IsDestroyed) { wall.HasArcher = false; UpdateArcherCount(); }
                    b.Active = false;
                    _boulders.RemoveAt(i);
                }
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void KillEnemy(Enemy e, float x, float y)
    {
        if (!e.Active) return;
        e.Active = false;
        int pts = PointsForEnemy(e.Type);
        state.Score += pts;
        SpawnText($"+{pts}", x > 0 ? x : e.X, GroundY - e.Collider.Height - 10f,
            e.Type == EnemyType.Cow ? ColGold : ColAccent);
    }

    private static int PointsForEnemy(EnemyType t) => t switch
    {
        EnemyType.Spearman => 100,
        EnemyType.Swordsman => 200,
        EnemyType.Berserker => 150,
        EnemyType.Crossbowman => 250,
        EnemyType.Catapult => 300,
        EnemyType.Ram => 500,
        EnemyType.Cow => 3000,
        _ => 50
    };

    private void UpdateArcherCount()
        => _archerCount = _walls.Count(w => w.HasArcher && !w.IsDestroyed);

    private void SpawnText(string text, float x, float y, SKColor color)
        => _texts.Add(new FloatText { Text = text, X = x, Y = y, Life = 1.4f, Color = color });

    // ── Draw ──────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(ColSky);
        DrawGame(canvas);
    }

    internal void DrawGame(SKCanvas canvas)
    {
        DrawBackground(canvas);
        DrawKeep(canvas);
        DrawWalls(canvas);
        DrawWorkers(canvas);
        DrawLord(canvas);
        DrawEnemies(canvas, _enemies);
        DrawBoulders(canvas, _boulders);
        DrawArrows(canvas, _arrows);
        DrawAimIndicator(canvas);
        DrawFloatTexts(canvas);
        DrawHud(canvas);
    }

    // ── Background ────────────────────────────────────────────────────────

    private static void DrawBackground(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GroundY), SkyPaint);
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, GameHeight - GroundY), GroundPaint);
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, 4f), GroundEdgePaint);
        canvas.DrawPath(HillPath, HillPaint);
    }

    // ── Keep ──────────────────────────────────────────────────────────────

    private void DrawKeep(SKCanvas canvas)
    {
        float builtH = KeepFullH * _keepProgress;
        float builtY = KeepBaseY - builtH;
        float keepW = KeepRight - KeepLeft;

        canvas.DrawRect(SKRect.Create(KeepLeft, KeepBaseY - KeepFullH, keepW, KeepFullH), KeepFoundPaint);

        if (builtH > 0f)
            canvas.DrawRect(SKRect.Create(KeepLeft, builtY, keepW, builtH), KeepFillPaint);

        canvas.DrawRect(SKRect.Create(KeepLeft, KeepBaseY - KeepFullH, keepW, KeepFullH), KeepBorderPaint);

        float merlonW = 14f, merlonH = 14f, gap = 4f;
        float startX = KeepLeft + 4f;
        float topY = KeepBaseY - KeepFullH;
        while (startX + merlonW <= KeepRight - 4f)
        {
            canvas.DrawRect(SKRect.Create(startX, topY - merlonH, merlonW, merlonH), KeepMerlonPaint);
            startX += merlonW + gap;
        }

        if (_keepProgress < 1f)
        {
            _keepProgressText.Text = $"Keep {(int)(_keepProgress * 100)}%";
            _keepProgressText.Color = ColHud;
        }
        else
        {
            _keepProgressText.Text = "COMPLETE!";
            _keepProgressText.Color = ColGold;
        }
        canvas.Save(); canvas.Translate((KeepLeft + KeepRight) / 2f, KeepBaseY - KeepFullH - 20f); _keepProgressText.Draw(canvas); canvas.Restore();
    }

    // ── Walls ─────────────────────────────────────────────────────────────

    private void DrawWalls(SKCanvas canvas)
    {
        for (int wi = 0; wi < _walls.Count; wi++)
        {
            var wall = _walls[wi];
            bool flash = wi < _wallFlash.Length && _wallFlash[wi].Active;

            float blockY = GroundY - BlockH;
            for (int bi = wall.Blocks.Count - 1; bi >= 0; bi--)
            {
                var block = wall.Blocks[bi];
                if (!block.Active) continue;

                _wallBlockSprite.HPRatio = block.HP / block.MaxHP;
                _wallBlockSprite.Flash = flash;
                canvas.Save(); canvas.Translate(wall.LeftX, blockY); _wallBlockSprite.Draw(canvas); canvas.Restore();
                blockY -= BlockH;
            }

            if (wall.HasArcher && !wall.IsDestroyed)
            {
                canvas.Save(); canvas.Translate(wall.ArcherCenterX, wall.ArcherBaseY); _archerSprite.Draw(canvas); canvas.Restore();
            }
        }
    }

    // ── Workers ───────────────────────────────────────────────────────────

    private void DrawWorkers(SKCanvas canvas)
    {
        float workerAreaW = KeepRight - KeepLeft - 10f;
        float spacing = workerAreaW / Math.Max(_workerCount, 1);
        for (int w = 0; w < _workerCount; w++)
        {
            canvas.Save(); canvas.Translate(KeepLeft + 8f + w * spacing, GroundY); _workerSprite.Draw(canvas); canvas.Restore();
        }
    }

    // ── Lord ──────────────────────────────────────────────────────────────

    private void DrawLord(SKCanvas canvas)
    {
        _lordSprite.Active = _lordActive;
        _lordSprite.HPRatio = _lordHP / 10f;
        canvas.Save(); canvas.Translate(LordStandX, GroundY); _lordSprite.Draw(canvas); canvas.Restore();
    }

    // ── Enemies ───────────────────────────────────────────────────────────

    private static void DrawEnemies(SKCanvas canvas, List<Enemy> enemies)
    {
        foreach (var e in enemies)
        {
            if (!e.Active) continue;
            e.Sprite.HP = e.HP;
            canvas.Save(); canvas.Translate(e.X, e.Y); e.Sprite.Draw(canvas); canvas.Restore();
        }
    }

    // ── Arrows ────────────────────────────────────────────────────────────

    private static void DrawArrows(SKCanvas canvas, List<Arrow> arrows)
    {
        foreach (var a in arrows)
        {
            if (!a.Active) continue;
            a.Sprite.IsEnemy = a.IsEnemy;
            a.Sprite.VelocityX = a.Rigidbody.VelocityX;
            a.Sprite.VelocityY = a.Rigidbody.VelocityY;
            canvas.Save(); canvas.Translate(a.X, a.Y); a.Sprite.Draw(canvas); canvas.Restore();
        }
    }

    private static void DrawBoulders(SKCanvas canvas, List<Boulder> boulders)
    {
        foreach (var b in boulders)
        {
            if (!b.Active) continue;
            canvas.Save(); canvas.Translate(b.X, b.Y); b.Sprite.Draw(canvas); canvas.Restore();
        }
    }

    // ── Aim indicator ─────────────────────────────────────────────────────

    private void DrawAimIndicator(SKCanvas canvas)
    {
        if (_archerCount == 0) return;

        Wall? refWall = null;
        for (int i = _walls.Count - 1; i >= 0; i--)
            if (!_walls[i].IsDestroyed && _walls[i].HasArcher) { refWall = _walls[i]; break; }
        if (refWall == null) return;

        float ax = refWall.ArcherCenterX;
        float ay = refWall.ArcherBaseY - ArcherH / 2f;
        float rad = _aimAngle * MathF.PI / 180f;
        float vx = ArrowSpeed * MathF.Cos(rad);
        float vy = -ArrowSpeed * MathF.Sin(rad);

        using var path = new SKPath();
        bool first = true;
        for (float t = 0; t < 3f; t += 0.05f)
        {
            float nx = ax + vx * t;
            float ny = ay + vy * t + 0.5f * ArrowGravity * t * t;
            if (nx > GameWidth || ny > GroundY) break;
            if (first) { path.MoveTo(nx, ny); first = false; }
            else path.LineTo(nx, ny);
        }
        canvas.DrawPath(path, AimDotPaint);

        float landT = FindLandingTime(ay, vy);
        if (landT > 0f)
        {
            float landX = ax + vx * landT;
            if (landX >= 0 && landX <= GameWidth)
                canvas.DrawCircle(landX, GroundY - 3f, 5f, AimLandPaint);
        }
    }

    private float FindLandingTime(float startY, float vy0)
    {
        float dy = GroundY - startY;
        float a = 0.5f * ArrowGravity;
        float b = vy0;
        float c = -dy;
        float disc = b * b - 4f * a * c;
        if (disc < 0f) return -1f;
        float t1 = (-b + MathF.Sqrt(disc)) / (2f * a);
        float t2 = (-b - MathF.Sqrt(disc)) / (2f * a);
        float t = t1 > 0 ? (t2 > 0 ? Math.Min(t1, t2) : t1) : t2;
        return t > 0f ? t : -1f;
    }

    // ── HUD ───────────────────────────────────────────────────────────────

    private void DrawHud(SKCanvas canvas)
    {
        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(8f, 26f); _scoreText.Draw(canvas); canvas.Restore();

        _levelText.Text = $"Level {state.Level}";
        canvas.Save(); canvas.Translate(GameWidth - 8f, 26f); _levelText.Draw(canvas); canvas.Restore();

        _archerText.Text = $"Archers: {_archerCount}";
        canvas.Save(); canvas.Translate(8f, 50f); _archerText.Draw(canvas); canvas.Restore();
        _workerText.Text = $"Workers: {_workerCount}";
        canvas.Save(); canvas.Translate(8f, 70f); _workerText.Draw(canvas); canvas.Restore();

        float barX = 8f, barY = 82f, barW = 140f, barH = 10f;
        canvas.DrawRect(SKRect.Create(barX, barY, barW, barH), HudBarBg);
        canvas.DrawRect(SKRect.Create(barX, barY, barW * _keepProgress, barH), HudBarFg);
        canvas.Save(); canvas.Translate(barX, barY - 3f); _keepLabel.Draw(canvas); canvas.Restore();

        _aimText.Text = $"Aim: {_aimAngle:F0}°";
        canvas.Save(); canvas.Translate(GameWidth / 2f, 20f); _aimText.Draw(canvas); canvas.Restore();

        if (_arrowCooldown.Active)
        {
            float r = _arrowCooldown.Remaining / ArrowCooldownTime;
            _cooldownDot.Color = new SKColor(0xFF, 0xFF, 0xFF, (byte)(r * 200));
            canvas.Save(); canvas.Translate(GameWidth / 2f, 38f); _cooldownDot.Draw(canvas); canvas.Restore();
        }

        if (_lordActive)
        {
            float ratio = _lordHP / 10f;
            _lordHpText.Text = $"Lord HP: {_lordHP:F0}";
            _lordHpText.Color = ratio < 0.3f ? ColRed : ColGold;
            canvas.Save(); canvas.Translate(GameWidth - 160f, 50f); _lordHpText.Draw(canvas); canvas.Restore();
        }

        if (_archerCount == 1 && _accuracyMult > 1)
        {
            _accuracyText.Text = $"Accuracy ×{_accuracyMult}";
            canvas.Save(); canvas.Translate(GameWidth / 2f, 60f); _accuracyText.Draw(canvas); canvas.Restore();
        }

        DrawTouchButtons(canvas);
    }

    private void DrawTouchButtons(SKCanvas canvas)
    {
        canvas.DrawRect(SKRect.Create(0, BtnY - 4f, GameWidth, BtnH + 8f), BtnStripPaint);

        bool canA2W = _workerCount > 1 && _archerCount > 0;
        bool canW2A = _workerCount > 1 && _archerCount < _walls.Count;
        bool ready = !_arrowCooldown.Active && _archerCount > 0;

        DrawButton(canvas, BtnAimLeft, "← Aim", true, ColDim, _touchAimLeft);
        DrawButton(canvas, BtnAimRight, "Aim →", true, ColDim, _touchAimRight);
        DrawButton(canvas, BtnA2W, "↓ A→W", canA2W, ColWorker, false);
        DrawButton(canvas, BtnW2A, "↑ W→A", canW2A, ColArcher, false);
        DrawButton(canvas, BtnFire, "FIRE", ready, SKColors.White, false, large: true);
        DrawButton(canvas, BtnOil, "Oil (Z)", _oilAvail, ColOil, false);
        DrawButton(canvas, BtnCannon, "Cannon (X)", _mangonelAvail, ColBoulder, false);
        DrawButton(canvas, BtnLogs, "Logs (C)", _logsAvail, ColFire, false);
    }

    private void DrawButton(SKCanvas canvas, SKRect rect, string label,
        bool enabled, SKColor labelCol, bool pressed, bool large = false)
    {
        _buttonSprite.Rect = rect;
        _buttonSprite.Label = label;
        _buttonSprite.Enabled = enabled;
        _buttonSprite.LabelColor = labelCol;
        _buttonSprite.Pressed = pressed;
        _buttonSprite.Large = large;
        _buttonSprite.Draw(canvas);
    }

    // ── Float texts ───────────────────────────────────────────────────────

    private void DrawFloatTexts(SKCanvas canvas)
    {
        foreach (var t in _texts)
        {
            t.Sprite.Text = t.Text;
            t.Sprite.Color = t.Color;
            t.Sprite.Life = t.Life;
            canvas.Save(); canvas.Translate(t.X, t.Y); t.Sprite.Draw(canvas); canvas.Restore();
        }
    }
}
