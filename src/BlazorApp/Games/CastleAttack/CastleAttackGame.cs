using SkiaSharp;
using SkiaSharpGames.GameEngine;

namespace SkiaSharpGames.BlazorApp.Games.CastleAttack;

// ── Enums ────────────────────────────────────────────────────────────────────

public enum CastlePhase { Start, Playing, GameOver, Victory }

public enum EnemyType { Spearman, Swordsman, Berserker, Crossbowman, Catapult, Ram, Cow }

// ── Main game engine ─────────────────────────────────────────────────────────

public class CastleAttackGame : GameScreenBase
{
    // ── Game dimensions ───────────────────────────────────────────────────────
    public const int GameWidth  = 1200;
    public const int GameHeight = 600;
    public override (int width, int height) GameDimensions => (GameWidth, GameHeight);

    // ── Layout constants ──────────────────────────────────────────────────────
    const float GroundY = 548f;      // top of the ground strip
    const float BlockW  = 40f;       // each wall block width
    const float BlockH  = 28f;       // each wall block height

    // Wall left-edge X positions (outer = rightmost, first wall enemies hit)
    const float OuterWallX  = 430f;  // 2 blocks tall
    const float MiddleWallX = 330f;  // 4 blocks tall
    const float InnerWallX  = 230f;  // 6 blocks tall

    // Keep (the dungeon being built)
    const float KeepLeft  =  10f;
    const float KeepRight = 200f;
    const float KeepFullH = 200f;    // height when fully built
    const float KeepBaseY = GroundY; // keep sits on the ground

    // Lord – defends in front of keep when all walls fall
    const float LordStandX = KeepRight + 8f;
    const float LordW = 14f;
    const float LordH = 28f;

    // Enemy spawn point
    const float SpawnX = 1185f;

    // Arrow physics
    const float ArrowSpeed   = 580f;
    const float ArrowGravity = 480f;  // px / s²

    // Keep build speed per worker (fraction of total per second)
    const float WorkerBuildRate = 1f / 110f;  // 110 s with 1 worker

    // Lord melee range
    const float LordMeleeRange  = 30f;
    const float LordAttackDamage = 2f;
    const float LordAttackInterval = 0.6f;

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly SKColor ColSky        = new(0x1A, 0x1A, 0x30);
    static readonly SKColor ColHorizon    = new(0x3A, 0x2E, 0x20);
    static readonly SKColor ColGround     = new(0x55, 0x44, 0x22);
    static readonly SKColor ColGroundEdge = new(0x44, 0x34, 0x16);
    static readonly SKColor ColStone      = new(0x88, 0x88, 0x88);
    static readonly SKColor ColStoneDmg   = new(0xBB, 0x66, 0x44);
    static readonly SKColor ColStoneLow   = new(0xCC, 0x55, 0x33);
    static readonly SKColor ColArcher     = new(0x2E, 0x7D, 0x32);
    static readonly SKColor ColWorker     = new(0xF5, 0xA5, 0x23);
    static readonly SKColor ColLord       = new(0xFF, 0xD7, 0x00);
    static readonly SKColor ColArrow      = new(0x8B, 0x4B, 0x13);
    static readonly SKColor ColKeepBase   = new(0x4A, 0x35, 0x28);
    static readonly SKColor ColKeepDone   = new(0x6B, 0x55, 0x33);
    static readonly SKColor ColKeepProg   = new(0x22, 0x88, 0x44);
    static readonly SKColor ColHud        = SKColors.White;
    static readonly SKColor ColDim        = new(0xAA, 0xAA, 0xAA);
    static readonly SKColor ColOil        = new(0x6B, 0x4A, 0x10);
    static readonly SKColor ColFire       = new(0xFF, 0x6B, 0x00);
    static readonly SKColor ColBoulder    = new(0x77, 0x77, 0x77);
    static readonly SKColor ColAccent     = new(0x00, 0xD4, 0xFF);
    static readonly SKColor ColGold       = new(0xFF, 0xD7, 0x00);
    static readonly SKColor ColRed        = new(0xFF, 0x2D, 0x55);

    static SKColor EnemyCol(EnemyType t) => t switch
    {
        EnemyType.Spearman    => new(0xE7, 0x4C, 0x3C),
        EnemyType.Swordsman   => new(0x8E, 0x44, 0xAD),
        EnemyType.Berserker   => new(0xE6, 0x7E, 0x22),
        EnemyType.Crossbowman => new(0x27, 0xAE, 0x60),
        EnemyType.Catapult    => new(0x95, 0x7C, 0x55),
        EnemyType.Ram         => new(0x6D, 0x5A, 0x3C),
        EnemyType.Cow         => new(0xF5, 0xF5, 0xDC),
        _                     => SKColors.Gray
    };

    // ── Nested data types ─────────────────────────────────────────────────────

    private sealed class WallBlock
    {
        public float HP    { get; set; }
        public float MaxHP { get; } = 100f;
        public bool  Active => HP > 0f;
        public WallBlock() { HP = MaxHP; }
    }

    private sealed class Wall
    {
        public float CenterX    { get; }
        public float LeftX      => CenterX - BlockW / 2f;
        public List<WallBlock> Blocks { get; } = [];
        public bool  HasArcher  { get; set; }
        public int   TotalBlocks { get; }

        public int   ActiveBlocks   => Blocks.Count(b => b.Active);
        public bool  IsDestroyed    => Blocks.All(b => !b.Active);

        // Top Y of the current wall surface (where archer stands)
        public float TopY => IsDestroyed ? GroundY : GroundY - ActiveBlocks * BlockH;
        public float ArcherCenterX => CenterX;
        public float ArcherBaseY   => TopY;   // archer feet

        public Wall(float cx, int blocks, bool hasArcher)
        {
            CenterX     = cx;
            TotalBlocks = blocks;
            HasArcher   = hasArcher;
            for (int i = 0; i < blocks; i++)
                Blocks.Add(new WallBlock());
        }

        // Damage the bottom-most active block
        public void TakeDamage(float dmg)
        {
            var block = Blocks.LastOrDefault(b => b.Active);
            if (block != null) block.HP = Math.Max(0f, block.HP - dmg);
        }

        // Instantly destroy every block (ram hit)
        public void Demolish()
        {
            foreach (var b in Blocks) b.HP = 0f;
            HasArcher = false;
        }
    }

    private enum EnemyState { Walking, AttackingWall, AttackingLord, Shooting, Idle }

    private sealed class Enemy
    {
        public EnemyType  Type;
        public float      X;
        public float      HP, MaxHP;
        public float      Speed;
        public EnemyState State   = EnemyState.Walking;
        public float      AttackTimer;
        public float      AttackInterval;
        public float      AttackDamage;
        public float      AttackRange;  // how far from wall to stop and attack
        public bool       Active  = true;
        public float      W, H;         // visual size
        public int        TargetWallIdx = -1; // cached target wall index
        // Crossbowman
        public float      FireCooldown;
        public float      FireInterval = 2.5f;
    }

