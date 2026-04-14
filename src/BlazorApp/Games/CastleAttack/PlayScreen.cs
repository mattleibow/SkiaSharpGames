using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.CastleAttack.CastleAttackConstants;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

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
            if (!CollisionResolver.Overlaps(a, a.Collider, e, e.Collider)) continue;

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
        DrawEnemies(canvas);
        DrawBoulders(canvas);
        DrawArrows(canvas);
        DrawAimIndicator(canvas);
        DrawFloatTexts(canvas);
        DrawHud(canvas);
    }

    // ── Background ────────────────────────────────────────────────────────

    private static void DrawBackground(SKCanvas canvas)
    {
        using var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, GroundY),
            [ColSky, ColHorizon], [0f, 1f], SKShaderTileMode.Clamp);
        using var skyPaint = new SKPaint { Shader = skyShader };
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GroundY), skyPaint);

        using var gp = new SKPaint { Color = ColGround };
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, GameHeight - GroundY), gp);
        using var gep = new SKPaint { Color = ColGroundEdge };
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, 4f), gep);

        using var hillPaint = new SKPaint { Color = new SKColor(0x2A, 0x20, 0x12), IsAntialias = true };
        using var hillPath = new SKPath();
        hillPath.MoveTo(0, GroundY);
        hillPath.CubicTo(200, GroundY - 60, 400, GroundY - 80, 600, GroundY - 40);
        hillPath.CubicTo(800, GroundY, 900, GroundY - 50, 1100, GroundY - 30);
        hillPath.LineTo(GameWidth, GroundY);
        hillPath.LineTo(0, GroundY);
        hillPath.Close();
        canvas.DrawPath(hillPath, hillPaint);
    }

    // ── Keep ──────────────────────────────────────────────────────────────

    private void DrawKeep(SKCanvas canvas)
    {
        float builtH = KeepFullH * _keepProgress;
        float builtY = KeepBaseY - builtH;
        float keepW = KeepRight - KeepLeft;

        using var foundPaint = new SKPaint { Color = ColKeepBase, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(KeepLeft, KeepBaseY - KeepFullH, keepW, KeepFullH), foundPaint);

        if (builtH > 0f)
        {
            using var fillPaint = new SKPaint { Color = ColKeepDone };
            canvas.DrawRect(SKRect.Create(KeepLeft, builtY, keepW, builtH), fillPaint);
        }

        using var borderPaint = new SKPaint { Color = ColStoneDmg, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
        canvas.DrawRect(SKRect.Create(KeepLeft, KeepBaseY - KeepFullH, keepW, KeepFullH), borderPaint);

        float merlonW = 14f, merlonH = 14f, gap = 4f;
        float startX = KeepLeft + 4f;
        float topY = KeepBaseY - KeepFullH;
        using var mPaint = new SKPaint { Color = ColKeepBase };
        while (startX + merlonW <= KeepRight - 4f)
        {
            canvas.DrawRect(SKRect.Create(startX, topY - merlonH, merlonW, merlonH), mPaint);
            startX += merlonW + gap;
        }

        if (_keepProgress < 1f)
            DrawHelper.DrawCenteredText(canvas, $"Keep {(int)(_keepProgress * 100)}%", 13f, ColHud,
                (KeepLeft + KeepRight) / 2f, KeepBaseY - KeepFullH - 20f);
        else
            DrawHelper.DrawCenteredText(canvas, "COMPLETE!", 13f, ColGold,
                (KeepLeft + KeepRight) / 2f, KeepBaseY - KeepFullH - 20f);
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

                float ratio = block.HP / block.MaxHP;
                SKColor col = flash ? ColStoneDmg
                            : ratio < 0.3f ? ColStoneLow
                            : ratio < 0.6f ? ColStoneDmg
                            : ColStone;

                float bx = wall.LeftX;
                using var bp = new SKPaint { Color = col, IsAntialias = true };
                canvas.DrawRoundRect(SKRect.Create(bx, blockY, BlockW, BlockH), 3f, 3f, bp);

                if (ratio < 0.6f)
                {
                    using var cp = new SKPaint { Color = new SKColor(0x00, 0x00, 0x00, 80), StrokeWidth = 1.5f, IsAntialias = true };
                    canvas.DrawLine(bx + 8f, blockY + 4f, bx + 18f, blockY + BlockH - 4f, cp);
                }

                using var shinePaint = new SKPaint { Color = SKColors.White.WithAlpha(40), IsAntialias = true };
                canvas.DrawRect(SKRect.Create(bx + 2f, blockY + 2f, BlockW - 4f, BlockH / 2f - 2f), shinePaint);
                blockY -= BlockH;
            }

            if (wall.HasArcher && !wall.IsDestroyed)
                DrawArcher(canvas, wall.ArcherCenterX, wall.ArcherBaseY);
        }
    }

    private static void DrawArcher(SKCanvas canvas, float cx, float baseY)
    {
        using var bp = new SKPaint { Color = ColArcher, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(cx - ArcherW / 2f, baseY - ArcherH, ArcherW, ArcherH - 8f), bp);
        canvas.DrawCircle(cx, baseY - ArcherH - 5f, 6f, bp);
        using var bowPaint = new SKPaint { Color = ColArrow, StrokeWidth = 2f, IsAntialias = true };
        canvas.DrawLine(cx + ArcherW / 2f, baseY - ArcherH + 4f, cx + ArcherW / 2f + 8f, baseY - ArcherH / 2f, bowPaint);
    }

    // ── Workers ───────────────────────────────────────────────────────────

    private void DrawWorkers(SKCanvas canvas)
    {
        float workerAreaW = KeepRight - KeepLeft - 10f;
        float spacing = workerAreaW / Math.Max(_workerCount, 1);
        for (int w = 0; w < _workerCount; w++)
            DrawWorker(canvas, KeepLeft + 8f + w * spacing, GroundY);
    }

    private static void DrawWorker(SKCanvas canvas, float cx, float baseY)
    {
        using var p = new SKPaint { Color = ColWorker, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(cx - WorkerW / 2f, baseY - WorkerH, WorkerW, WorkerH - 6f), p);
        canvas.DrawCircle(cx, baseY - WorkerH - 4f, 5f, p);
        using var tp = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
        canvas.DrawLine(cx + WorkerW / 2f, baseY - WorkerH + 2f, cx + WorkerW / 2f + 10f, baseY - WorkerH + 10f, tp);
    }

    // ── Lord ──────────────────────────────────────────────────────────────

    private void DrawLord(SKCanvas canvas)
    {
        if (!_lordActive) return;
        float lx = LordStandX, ly = GroundY;
        float barW = 40f, ratio = _lordHP / 10f;
        using var barBg = new SKPaint { Color = new SKColor(0x40, 0x00, 0x00) };
        canvas.DrawRect(SKRect.Create(lx - barW / 2f, ly - LordH - 14f, barW, 6f), barBg);
        using var barFg = new SKPaint { Color = new SKColor(0xFF, 0x22, 0x22) };
        canvas.DrawRect(SKRect.Create(lx - barW / 2f, ly - LordH - 14f, barW * ratio, 6f), barFg);

        using var lp = new SKPaint { Color = ColLord, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(lx - LordW / 2f, ly - LordH, LordW, LordH - 8f), lp);
        canvas.DrawCircle(lx, ly - LordH - 5f, 7f, lp);
        using var cp = new SKPaint { Color = ColGold, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(lx - 7f, ly - LordH - 17f, 4f, 6f), cp);
        canvas.DrawRect(SKRect.Create(lx - 2f, ly - LordH - 20f, 4f, 9f), cp);
        canvas.DrawRect(SKRect.Create(lx + 3f, ly - LordH - 17f, 4f, 6f), cp);
        using var sp = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
        canvas.DrawLine(lx + LordW / 2f, ly - LordH, lx + LordW / 2f + 18f, ly - LordH / 2f, sp);
    }

    // ── Enemies ───────────────────────────────────────────────────────────

    private void DrawEnemies(SKCanvas canvas)
    {
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            DrawEnemy(canvas, e);
        }
    }

    private static void DrawEnemy(SKCanvas canvas, Enemy e)
    {
        SKColor col = CastleAttackConstants.EnemyCol(e.Type);
        switch (e.Type)
        {
            case EnemyType.Catapult: DrawCatapultSprite(canvas, e, col); break;
            case EnemyType.Ram: DrawRamSprite(canvas, e, col); break;
            case EnemyType.Cow: DrawCowSprite(canvas, e, col); break;
            default: DrawHumanoidEnemy(canvas, e, col); break;
        }

        if (e.MaxHP > 1f)
        {
            float ex = e.X - e.Collider.Width / 2f;
            float ey = GroundY - e.Collider.Height;
            float barW = e.Collider.Width + 4f;
            float ratio = e.HP / e.MaxHP;
            using var bg = new SKPaint { Color = new SKColor(0x40, 0x00, 0x00) };
            canvas.DrawRect(SKRect.Create(ex - 2f, ey - 8f, barW, 4f), bg);
            using var fg = new SKPaint { Color = new SKColor(0xFF, 0x44, 0x00) };
            canvas.DrawRect(SKRect.Create(ex - 2f, ey - 8f, barW * ratio, 4f), fg);
        }
    }

    private static void DrawHumanoidEnemy(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.Collider.Width / 2f, ey = GroundY - e.Collider.Height;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(ex, ey, e.Collider.Width, e.Collider.Height - 8f), p);
        canvas.DrawCircle(e.X, ey - 5f, 6f, p);

        if (e.Type == EnemyType.Crossbowman)
        {
            using var wp = new SKPaint { Color = ColArrow, StrokeWidth = 1.5f, IsAntialias = true };
            canvas.DrawLine(ex - 4f, ey + e.Collider.Height / 3f, ex - 12f, ey + e.Collider.Height / 3f, wp);
        }
        else if (e.Type is EnemyType.Spearman or EnemyType.Berserker)
        {
            using var wp = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
            canvas.DrawLine(ex - 2f, ey, ex - 14f, ey - 10f, wp);
        }
        else
        {
            using var wp = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 3f, IsAntialias = true };
            canvas.DrawLine(ex - 2f, ey + 2f, ex - 14f, ey + 8f, wp);
        }
    }

    private static void DrawCatapultSprite(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.Collider.Width / 2f, ey = GroundY - e.Collider.Height;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(ex, ey + 8f, e.Collider.Width, e.Collider.Height - 8f), 3f, 3f, p);
        using var arm = new SKPaint { Color = new SKColor(0x8B, 0x5E, 0x2E), StrokeWidth = 3f, IsAntialias = true };
        canvas.DrawLine(e.X, ey + 14f, e.X - 14f, ey, arm);
        using var wheel = new SKPaint { Color = new SKColor(0x55, 0x44, 0x22), IsAntialias = true };
        canvas.DrawCircle(ex + 6f, GroundY - 5f, 5f, wheel);
        canvas.DrawCircle(ex + e.Collider.Width - 6f, GroundY - 5f, 5f, wheel);
    }

    private static void DrawRamSprite(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.Collider.Width / 2f, ey = GroundY - e.Collider.Height;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(ex, ey, e.Collider.Width, e.Collider.Height - 6f), 4f, 4f, p);
        using var tip = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), IsAntialias = true };
        canvas.DrawRect(SKRect.Create(ex - 10f, ey + 6f, 14f, 8f), tip);
        using var wheel = new SKPaint { Color = new SKColor(0x55, 0x44, 0x22), IsAntialias = true };
        canvas.DrawCircle(ex + 6f, GroundY - 4f, 5f, wheel);
        canvas.DrawCircle(ex + e.Collider.Width - 6f, GroundY - 4f, 5f, wheel);
    }

    private static void DrawCowSprite(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.Collider.Width / 2f, ey = GroundY - e.Collider.Height;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(ex, ey + 4f, e.Collider.Width, e.Collider.Height - 8f), 5f, 5f, p);
        using var head = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(ex - 8f, ey + 6f, 14f, 10f), 4f, 4f, head);
        using var leg = new SKPaint { Color = new SKColor(0xDD, 0xDD, 0xCC), StrokeWidth = 3f, IsAntialias = true };
        canvas.DrawLine(ex + 4f, ey + e.Collider.Height - 4f, ex + 4f, GroundY, leg);
        canvas.DrawLine(ex + e.Collider.Width - 4f, ey + e.Collider.Height - 4f, ex + e.Collider.Width - 4f, GroundY, leg);
        using var spot = new SKPaint { Color = new SKColor(0x88, 0x88, 0x77), IsAntialias = true };
        canvas.DrawCircle(e.X, ey + 8f, 3f, spot);
        DrawHelper.DrawCenteredText(canvas, "MOO", 10f, new SKColor(0x55, 0x44, 0x22), e.X, ey);
    }

    // ── Arrows ────────────────────────────────────────────────────────────

    private void DrawArrows(SKCanvas canvas)
    {
        foreach (var a in _arrows)
        {
            if (!a.Active) continue;
            float angle = MathF.Atan2(a.Rigidbody.VelocityY, a.Rigidbody.VelocityX);
            float len = 14f;
            float dx = MathF.Cos(angle) * len;
            float dy = MathF.Sin(angle) * len;
            using var ap = new SKPaint
            {
                Color = a.IsEnemy ? ColFire : ColArrow,
                StrokeWidth = a.IsEnemy ? 2.5f : 2f,
                IsAntialias = true
            };
            canvas.DrawLine(a.X - dx / 2f, a.Y - dy / 2f, a.X + dx / 2f, a.Y + dy / 2f, ap);
        }
    }

    private void DrawBoulders(SKCanvas canvas)
    {
        foreach (var b in _boulders)
        {
            if (!b.Active) continue;
            using var p = new SKPaint { Color = ColBoulder, IsAntialias = true };
            canvas.DrawCircle(b.X, b.Y, 7f, p);
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

        using var dotPaint = new SKPaint
        {
            Color = new SKColor(0xFF, 0xFF, 0xFF, 100),
            StrokeWidth = 1.5f,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash([4f, 6f], 0f)
        };
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
        canvas.DrawPath(path, dotPaint);

        float landT = FindLandingTime(ay, vy);
        if (landT > 0f)
        {
            float landX = ax + vx * landT;
            if (landX >= 0 && landX <= GameWidth)
            {
                using var dotFill = new SKPaint { Color = new SKColor(0xFF, 0xFF, 0x00, 180), IsAntialias = true };
                canvas.DrawCircle(landX, GroundY - 3f, 5f, dotFill);
            }
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
        DrawHelper.DrawText(canvas, $"Score: {state.Score}", 20f, ColHud, 8f, 26f);
        string levelStr = $"Level {state.Level}";
        float levelW = DrawHelper.MeasureText(levelStr, 20f);
        DrawHelper.DrawText(canvas, levelStr, 20f, ColHud, GameWidth - levelW - 8f, 26f);

        DrawHelper.DrawText(canvas, $"Archers: {_archerCount}", 16f, ColArcher, 8f, 50f);
        DrawHelper.DrawText(canvas, $"Workers: {_workerCount}", 16f, ColWorker, 8f, 70f);

        float barX = 8f, barY = 82f, barW = 140f, barH = 10f;
        using var barBg = new SKPaint { Color = new SKColor(0x30, 0x30, 0x30) };
        canvas.DrawRect(SKRect.Create(barX, barY, barW, barH), barBg);
        using var barFg = new SKPaint { Color = ColKeepProg };
        canvas.DrawRect(SKRect.Create(barX, barY, barW * _keepProgress, barH), barFg);
        DrawHelper.DrawText(canvas, "Keep", 11f, ColDim, barX, barY - 3f);

        DrawHelper.DrawCenteredText(canvas, $"Aim: {_aimAngle:F0}°", 15f, ColDim, GameWidth / 2f, 20f);

        if (_arrowCooldown.Active)
        {
            float r = _arrowCooldown.Remaining / ArrowCooldownTime;
            DrawHelper.DrawCenteredText(canvas, "●", 12f,
                new SKColor(0xFF, 0xFF, 0xFF, (byte)(r * 200)), GameWidth / 2f, 38f);
        }

        if (_lordActive)
        {
            float ratio = _lordHP / 10f;
            DrawHelper.DrawText(canvas, $"Lord HP: {_lordHP:F0}", 16f,
                ratio < 0.3f ? ColRed : ColGold, GameWidth - 160f, 50f);
        }

        if (_archerCount == 1 && _accuracyMult > 1)
            DrawHelper.DrawCenteredText(canvas, $"Accuracy ×{_accuracyMult}", 15f, ColGold, GameWidth / 2f, 60f);

        DrawTouchButtons(canvas);
    }

    private void DrawTouchButtons(SKCanvas canvas)
    {
        using var stripPaint = new SKPaint { Color = new SKColor(0x00, 0x00, 0x00, 140) };
        canvas.DrawRect(SKRect.Create(0, BtnY - 4f, GameWidth, BtnH + 8f), stripPaint);

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

    private static void DrawButton(SKCanvas canvas, SKRect rect, string label,
        bool enabled, SKColor labelCol, bool pressed, bool large = false)
    {
        float alpha = enabled ? 1f : 0.4f;
        byte bgA = pressed ? (byte)180 : (byte)110;
        SKColor bg = pressed ? new SKColor(0xFF, 0xFF, 0xFF, bgA)
                               : new SKColor(0x22, 0x22, 0x33, bgA);
        using var bgPaint = new SKPaint { Color = bg, IsAntialias = true };
        canvas.DrawRoundRect(rect, BtnR, BtnR, bgPaint);

        SKColor border = enabled
            ? (pressed ? SKColors.White : new SKColor(0x88, 0x88, 0xAA))
            : new SKColor(0x44, 0x44, 0x55);
        using var borderPaint = new SKPaint
        {
            Color = border.WithAlpha((byte)(200 * alpha)),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            IsAntialias = true
        };
        canvas.DrawRoundRect(rect, BtnR, BtnR, borderPaint);

        float fontSize = large ? 16f : 13f;
        SKColor col = pressed ? SKColors.Black : labelCol;
        DrawHelper.DrawCenteredText(canvas, label, fontSize, col, rect.MidX, rect.MidY + fontSize * 0.38f, alpha);
    }

    // ── Float texts ───────────────────────────────────────────────────────

    private void DrawFloatTexts(SKCanvas canvas)
    {
        foreach (var t in _texts)
        {
            float alpha = Math.Clamp(t.Life / 1.4f, 0f, 1f);
            DrawHelper.DrawCenteredText(canvas, t.Text, 16f, t.Color, t.X, t.Y, alpha);
        }
    }
}