    private sealed class Arrow
    {
        public float X, Y;
        public float VX, VY;
        public bool  Active   = true;
        public bool  IsEnemy  = false;   // crossbowman bolt
        public int   EnemyTargetWall;    // which archer it aims at
    }

    private sealed class Boulder
    {
        public float X, Y;
        public float VX, VY;
        public bool  Active = true;
        public int   TargetWallIdx;
    }

    private sealed class FloatText
    {
        public float    X, Y;
        public float    Life;
        public string   Text  = "";
        public SKColor  Color = SKColors.White;
    }

    // ── State fields ──────────────────────────────────────────────────────────

    public CastlePhase Phase   { get; private set; } = CastlePhase.Start;
    public int         Score   { get; private set; }
    public int         Level   { get; private set; } = 1;

    private readonly List<Wall>      _walls    = [];
    private readonly List<Enemy>     _enemies  = [];
    private readonly List<Arrow>     _arrows   = [];
    private readonly List<Boulder>   _boulders = [];
    private readonly List<FloatText> _texts    = [];

    // Archer / worker counts
    private int   _archerCount = 3;
    private int   _workerCount = 3;

    // Keep construction
    private float _keepProgress = 0f;   // 0..1

    // Lord
    private float _lordHP     = 10f;
    private bool  _lordActive = false;
    private float _lordAttackTimer;

    // Aiming
    private float _aimAngle = 18f;     // degrees above horizontal
    private const float AimMin   = -8f;
    private const float AimMax   = 62f;
    private const float AimSpeed = 50f;

    // Arrow volley cooldown
    private float _arrowCooldown;
    private const float ArrowCooldownTime = 0.75f;

    // Special weapons
    private bool _oilAvail      = true;
    private bool _mangonelAvail = true;
    private bool _logsAvail     = true;

    // Input state
    private bool _leftHeld, _rightHeld;

    // Enemy spawning
    private float _spawnTimer    = 3.5f;
    private float _spawnInterval = 5f;
    private float _levelTime;

    // Cow
    private bool  _cowSpawned;
    private float _cowSpawnTime = 25f;

    // Accuracy scoring
    private int   _accuracyMult     = 1;
    private int   _consecutiveHits;
    private float _accuracyResetTimer;

    // Damage flash timers per wall
    private float[] _wallFlash = new float[3];

    // ── Initialisation ────────────────────────────────────────────────────────

    public CastleAttackGame() => InitWalls();

    private void InitWalls()
    {
        _walls.Clear();
        _walls.Add(new Wall(InnerWallX  + BlockW / 2f, 6, hasArcher: true));   // [0] inner
        _walls.Add(new Wall(MiddleWallX + BlockW / 2f, 4, hasArcher: true));   // [1] middle
        _walls.Add(new Wall(OuterWallX  + BlockW / 2f, 2, hasArcher: true));   // [2] outer
    }

    public void StartGame()
    {
        Phase         = CastlePhase.Playing;
        Score         = 0;
        Level         = 1;
        _archerCount  = 3;
        _workerCount  = 3;
        _keepProgress = 0f;
        _lordHP       = 10f;
        _lordActive   = false;
        _lordAttackTimer = 0f;
        _oilAvail     = true;
        _mangonelAvail = true;
        _logsAvail    = true;
        _leftHeld     = false;
        _rightHeld    = false;
        _arrowCooldown = 0f;
        _aimAngle     = 18f;
        _spawnTimer   = 3.5f;
        _spawnInterval = 5f;
        _levelTime    = 0f;
        _cowSpawned   = false;
        _cowSpawnTime = 25f;
        _accuracyMult = 1;
        _consecutiveHits = 0;
        _accuracyResetTimer = 0f;
        _wallFlash    = new float[3];
        _enemies.Clear();
        _arrows.Clear();
        _boulders.Clear();
        _texts.Clear();
        InitWalls();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    public override void OnPointerDown(float x, float y)
    {
        if (IsTransitioning) return;
        if (Phase is CastlePhase.Start or CastlePhase.GameOver or CastlePhase.Victory)
            BeginTransition(new FadeTransition(), 0.35f, StartGame);
    }

    public override void OnKeyDown(string key)
    {
        if (Phase != CastlePhase.Playing) return;

        switch (key)
        {
            case "ArrowLeft":  _leftHeld  = true;  break;
            case "ArrowRight": _rightHeld = true;  break;
            case " ":          FireVolley();        break;
            case "ArrowDown":  ConvertArcherToWorker(); break;
            case "ArrowUp":    ConvertWorkerToArcher(); break;
            case "z": case "Z": UseOil();      break;
            case "x": case "X": UseMangonel(); break;
            case "c": case "C": UseLogs();     break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft":  _leftHeld  = false; break;
            case "ArrowRight": _rightHeld = false; break;
        }
    }

    // ── Archer ↔ Worker conversion ────────────────────────────────────────────

    private void ConvertArcherToWorker()
    {
        if (_workerCount <= 1) return; // must always keep at least 1 worker — need ≥2 to convert one
        if (_archerCount <= 0) return;

        // Remove archer from the outermost (rightmost) wall that has one
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
        if (_workerCount <= 1) return; // must keep at least 1 worker
        if (_archerCount >= _walls.Count) return; // no free wall slot

        // Place archer on the outermost (rightmost) non-destroyed wall without an archer
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

    // ── Special weapons ───────────────────────────────────────────────────────

    private void UseOil()
    {
        if (!_oilAvail) return;
        _oilAvail = false;
        // Kill all enemies under the walls (up to OuterWallX + 60)
        float maxRange = OuterWallX + BlockW + 80f;
        foreach (var e in _enemies)
        {
            if (!e.Active || e.X > maxRange) continue;
            if (e.Type is EnemyType.Catapult or EnemyType.Ram) continue; // no effect on siege equipment
            KillEnemy(e, 0f, 0f);
        }
        SpawnText("BOILING OIL!", GameWidth / 2f, 120f, ColOil);
    }

    private void UseMangonel()
    {
        if (!_mangonelAvail) return;
        _mangonelAvail = false;
        // Kill enemies furthest from castle (rightmost third of screen)
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
        // Kill all enemies on screen
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            KillEnemy(e, 0f, 0f);
        }
        SpawnText("FLAMING LOGS!", GameWidth / 2f, 120f, ColFire);
    }

    // ── Arrow firing ──────────────────────────────────────────────────────────

    private void FireVolley()
    {
        if (_arrowCooldown > 0f) return;
        if (_archerCount == 0) return;
        _arrowCooldown = ArrowCooldownTime;

        float rad = _aimAngle * MathF.PI / 180f;
        float vx  =  ArrowSpeed * MathF.Cos(rad);
        float vy  = -ArrowSpeed * MathF.Sin(rad);

        foreach (var wall in _walls)
        {
            if (!wall.HasArcher || wall.IsDestroyed) continue;
            float ax = wall.ArcherCenterX;
            float ay = wall.ArcherBaseY - ArcherH / 2f;
            _arrows.Add(new Arrow { X = ax, Y = ay, VX = vx, VY = vy });
        }
    }

    const float ArcherH = 26f;
    const float ArcherW = 12f;
    const float WorkerH = 22f;
    const float WorkerW = 10f;

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update(float deltaTime)
    {
        UpdateTransition(deltaTime);

        if (Phase != CastlePhase.Playing) return;

        _levelTime    += deltaTime;
        _arrowCooldown = Math.Max(0f, _arrowCooldown - deltaTime);

        // Aim angle from held keys
        if (_leftHeld)  _aimAngle = Math.Max(AimMin, _aimAngle - AimSpeed * deltaTime);
        if (_rightHeld) _aimAngle = Math.Min(AimMax, _aimAngle + AimSpeed * deltaTime);

        // Accuracy multiplier reset timer
        if (_accuracyResetTimer > 0f)
        {
            _accuracyResetTimer -= deltaTime;
            if (_accuracyResetTimer <= 0f)
            {
                _accuracyMult = 1;
                _consecutiveHits = 0;
            }
        }

        // Keep construction by workers
        if (_workerCount > 0 && _keepProgress < 1f)
        {
            _keepProgress = Math.Min(1f, _keepProgress + _workerCount * WorkerBuildRate * deltaTime);
            if (_keepProgress >= 1f && !IsTransitioning)
                BeginTransition(new FadeTransition(), 0.5f, () => Phase = CastlePhase.Victory);
        }

        // Cow spawn
        if (!_cowSpawned && _levelTime >= _cowSpawnTime)
        {
            SpawnCow();
            _cowSpawned = true;
        }

        // Enemy spawning
        UpdateSpawning(deltaTime);

        // Wall flash timers
        for (int i = 0; i < _wallFlash.Length; i++)
            _wallFlash[i] = Math.Max(0f, _wallFlash[i] - deltaTime);

        // Update enemies
        UpdateEnemies(deltaTime);

        // Check if all walls destroyed → activate Lord
        if (!_lordActive && _walls.All(w => w.IsDestroyed))
        {
            _lordActive = true;
            SpawnText("THE LORD DEFENDS!", GameWidth / 2f, 200f, ColGold);
        }

        // Lord melee attack
        if (_lordActive)
            UpdateLord(deltaTime);

        // Update arrows
        UpdateArrows(deltaTime);

        // Update boulders (catapult projectiles)
        UpdateBoulders(deltaTime);

        // Float texts
        for (int i = _texts.Count - 1; i >= 0; i--)
        {
            _texts[i].Life -= deltaTime;
            _texts[i].Y    -= 28f * deltaTime;
            if (_texts[i].Life <= 0f) _texts.RemoveAt(i);
        }
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    private void UpdateSpawning(float deltaTime)
    {
        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f) return;

        // Decrease interval over time (increase difficulty)
        _spawnInterval = Math.Max(1.5f, 5f - _levelTime * 0.02f);
        _spawnTimer = _spawnInterval + Random.Shared.NextSingle() * 1.5f;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        float t = _levelTime;
        // Probability distribution shifts as time passes
        EnemyType type;
        double r = Random.Shared.NextDouble();

        if (t < 15f)
        {
            type = r < 0.7 ? EnemyType.Spearman : EnemyType.Swordsman;
        }
        else if (t < 35f)
        {
            type = r < 0.4 ? EnemyType.Spearman
                 : r < 0.7 ? EnemyType.Swordsman
                 :            EnemyType.Berserker;
        }
        else if (t < 60f)
        {
            type = r < 0.25 ? EnemyType.Spearman
                 : r < 0.45 ? EnemyType.Swordsman
                 : r < 0.65 ? EnemyType.Berserker
                 : r < 0.80 ? EnemyType.Crossbowman
                 :             EnemyType.Catapult;
        }
        else
        {
            type = r < 0.15 ? EnemyType.Spearman
                 : r < 0.30 ? EnemyType.Swordsman
                 : r < 0.50 ? EnemyType.Berserker
                 : r < 0.65 ? EnemyType.Crossbowman
                 : r < 0.78 ? EnemyType.Catapult
                 :             EnemyType.Ram;
        }

        var e = CreateEnemy(type);
        _enemies.Add(e);
    }

    private void SpawnCow()
    {
        var cow = CreateEnemy(EnemyType.Cow);
        _enemies.Add(cow);
    }

    private static Enemy CreateEnemy(EnemyType type) => type switch
    {
        EnemyType.Spearman => new Enemy
        {
            Type = type, X = SpawnX, HP = 1f, MaxHP = 1f,
            Speed = 95f, W = 12f, H = 26f,
            AttackInterval = 0.5f, AttackDamage = 8f, AttackRange = BlockW + 2f
        },
        EnemyType.Swordsman => new Enemy
        {
            Type = type, X = SpawnX, HP = 4f, MaxHP = 4f,
            Speed = 45f, W = 14f, H = 28f,
            AttackInterval = 1.0f, AttackDamage = 12f, AttackRange = BlockW + 2f
        },
        EnemyType.Berserker => new Enemy
        {
            Type = type, X = SpawnX, HP = 2f, MaxHP = 2f,
            Speed = 110f, W = 13f, H = 24f,
            AttackInterval = 0.4f, AttackDamage = 10f, AttackRange = BlockW + 2f
        },
        EnemyType.Crossbowman => new Enemy
        {
            Type = type, X = SpawnX, HP = 1f, MaxHP = 1f,
            Speed = 65f, W = 12f, H = 26f,
            AttackInterval = 0f, AttackDamage = 0f, AttackRange = 250f,
            FireInterval = 3f
        },
        EnemyType.Catapult => new Enemy
        {
            Type = type, X = SpawnX, HP = 5f, MaxHP = 5f,
            Speed = 35f, W = 36f, H = 26f,
            AttackInterval = 4f, AttackDamage = 0f, AttackRange = 180f
        },
        EnemyType.Ram => new Enemy
        {
            Type = type, X = SpawnX, HP = 10f, MaxHP = 10f,
            Speed = 28f, W = 40f, H = 22f,
            AttackInterval = 0f, AttackDamage = 0f, AttackRange = 0f
        },
        EnemyType.Cow => new Enemy
        {
            Type = type, X = SpawnX, HP = 1f, MaxHP = 1f,
            Speed = 55f, W = 28f, H = 20f,
            AttackInterval = 0f, AttackDamage = 0f, AttackRange = 0f,
            FireCooldown = 0f
        },
        _ => new Enemy { Type = type, X = SpawnX, HP = 1f, MaxHP = 1f, Speed = 50f, W = 12f, H = 24f }
    };

    // ── Enemy update ──────────────────────────────────────────────────────────

    private void UpdateEnemies(float deltaTime)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var e = _enemies[i];
            if (!e.Active)
            {
                _enemies.RemoveAt(i);
                continue;
            }

            UpdateEnemy(e, deltaTime);
        }
    }

    private void UpdateEnemy(Enemy e, float dt)
    {
        // Determine target: find the rightmost (outer) wall that still blocks this enemy
        int tgtWall = FindTargetWall(e.X);

        switch (e.Type)
        {
            case EnemyType.Cow:
                e.X -= e.Speed * dt;
                if (e.X < -e.W) e.Active = false;
                return;

            case EnemyType.Ram:
                UpdateRam(e, dt, tgtWall);
                return;

            case EnemyType.Catapult:
                UpdateCatapult(e, dt, tgtWall);
                return;

            case EnemyType.Crossbowman:
                UpdateCrossbowman(e, dt, tgtWall);
                return;

            default:
                UpdateMeleeEnemy(e, dt, tgtWall);
                return;
        }
    }

    // Returns index into _walls of the rightmost active wall that is to the LEFT of enemy X.
    // Returns -1 if all walls are destroyed (enemy goes for Lord/keep).
    private int FindTargetWall(float enemyX)
    {
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            if (!_walls[i].IsDestroyed)
                return i;
        }
        return -1;
    }

    private void UpdateMeleeEnemy(Enemy e, float dt, int tgtWall)
    {
        if (tgtWall < 0)
        {
            // All walls down – head for Lord / keep
            float targetX = _lordActive ? LordStandX : KeepRight;
            if (e.X - e.W / 2f > targetX + LordW + 5f)
            {
                e.State = EnemyState.Walking;
                e.X -= e.Speed * dt;
            }
            else
            {
                e.State = EnemyState.AttackingLord;
                e.AttackTimer += dt;
                if (e.AttackTimer >= e.AttackInterval)
                {
                    e.AttackTimer = 0f;
                    _lordHP -= e.AttackDamage * 0.5f;
                    if (_lordHP <= 0f && !IsTransitioning)
                        BeginTransition(new FadeTransition(), 0.5f, () => Phase = CastlePhase.GameOver);
                }
            }
            return;
        }

        var wall = _walls[tgtWall];
        float wallFace = wall.LeftX - 2f; // right face of wall the enemy walks toward

        if (e.X - e.W / 2f > wallFace + e.AttackRange)
        {
            // Walk toward wall
            e.State = EnemyState.Walking;
            e.X -= e.Speed * dt;
        }
        else
        {
            // Attack the wall
            e.State = EnemyState.AttackingWall;
            e.AttackTimer += dt;
            if (e.AttackTimer >= e.AttackInterval)
            {
                e.AttackTimer = 0f;
                wall.TakeDamage(e.AttackDamage);
                if (tgtWall < _wallFlash.Length)
                    _wallFlash[tgtWall] = 0.15f;
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
            // Walk toward keep
            if (e.X - e.W / 2f > KeepRight + 5f)
                e.X -= e.Speed * dt;
            else
            {
                _lordHP -= 5f * dt;
                if (_lordHP <= 0f && !IsTransitioning)
                    BeginTransition(new FadeTransition(), 0.5f, () => Phase = CastlePhase.GameOver);
            }
            return;
        }

        var wall = _walls[tgtWall];
        float wallFace = wall.LeftX;

        if (e.X - e.W / 2f > wallFace + 1f)
        {
            e.State = EnemyState.Walking;
            e.X -= e.Speed * dt;
        }
        else
        {
            // Demolish the wall instantly
            wall.Demolish();
            if (tgtWall < _wallFlash.Length) _wallFlash[tgtWall] = 0.3f;
            UpdateArcherCount();
            SpawnText("WALL BREACHED!", wall.CenterX, wall.TopY - 50f, ColRed);
            // Ram keeps moving
            e.X -= e.Speed * dt * 0.5f;
        }
    }

    private void UpdateCatapult(Enemy e, float dt, int tgtWall)
    {
        if (tgtWall < 0)
        {
            e.X -= e.Speed * dt;
            return;
        }

        var wall = _walls[tgtWall];
        float stopX = wall.LeftX + BlockW + e.AttackRange + e.W / 2f;

        if (e.X > stopX)
        {
            e.State = EnemyState.Walking;
            e.X -= e.Speed * dt;
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
        // Find the rightmost archer to target
        int archerWallIdx = -1;
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            if (!_walls[i].IsDestroyed && _walls[i].HasArcher)
            { archerWallIdx = i; break; }
        }

        if (archerWallIdx < 0)
        {
            // No archers — just walk and attack Lord/keep like melee
            UpdateMeleeEnemy(e, dt, tgtWall);
            return;
        }

        var targetWall = _walls[archerWallIdx];
        float stopX = targetWall.LeftX - e.AttackRange;

        if (e.X > stopX)
        {
            e.State = EnemyState.Walking;
            e.X -= e.Speed * dt;
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
        float tx = wall.CenterX;
        float ty = wall.TopY;
        float dx = tx - catapult.X;
        float dy = ty - (GroundY - 30f); // approx catapult height

        // Compute launch velocity to arc to target (fix T based on horizontal speed)
        float T  = Math.Abs(dx) / 260f;
        float vx = dx / T;
        float vy = (dy - 0.5f * ArrowGravity * T * T) / T;

        _boulders.Add(new Boulder
        {
            X = catapult.X, Y = GroundY - 30f,
            VX = vx, VY = vy,
            TargetWallIdx = targetWallIdx
        });
    }

    private void FireCrossbowBolt(Enemy crossbow, int archerWallIdx)
    {
        var wall  = _walls[archerWallIdx];
        float ay = wall.ArcherBaseY - ArcherH / 2f;
        _arrows.Add(new Arrow
        {
            X = crossbow.X - crossbow.W / 2f,
            Y = ay,
            VX = -320f,
            VY = 0f,
            IsEnemy = true,
            EnemyTargetWall = archerWallIdx
        });
    }

    // ── Lord ──────────────────────────────────────────────────────────────────

    private void UpdateLord(float dt)
    {
        _lordAttackTimer += dt;
        if (_lordAttackTimer < LordAttackInterval) return;
        _lordAttackTimer = 0f;

        // Attack the nearest enemy
        Enemy? target = null;
        float  minDist = float.MaxValue;
        foreach (var e in _enemies)
        {
            if (!e.Active || e.Type == EnemyType.Cow) continue;
            float dx = e.X - LordStandX;
            float dist = MathF.Abs(dx);
            if (dist < minDist && dist < LordMeleeRange + 60f)
            {
                minDist = dist;
                target  = e;
            }
        }

        if (target != null)
        {
            target.HP -= LordAttackDamage;
            if (target.HP <= 0f) KillEnemy(target, target.X, GroundY - target.H);
        }
    }

    // ── Arrow & boulder update ────────────────────────────────────────────────

    private void UpdateArrows(float dt)
    {
        for (int i = _arrows.Count - 1; i >= 0; i--)
        {
            var a = _arrows[i];
            if (!a.Active) { _arrows.RemoveAt(i); continue; }

            a.VY += ArrowGravity * dt;
            a.X  += a.VX * dt;
            a.Y  += a.VY * dt;

            // Out of bounds
            if (a.X > GameWidth + 20f || a.X < -20f || a.Y > GroundY + 20f)
            {
                if (!a.IsEnemy)
                {
                    // Missed — reset accuracy
                    _consecutiveHits    = 0;
                    _accuracyMult       = 1;
                    _accuracyResetTimer = 0f;
                }
                a.Active = false;
                _arrows.RemoveAt(i);
                continue;
            }

            if (a.IsEnemy)
            {
                // Enemy bolt targeting an archer
                CheckBoltHitsArcher(a);
            }
            else
            {
                // Player arrow — check against enemies
                CheckArrowHitsEnemy(a);
            }
        }
    }

    private void CheckArrowHitsEnemy(Arrow a)
    {
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            float ex = e.X - e.W / 2f;
            float ey = GroundY - e.H;
            if (a.X >= ex && a.X <= ex + e.W && a.Y >= ey && a.Y <= GroundY)
            {
                // Hit!
                float pts = PointsForEnemy(e.Type);
                e.HP -= 1f;

                // Accuracy bonus (only if single archer on wall)
                if (_archerCount == 1)
                {
                    _consecutiveHits++;
                    _accuracyResetTimer = 3f;
                    if (_consecutiveHits % 3 == 0)
                        _accuracyMult = Math.Min(8, _accuracyMult + 1);
                    pts += 25f * _accuracyMult;
                }

                if (e.HP <= 0f)
                    KillEnemy(e, a.X, a.Y);
                else
                    SpawnText($"-{1}", e.X, GroundY - e.H - 5f, ColDim);

                a.Active = false;
                return;
            }
        }
    }

    private void CheckBoltHitsArcher(Arrow bolt)
    {
        int idx = bolt.EnemyTargetWall;
        if (idx < 0 || idx >= _walls.Count) { bolt.Active = false; return; }
        var wall = _walls[idx];

        if (!wall.HasArcher || wall.IsDestroyed) { bolt.Active = false; return; }

        float ax = wall.ArcherCenterX;
        float ay = wall.ArcherBaseY - ArcherH;

        if (MathF.Abs(bolt.X - ax) < ArcherW && bolt.Y >= ay && bolt.Y <= ay + ArcherH)
        {
            // Archer killed
            wall.HasArcher = false;
            _archerCount   = Math.Max(0, _archerCount - 1);
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

            b.VY += ArrowGravity * dt;
            b.X  += b.VX * dt;
            b.Y  += b.VY * dt;

            if (b.X < -40f || b.X > GameWidth + 40f || b.Y > GroundY + 20f)
            { b.Active = false; _boulders.RemoveAt(i); continue; }

            // Check hit on target wall
            if (b.TargetWallIdx >= 0 && b.TargetWallIdx < _walls.Count)
            {
                var wall = _walls[b.TargetWallIdx];
                float lx = wall.LeftX;
                if (!wall.IsDestroyed && b.X >= lx - 10f && b.X <= lx + BlockW + 10f)
                {
                    wall.TakeDamage(50f);
                    if (b.TargetWallIdx < _wallFlash.Length)
                        _wallFlash[b.TargetWallIdx] = 0.2f;
                    if (wall.IsDestroyed)
                    {
                        wall.HasArcher = false;
                        UpdateArcherCount();
                    }
                    b.Active = false;
                    _boulders.RemoveAt(i);
                }
            }
        }
    }

    // ── Kill & score helpers ──────────────────────────────────────────────────

    private void KillEnemy(Enemy e, float x, float y)
    {
        if (!e.Active) return;
        e.Active = false;

        int pts = PointsForEnemy(e.Type);
        Score += pts;
        SpawnText($"+{pts}", x > 0 ? x : e.X, GroundY - e.H - 10f,
            e.Type == EnemyType.Cow ? ColGold : ColAccent);
    }

    private static int PointsForEnemy(EnemyType t) => t switch
    {
        EnemyType.Spearman    => 100,
        EnemyType.Swordsman   => 200,
        EnemyType.Berserker   => 150,
        EnemyType.Crossbowman => 250,
        EnemyType.Catapult    => 300,
        EnemyType.Ram         => 500,
        EnemyType.Cow         => 3000,
        _                     => 50
    };

    private void UpdateArcherCount()
    {
        _archerCount = _walls.Count(w => w.HasArcher && !w.IsDestroyed);
    }

    private void SpawnText(string text, float x, float y, SKColor color)
    {
        _texts.Add(new FloatText { Text = text, X = x, Y = y, Life = 1.4f, Color = color });
    }

    // ── Draw ──────────────────────────────────────────────────────────────────

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        float scale   = MathF.Min(width / (float)GameWidth, height / (float)GameHeight);
        float offsetX = (width  - GameWidth  * scale) / 2f;
        float offsetY = (height - GameHeight * scale) / 2f;

        canvas.Clear(ColSky);

        canvas.Save();
        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);

        switch (Phase)
        {
            case CastlePhase.Start:
                DrawBackground(canvas);
                DrawStartScreen(canvas);
                break;
            case CastlePhase.Playing:
                DrawGame(canvas);
                break;
            case CastlePhase.GameOver:
                DrawGame(canvas);
                DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight, 0.75f);
                DrawHelper.DrawCenteredText(canvas, "DEFEAT", 72f, ColRed, GameWidth / 2f, 250f);
                DrawHelper.DrawCenteredText(canvas, $"Score: {Score}", 32f, ColHud, GameWidth / 2f, 315f);
                DrawHelper.DrawCenteredText(canvas, "Click or Tap to Try Again", 22f, ColAccent, GameWidth / 2f, 370f);
                break;
            case CastlePhase.Victory:
                DrawGame(canvas);
                DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight, 0.75f);
                DrawHelper.DrawCenteredText(canvas, "VICTORY!", 72f, ColGold, GameWidth / 2f, 250f);
                DrawHelper.DrawCenteredText(canvas, $"Score: {Score}", 32f, ColHud, GameWidth / 2f, 315f);
                DrawHelper.DrawCenteredText(canvas, "The keep is complete!", 22f, ColAccent, GameWidth / 2f, 360f);
                DrawHelper.DrawCenteredText(canvas, "Click or Tap to Play Again", 22f, ColDim, GameWidth / 2f, 395f);
                break;
        }

        DrawTransitionOverlay(canvas);
        canvas.Restore();
    }

    private void DrawGame(SKCanvas canvas)
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

    // ── Background ────────────────────────────────────────────────────────────

    private void DrawBackground(SKCanvas canvas)
    {
        // Sky gradient
        using var skyShader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, GroundY),
            [ColSky, ColHorizon], [0f, 1f], SKShaderTileMode.Clamp);
        using var skyPaint = new SKPaint { Shader = skyShader };
        canvas.DrawRect(SKRect.Create(0, 0, GameWidth, GroundY), skyPaint);

        // Ground
        using var gp = new SKPaint { Color = ColGround };
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, GameHeight - GroundY), gp);
        using var gep = new SKPaint { Color = ColGroundEdge };
        canvas.DrawRect(SKRect.Create(0, GroundY, GameWidth, 4f), gep);

        // Distant hills silhouette
        using var hillPaint = new SKPaint { Color = new SKColor(0x2A, 0x20, 0x12), IsAntialias = true };
        using var hillPath = new SKPath();
        hillPath.MoveTo(0, GroundY);
        hillPath.CubicTo(200, GroundY - 60, 400, GroundY - 80, 600, GroundY - 40);
        hillPath.CubicTo(800, GroundY,      900, GroundY - 50, 1100, GroundY - 30);
        hillPath.LineTo(GameWidth, GroundY);
        hillPath.LineTo(0, GroundY);
        hillPath.Close();
        canvas.DrawPath(hillPath, hillPaint);
    }

    // ── Keep ──────────────────────────────────────────────────────────────────

    private void DrawKeep(SKCanvas canvas)
    {
        float builtH = KeepFullH * _keepProgress;
        float builtY = KeepBaseY - builtH;
        float keepW  = KeepRight - KeepLeft;

        // Foundation outline
        using var foundPaint = new SKPaint { Color = ColKeepBase, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(KeepLeft, KeepBaseY - KeepFullH, keepW, KeepFullH), foundPaint);

        // Filled progress
        if (builtH > 0f)
        {
            using var fillPaint = new SKPaint { Color = ColKeepDone };
            canvas.DrawRect(SKRect.Create(KeepLeft, builtY, keepW, builtH), fillPaint);
        }

        // Border
        using var borderPaint = new SKPaint { Color = ColStoneDmg, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
        canvas.DrawRect(SKRect.Create(KeepLeft, KeepBaseY - KeepFullH, keepW, KeepFullH), borderPaint);

        // Battlements
        float merlonW = 14f, merlonH = 14f, gap = 4f;
        float startX = KeepLeft + 4f;
        float topY   = KeepBaseY - KeepFullH;
        using var mPaint = new SKPaint { Color = ColKeepBase };
        while (startX + merlonW <= KeepRight - 4f)
        {
            canvas.DrawRect(SKRect.Create(startX, topY - merlonH, merlonW, merlonH), mPaint);
            startX += merlonW + gap;
        }

        // Keep label
        if (_keepProgress < 1f)
        {
            string pct = $"Keep {(int)(_keepProgress * 100)}%";
            DrawHelper.DrawCenteredText(canvas, pct, 13f, ColHud, (KeepLeft + KeepRight) / 2f, KeepBaseY - KeepFullH - 20f);
        }
        else
        {
            DrawHelper.DrawCenteredText(canvas, "COMPLETE!", 13f, ColGold, (KeepLeft + KeepRight) / 2f, KeepBaseY - KeepFullH - 20f);
        }
    }

    // ── Walls ─────────────────────────────────────────────────────────────────

    private void DrawWalls(SKCanvas canvas)
    {
        for (int wi = 0; wi < _walls.Count; wi++)
        {
            var wall = _walls[wi];
            float flash = wi < _wallFlash.Length ? _wallFlash[wi] : 0f;

            // Draw each remaining block from bottom to top
            int blockIdx = wall.Blocks.Count - 1;
            float blockY = GroundY - BlockH;
            for (int bi = wall.Blocks.Count - 1; bi >= 0; bi--)
            {
                var block = wall.Blocks[bi];
                if (!block.Active) continue;

                float ratio  = block.HP / block.MaxHP;
                SKColor col  = flash > 0f ? ColStoneDmg
                             : ratio < 0.3f ? ColStoneLow
                             : ratio < 0.6f ? ColStoneDmg
                             : ColStone;

                float bx = wall.LeftX;

                // Block fill
                using var bp = new SKPaint { Color = col, IsAntialias = true };
                canvas.DrawRoundRect(SKRect.Create(bx, blockY, BlockW, BlockH), 3f, 3f, bp);

                // Crack lines for damage
                if (ratio < 0.6f)
                {
                    using var cp = new SKPaint
                    {
                        Color = new SKColor(0x00, 0x00, 0x00, 80),
                        StrokeWidth = 1.5f,
                        IsAntialias = true
                    };
                    canvas.DrawLine(bx + 8f, blockY + 4f, bx + 18f, blockY + BlockH - 4f, cp);
                }

                // Highlight shine
                using var shinePaint = new SKPaint
                {
                    Color = SKColors.White.WithAlpha(40),
                    IsAntialias = true
                };
                canvas.DrawRect(SKRect.Create(bx + 2f, blockY + 2f, BlockW - 4f, BlockH / 2f - 2f), shinePaint);

                blockY -= BlockH;
            }

            // Archer on top
            if (wall.HasArcher && !wall.IsDestroyed)
            {
                float ax = wall.ArcherCenterX;
                float ay = wall.ArcherBaseY;
                DrawArcher(canvas, ax, ay);
            }
        }
    }

    private static void DrawArcher(SKCanvas canvas, float cx, float baseY)
    {
        // Body
        using var bp = new SKPaint { Color = ColArcher, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(cx - ArcherW / 2f, baseY - ArcherH, ArcherW, ArcherH - 8f), bp);
        // Head
        canvas.DrawCircle(cx, baseY - ArcherH - 5f, 6f, bp);
        // Bow
        using var bowPaint = new SKPaint { Color = ColArrow, StrokeWidth = 2f, IsAntialias = true };
        canvas.DrawLine(cx + ArcherW / 2f, baseY - ArcherH + 4f, cx + ArcherW / 2f + 8f, baseY - ArcherH / 2f, bowPaint);
    }

    // ── Workers ───────────────────────────────────────────────────────────────

    private void DrawWorkers(SKCanvas canvas)
    {
        float workerAreaW = KeepRight - KeepLeft - 10f;
        float spacing     = workerAreaW / Math.Max(_workerCount, 1);
        for (int w = 0; w < _workerCount; w++)
        {
            float wx = KeepLeft + 8f + w * spacing;
            float wy = GroundY;
            DrawWorker(canvas, wx, wy);
        }
    }

    private static void DrawWorker(SKCanvas canvas, float cx, float baseY)
    {
        using var p = new SKPaint { Color = ColWorker, IsAntialias = true };
        // Body
        canvas.DrawRect(SKRect.Create(cx - WorkerW / 2f, baseY - WorkerH, WorkerW, WorkerH - 6f), p);
        // Head
        canvas.DrawCircle(cx, baseY - WorkerH - 4f, 5f, p);
        // Tool
        using var tp = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
        canvas.DrawLine(cx + WorkerW / 2f, baseY - WorkerH + 2f, cx + WorkerW / 2f + 10f, baseY - WorkerH + 10f, tp);
    }

    // ── Lord ──────────────────────────────────────────────────────────────────

    private void DrawLord(SKCanvas canvas)
    {
        if (!_lordActive) return;

        float lx = LordStandX;
        float ly = GroundY;

        // HP bar above Lord
        float barW = 40f;
        float ratio = _lordHP / 10f;
        using var barBg = new SKPaint { Color = new SKColor(0x40, 0x00, 0x00) };
        canvas.DrawRect(SKRect.Create(lx - barW / 2f, ly - LordH - 14f, barW, 6f), barBg);
        using var barFg = new SKPaint { Color = new SKColor(0xFF, 0x22, 0x22) };
        canvas.DrawRect(SKRect.Create(lx - barW / 2f, ly - LordH - 14f, barW * ratio, 6f), barFg);

        // Body
        using var lp = new SKPaint { Color = ColLord, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(lx - LordW / 2f, ly - LordH, LordW, LordH - 8f), lp);
        // Head
        canvas.DrawCircle(lx, ly - LordH - 5f, 7f, lp);
        // Crown
        using var cp = new SKPaint { Color = ColGold, IsAntialias = true };
        canvas.DrawRect(SKRect.Create(lx - 7f, ly - LordH - 17f, 4f, 6f), cp);
        canvas.DrawRect(SKRect.Create(lx - 2f, ly - LordH - 20f, 4f, 9f), cp);
        canvas.DrawRect(SKRect.Create(lx + 3f, ly - LordH - 17f, 4f, 6f), cp);
        // Sword
        using var sp = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), StrokeWidth = 2f, IsAntialias = true };
        canvas.DrawLine(lx + LordW / 2f, ly - LordH, lx + LordW / 2f + 18f, ly - LordH / 2f, sp);
    }

    // ── Enemies ───────────────────────────────────────────────────────────────

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
        float ex = e.X - e.W / 2f;
        float ey = GroundY - e.H;
        SKColor col = EnemyCol(e.Type);

        switch (e.Type)
        {
            case EnemyType.Catapult:
                DrawCatapultSprite(canvas, e, col);
                break;
            case EnemyType.Ram:
                DrawRamSprite(canvas, e, col);
                break;
            case EnemyType.Cow:
                DrawCowSprite(canvas, e, col);
                break;
            default:
                DrawHumanoidEnemy(canvas, e, col);
                break;
        }

        // HP bar (only for enemies with > 1 max HP)
        if (e.MaxHP > 1f)
        {
            float barW  = e.W + 4f;
            float ratio = e.HP / e.MaxHP;
            using var bg = new SKPaint { Color = new SKColor(0x40, 0x00, 0x00) };
            canvas.DrawRect(SKRect.Create(ex - 2f, ey - 8f, barW, 4f), bg);
            using var fg = new SKPaint { Color = new SKColor(0xFF, 0x44, 0x00) };
            canvas.DrawRect(SKRect.Create(ex - 2f, ey - 8f, barW * ratio, 4f), fg);
        }
    }

    private static void DrawHumanoidEnemy(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.W / 2f;
        float ey = GroundY - e.H;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        // Body
        canvas.DrawRect(SKRect.Create(ex, ey, e.W, e.H - 8f), p);
        // Head
        canvas.DrawCircle(e.X, ey - 5f, 6f, p);
        // Weapon indicator
        if (e.Type == EnemyType.Crossbowman)
        {
            using var wp = new SKPaint { Color = ColArrow, StrokeWidth = 1.5f, IsAntialias = true };
            canvas.DrawLine(ex - 4f, ey + e.H / 3f, ex - 12f, ey + e.H / 3f, wp);
        }
        else if (e.Type == EnemyType.Spearman || e.Type == EnemyType.Berserker)
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
        float ex = e.X - e.W / 2f;
        float ey = GroundY - e.H;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        // Base frame
        canvas.DrawRoundRect(SKRect.Create(ex, ey + 8f, e.W, e.H - 8f), 3f, 3f, p);
        // Arm
        using var arm = new SKPaint { Color = new SKColor(0x8B, 0x5E, 0x2E), StrokeWidth = 3f, IsAntialias = true };
        canvas.DrawLine(e.X, ey + 14f, e.X - 14f, ey, arm);
        // Wheels
        using var wheel = new SKPaint { Color = new SKColor(0x55, 0x44, 0x22), IsAntialias = true };
        canvas.DrawCircle(ex + 6f,  GroundY - 5f, 5f, wheel);
        canvas.DrawCircle(ex + e.W - 6f, GroundY - 5f, 5f, wheel);
    }

    private static void DrawRamSprite(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.W / 2f;
        float ey = GroundY - e.H;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        // Body (wooden roof frame)
        canvas.DrawRoundRect(SKRect.Create(ex, ey, e.W, e.H - 6f), 4f, 4f, p);
        // Ram tip
        using var tip = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), IsAntialias = true };
        canvas.DrawRect(SKRect.Create(ex - 10f, ey + 6f, 14f, 8f), tip);
        // Wheels
        using var wheel = new SKPaint { Color = new SKColor(0x55, 0x44, 0x22), IsAntialias = true };
        canvas.DrawCircle(ex + 6f,      GroundY - 4f, 5f, wheel);
        canvas.DrawCircle(ex + e.W - 6f, GroundY - 4f, 5f, wheel);
    }

    private static void DrawCowSprite(SKCanvas canvas, Enemy e, SKColor col)
    {
        float ex = e.X - e.W / 2f;
        float ey = GroundY - e.H;
        using var p = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(ex, ey + 4f, e.W, e.H - 8f), 5f, 5f, p);
        // Head
        using var head = new SKPaint { Color = col, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(ex - 8f, ey + 6f, 14f, 10f), 4f, 4f, head);
        // Legs
        using var leg = new SKPaint { Color = new SKColor(0xDD, 0xDD, 0xCC), StrokeWidth = 3f, IsAntialias = true };
        canvas.DrawLine(ex + 4f,      ey + e.H - 4f, ex + 4f,      GroundY, leg);
        canvas.DrawLine(ex + e.W - 4f, ey + e.H - 4f, ex + e.W - 4f, GroundY, leg);
        // Spots
        using var spot = new SKPaint { Color = new SKColor(0x88, 0x88, 0x77), IsAntialias = true };
        canvas.DrawCircle(e.X, ey + 8f, 3f, spot);
        // Cow label
        DrawHelper.DrawCenteredText(canvas, "MOO", 10f, new SKColor(0x55, 0x44, 0x22), e.X, ey);
    }

    // ── Arrows ────────────────────────────────────────────────────────────────

    private void DrawArrows(SKCanvas canvas)
    {
        foreach (var a in _arrows)
        {
            if (!a.Active) continue;
            float angle = MathF.Atan2(a.VY, a.VX);
            float len   = 14f;
            float dx    = MathF.Cos(angle) * len;
            float dy    = MathF.Sin(angle) * len;
            using var ap = new SKPaint
            {
                Color       = a.IsEnemy ? ColFire : ColArrow,
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

    // ── Aim indicator ─────────────────────────────────────────────────────────

    private void DrawAimIndicator(SKCanvas canvas)
    {
        if (_archerCount == 0) return;

        // Find the outermost active archer's position as reference
        Wall? refWall = null;
        for (int i = _walls.Count - 1; i >= 0; i--)
        {
            if (!_walls[i].IsDestroyed && _walls[i].HasArcher)
            { refWall = _walls[i]; break; }
        }
        if (refWall == null) return;

        float ax = refWall.ArcherCenterX;
        float ay = refWall.ArcherBaseY - ArcherH / 2f;

        float rad = _aimAngle * MathF.PI / 180f;
        float vx  =  ArrowSpeed * MathF.Cos(rad);
        float vy  = -ArrowSpeed * MathF.Sin(rad);

        // Draw dotted preview trajectory
        using var dotPaint = new SKPaint
        {
            Color       = new SKColor(0xFF, 0xFF, 0xFF, 100),
            StrokeWidth = 1.5f,
            IsAntialias = true,
            PathEffect  = SKPathEffect.CreateDash([4f, 6f], 0f)
        };

        using var path = new SKPath();
        path.MoveTo(ax, ay);

        float px = ax, py = ay, pvx = vx, pvy = vy;
        bool first = true;
        for (float t = 0; t < 3f; t += 0.05f)
        {
            float nx = ax + vx * t;
            float ny = ay + vy * t + 0.5f * ArrowGravity * t * t;
            if (nx > GameWidth || ny > GroundY) break;
            if (first) { path.MoveTo(nx, ny); first = false; }
            else        path.LineTo(nx, ny);
        }
        canvas.DrawPath(path, dotPaint);

        // Landing dot
        float landT = FindLandingTime(ay, vy);
        if (landT > 0f)
        {
            float landX = ax + vx * landT;
            float landY = GroundY;
            if (landX >= 0 && landX <= GameWidth)
            {
                using var dotFill = new SKPaint { Color = new SKColor(0xFF, 0xFF, 0x00, 180), IsAntialias = true };
                canvas.DrawCircle(landX, landY - 3f, 5f, dotFill);
            }
        }
    }

    private float FindLandingTime(float startY, float vy0)
    {
        // Solve: startY + vy0*t + 0.5*g*t^2 = GroundY
        float dy = GroundY - startY;
        float a  = 0.5f * ArrowGravity;
        float b  = vy0;
        float c  = -dy;
        float disc = b * b - 4f * a * c;
        if (disc < 0f) return -1f;
        float t1 = (-b + MathF.Sqrt(disc)) / (2f * a);
        float t2 = (-b - MathF.Sqrt(disc)) / (2f * a);
        float t  = t1 > 0 ? (t2 > 0 ? Math.Min(t1, t2) : t1) : t2;
        return t > 0f ? t : -1f;
    }

    // ── HUD ───────────────────────────────────────────────────────────────────

    private void DrawHud(SKCanvas canvas)
    {
        // Score
        DrawHelper.DrawText(canvas, $"Score: {Score}", 20f, ColHud, 8f, 26f);

        // Level (placeholder, always 1 for now)
        string levelStr = $"Level {Level}";
        float  levelW   = DrawHelper.MeasureText(levelStr, 20f);
        DrawHelper.DrawText(canvas, levelStr, 20f, ColHud, GameWidth - levelW - 8f, 26f);

        // Archer/Worker counts
        DrawHelper.DrawText(canvas, $"Archers: {_archerCount}", 16f, ColArcher, 8f, 50f);
        DrawHelper.DrawText(canvas, $"Workers: {_workerCount}", 16f, ColWorker, 8f, 70f);

        // Keep progress bar (top-left below counts)
        float barX = 8f, barY = 82f, barW = 140f, barH = 10f;
        using var barBg = new SKPaint { Color = new SKColor(0x30, 0x30, 0x30) };
        canvas.DrawRect(SKRect.Create(barX, barY, barW, barH), barBg);
        using var barFg = new SKPaint { Color = ColKeepProg };
        canvas.DrawRect(SKRect.Create(barX, barY, barW * _keepProgress, barH), barFg);
        DrawHelper.DrawText(canvas, "Keep", 11f, ColDim, barX, barY - 3f);

        // Aim angle indicator
        string aimStr = $"Aim: {_aimAngle:F0}°";
        DrawHelper.DrawCenteredText(canvas, aimStr, 15f, ColDim, GameWidth / 2f, 20f);

        // Special weapon icons (top-center)
        DrawSpecialWeaponIcons(canvas);

        // Arrow cooldown indicator
        if (_arrowCooldown > 0f)
        {
            float r = _arrowCooldown / ArrowCooldownTime;
            DrawHelper.DrawCenteredText(canvas, "●", 12f,
                new SKColor(0xFF, 0xFF, 0xFF, (byte)(r * 200)),
                GameWidth / 2f, 38f);
        }

        // Controls hint (small, bottom-right)
        DrawHelper.DrawText(canvas, "← → aim  |  SPACE fire  |  ↑↓ convert  |  Z oil  X cannon  C logs",
            11f, ColDim, 10f, GameHeight - 12f);

        // Lord HP if active
        if (_lordActive)
        {
            float ratio = _lordHP / 10f;
            DrawHelper.DrawText(canvas, $"Lord HP: {_lordHP:F0}", 16f,
                ratio < 0.3f ? ColRed : ColGold,
                GameWidth - 160f, 50f);
        }

        // Accuracy multiplier
        if (_archerCount == 1 && _accuracyMult > 1)
        {
            DrawHelper.DrawCenteredText(canvas, $"Accuracy ×{_accuracyMult}", 15f, ColGold,
                GameWidth / 2f, 60f);
        }
    }

    private void DrawSpecialWeaponIcons(SKCanvas canvas)
    {
        float cy = 48f;
        float spacing = 70f;
        float startX  = GameWidth / 2f - spacing;

        DrawWeaponIcon(canvas, "Z Oil",    startX,           cy, _oilAvail,      ColOil);
        DrawWeaponIcon(canvas, "X Cannon", startX + spacing, cy, _mangonelAvail, ColBoulder);
        DrawWeaponIcon(canvas, "C Logs",   startX + spacing * 2f, cy, _logsAvail, ColFire);
    }

    private static void DrawWeaponIcon(SKCanvas canvas, string label, float cx, float cy, bool avail, SKColor col)
    {
        float alpha = avail ? 1f : 0.35f;
        DrawHelper.DrawCenteredText(canvas, label, 14f, avail ? col : ColDim, cx, cy, alpha);
    }

    // ── Float texts ───────────────────────────────────────────────────────────

    private void DrawFloatTexts(SKCanvas canvas)
    {
        foreach (var t in _texts)
        {
            float alpha = Math.Clamp(t.Life / 1.4f, 0f, 1f);
            DrawHelper.DrawCenteredText(canvas, t.Text, 16f, t.Color, t.X, t.Y, alpha);
        }
    }

    // ── Start screen ──────────────────────────────────────────────────────────

    private static void DrawStartScreen(SKCanvas canvas)
    {
        DrawHelper.DrawOverlay(canvas, GameWidth, GameHeight, 0.6f);

        DrawHelper.DrawCenteredText(canvas, "CASTLE ATTACK", 68f, ColGold, GameWidth / 2f, 190f);
        DrawHelper.DrawCenteredText(canvas, "Defend the castle until the keep is complete!", 22f, ColHud, GameWidth / 2f, 258f);

        float y = 310f;
        DrawHelper.DrawCenteredText(canvas, "← → Adjust aim   SPACE Fire volley", 17f, ColDim, GameWidth / 2f, y);
        y += 24f;
        DrawHelper.DrawCenteredText(canvas, "↓ Archer → Worker     ↑ Worker → Archer", 17f, ColDim, GameWidth / 2f, y);
        y += 24f;
        DrawHelper.DrawCenteredText(canvas, "Z Boiling Oil   X Mangonel   C Flaming Logs", 17f, ColDim, GameWidth / 2f, y);

        DrawHelper.DrawCenteredText(canvas, "Click or Tap to Start", 24f, ColAccent, GameWidth / 2f, 420f);
    }
}
