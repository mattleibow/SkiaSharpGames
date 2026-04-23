using SkiaSharp;
using SkiaSharp.Theatre;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class PlayScreen(SpaceInvadersGameState state, IDirector director) : Scene
{
    private static readonly SKPaint _starPaint = new() { Color = SKColors.White.WithAlpha((byte)(255 * 0.5f)), IsAntialias = true };

    private readonly HudLabel _scoreText = new() { FontSize = 24f, Color = SKColors.White };
    private readonly HudLabel _livesText = new() { FontSize = 24f, Color = AccentColor };
    private readonly HudLabel _controlsText = new() { Text = "LEFT RIGHT move    SPACE / ENTER fire", FontSize = 18f, Color = HudDimColor, Align = TextAlign.Center };

    private readonly Actor _formation = new();
    private readonly Actor _shields = new();
    private readonly Actor _playerBullets = new();
    private readonly Actor _enemyBullets = new();
    private readonly PlayerCannon _player = new();
    private readonly List<SKPoint> _stars = [];

    // ── Touch control pad ────────────────────────────────────────────────
    private bool _touchActive;
    private bool _touchLeft, _touchRight, _touchFire;

    private const float PadY = GameHeight - 80f;
    private const float PadBtnW = 90f;
    private const float PadBtnH = 50f;
    private const float PadGap = 16f;
    private static readonly float PadTotalW = PadBtnW * 3 + PadGap * 2;
    private static readonly float PadLeft = (GameWidth - PadTotalW) / 2f;
    private static readonly SKRect LeftBtnRect = SKRect.Create(PadLeft, PadY, PadBtnW, PadBtnH);
    private static readonly SKRect FireBtnRect = SKRect.Create(PadLeft + PadBtnW + PadGap, PadY, PadBtnW, PadBtnH);
    private static readonly SKRect RightBtnRect = SKRect.Create(PadLeft + 2 * (PadBtnW + PadGap), PadY, PadBtnW, PadBtnH);

    private bool _leftHeld;
    private bool _rightHeld;
    private bool _invaderFrameB;
    private bool _endTriggered;
    private float _invaderDirection = 1f;
    private float _invaderMoveTimer;
    private float _enemyFireTimer;
    private float _playerFireCooldown;

    public override void OnActivated()
    {
        if (_stars.Count == 0)
        {
            var random = new Random(406);
            for (int i = 0; i < 96; i++)
                _stars.Add(new SKPoint(random.NextSingle() * GameWidth, random.NextSingle() * GameHeight));
        }

        state.Score = 0;
        state.Lives = 3;
        _leftHeld = false;
        _rightHeld = false;
        _touchActive = false;
        _touchLeft = false;
        _touchRight = false;
        _touchFire = false;
        _invaderFrameB = false;
        _endTriggered = false;
        _invaderDirection = 1f;
        _playerFireCooldown = 0f;
        _invaderMoveTimer = InvaderMoveStartInterval;
        _enemyFireTimer = 0.8f;
        ClearChildren(_formation);
        ClearChildren(_shields);
        ClearChildren(_playerBullets);
        ClearChildren(_enemyBullets);

        _player.X = GameWidth / 2f;
        _player.Y = PlayerY;
        BuildInvaders();
        BuildShields();
    }

    public override void OnPointerDown(float x, float y) => HandleTouch(x, y, true);
    public override void OnPointerMove(float x, float y) { if (_touchActive) HandleTouch(x, y, true); }

    public override void OnPointerUp(float x, float y)
    {
        _touchActive = false;
        _touchLeft = false;
        _touchRight = false;
        _touchFire = false;
    }

    private void HandleTouch(float x, float y, bool down)
    {
        _touchActive = down;
        _touchLeft = down && LeftBtnRect.Contains(x, y);
        _touchRight = down && RightBtnRect.Contains(x, y);
        bool wasFire = _touchFire;
        _touchFire = down && FireBtnRect.Contains(x, y);
        if (_touchFire && !wasFire)
            TryFirePlayerBullet();
    }

    public override void OnKeyDown(string key)
    {
        switch (key)
        {
            case "ArrowLeft":
                _leftHeld = true;
                break;
            case "ArrowRight":
                _rightHeld = true;
                break;
            case " ":
            case "Enter":
                TryFirePlayerBullet();
                break;
        }
    }

    public override void OnKeyUp(string key)
    {
        switch (key)
        {
            case "ArrowLeft":
                _leftHeld = false;
                break;
            case "ArrowRight":
                _rightHeld = false;
                break;
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_endTriggered)
            return;

        bool moveLeft = _leftHeld || _touchLeft;
        bool moveRight = _rightHeld || _touchRight;
        if (moveLeft ^ moveRight)
        {
            float direction = moveLeft ? -1f : 1f;
            MovePlayerTo(_player.X + direction * PlayerSpeed * deltaTime);
        }

        _playerFireCooldown = MathF.Max(0f, _playerFireCooldown - deltaTime);

        UpdateInvaderFormation(deltaTime);
        UpdateEnemyFire(deltaTime);
        UpdateBullets(deltaTime);
        CheckEndConditions();
    }

    private void MovePlayerTo(float x) =>
        _player.X = Math.Clamp(x, PlayerWidth / 2f + 8f, GameWidth - PlayerWidth / 2f - 8f);

    private void TryFirePlayerBullet()
    {
        if (_playerFireCooldown > 0f || _playerBullets.ChildCount > 0 || _endTriggered)
            return;

        _playerFireCooldown = PlayerFireCooldown;

        var bullet = new Bullet(fromEnemy: false, speedY: -PlayerBulletSpeed)
        {
            X = _player.X,
            Y = _player.Y - PlayerHeight / 2f - BulletHeight / 2f - 2f,
        };
        _playerBullets.Children.Add(bullet);
    }

    private void UpdateInvaderFormation(float deltaTime)
    {
        _invaderMoveTimer -= deltaTime;
        if (_invaderMoveTimer > 0f)
            return;

        var invaders = _formation.Children;
        int alive = 0;
        for (int i = 0; i < invaders.Count; i++)
            if (invaders[i].Active) alive++;

        if (alive <= 0)
            return;

        float progress = 1f - alive / (float)(InvaderRows * InvaderCols);
        _invaderMoveTimer = InvaderMoveStartInterval + (InvaderMoveMinInterval - InvaderMoveStartInterval) * progress;

        float minX = float.MaxValue, maxX = float.MinValue;
        for (int i = 0; i < invaders.Count; i++)
        {
            if (!invaders[i].Active) continue;
            if (((Actor)invaders[i]).X < minX) minX = ((Actor)invaders[i]).X;
            if (((Actor)invaders[i]).X > maxX) maxX = ((Actor)invaders[i]).X;
        }

        float nextLeft = minX - InvaderWidth / 2f + InvaderStepX * _invaderDirection;
        float nextRight = maxX + InvaderWidth / 2f + InvaderStepX * _invaderDirection;

        bool hitSide = nextLeft <= InvaderSideMargin || nextRight >= GameWidth - InvaderSideMargin;

        for (int i = 0; i < invaders.Count; i++)
        {
            if (!invaders[i].Active) continue;

            if (hitSide)
                ((Actor)invaders[i]).Y += InvaderStepDown;
            else
                ((Actor)invaders[i]).X += InvaderStepX * _invaderDirection;
        }

        if (hitSide)
            _invaderDirection = -_invaderDirection;

        _invaderFrameB = !_invaderFrameB;
        for (int i = 0; i < invaders.Count; i++)
        {
            if (invaders[i] is Invader inv)
                inv.FrameB = _invaderFrameB;
        }
    }

    private void UpdateEnemyFire(float deltaTime)
    {
        _enemyFireTimer -= deltaTime;
        if (_enemyFireTimer > 0f)
            return;

        var invaders = _formation.Children;
        int alive = 0;
        for (int i = 0; i < invaders.Count; i++)
            if (invaders[i].Active) alive++;

        float threat = alive / (float)(InvaderRows * InvaderCols);
        float fireWindow = EnemyFireMinInterval + (EnemyFireMaxInterval - EnemyFireMinInterval) * threat;
        _enemyFireTimer = Random.Shared.NextSingle() * fireWindow + EnemyFireMinInterval;

        var shooter = SelectEnemyShooter();
        if (shooter is null)
            return;

        var bullet = new Bullet(fromEnemy: true, speedY: EnemyBulletSpeed)
        {
            X = shooter.X,
            Y = shooter.Y + InvaderHeight / 2f + BulletHeight / 2f + 2f,
        };
        _enemyBullets.Children.Add(bullet);
    }

    private Invader? SelectEnemyShooter()
    {
        var invaders = _formation.Children;
        List<int> columnsWithEnemies = [];
        for (int i = 0; i < invaders.Count; i++)
        {
            if (invaders[i] is Invader inv && inv.Active && !columnsWithEnemies.Contains(inv.Col))
                columnsWithEnemies.Add(inv.Col);
        }

        if (columnsWithEnemies.Count == 0)
            return null;

        int column = columnsWithEnemies[Random.Shared.Next(columnsWithEnemies.Count)];
        Invader? best = null;
        for (int i = 0; i < invaders.Count; i++)
        {
            if (invaders[i] is Invader inv && inv.Active && inv.Col == column)
            {
                if (best is null || inv.Row > best.Row)
                    best = inv;
            }
        }

        return best;
    }

    private void UpdateBullets(float deltaTime)
    {
        // Update bullet positions via rigidbody
        _playerBullets.Update(deltaTime);
        _enemyBullets.Update(deltaTime);

        // Player bullets vs invaders, shields, and enemy bullets
        var playerBulletList = _playerBullets.Children;
        for (int i = playerBulletList.Count - 1; i >= 0; i--)
        {
            var bullet = (Bullet)playerBulletList[i];
            if (!bullet.Active) continue;

            // vs invaders
            var hitInvader = _formation.FindChildCollision(bullet, out _);
            if (hitInvader is Invader invader)
            {
                invader.Active = false;
                state.Score += invader.ScoreValue;
                bullet.Active = false;
                continue;
            }

            // vs shields
            var hitShield = _shields.FindChildCollision(bullet, out _);
            if (hitShield is ShieldBlock block)
            {
                block.Hit();
                bullet.Active = false;
                continue;
            }

            // vs enemy bullets
            var enemyBulletList = _enemyBullets.Children;
            for (int j = enemyBulletList.Count - 1; j >= 0; j--)
            {
                var enemyBullet = enemyBulletList[j];
                if (!enemyBullet.Active) continue;
                if (bullet.Overlaps((Actor)enemyBullet))
                {
                    enemyBullet.Active = false;
                    bullet.Active = false;
                    break;
                }
            }

            if (!bullet.Active) continue;

            if (bullet.Y < -20f)
                bullet.Active = false;
        }

        // Enemy bullets vs shields and player
        var enemyBullets = _enemyBullets.Children;
        for (int i = enemyBullets.Count - 1; i >= 0; i--)
        {
            var bullet = (Bullet)enemyBullets[i];
            if (!bullet.Active) continue;

            // vs shields
            var hitShield = _shields.FindChildCollision(bullet, out _);
            if (hitShield is ShieldBlock block)
            {
                block.Hit();
                bullet.Active = false;
                continue;
            }

            // vs player
            if (bullet.Overlaps(_player))
            {
                bullet.Active = false;
                state.Lives--;

                // Clear all player bullets
                for (int j = 0; j < playerBulletList.Count; j++)
                    playerBulletList[j].Active = false;

                if (state.Lives <= 0)
                    TriggerGameOver();
            }
            else if (bullet.Y > GameHeight + 20f)
            {
                bullet.Active = false;
            }
        }

        _playerBullets.Children.RemoveInactive();
        _enemyBullets.Children.RemoveInactive();
    }

    private void CheckEndConditions()
    {
        if (_endTriggered)
            return;

        var invaders = _formation.Children;
        bool anyAlive = false;
        for (int i = 0; i < invaders.Count; i++)
        {
            if (!invaders[i].Active) continue;
            anyAlive = true;
            if (((Actor)invaders[i]).Y + InvaderHeight / 2f >= PlayerY - 24f)
            {
                TriggerGameOver();
                return;
            }
        }

        if (!anyAlive)
            TriggerVictory();
    }

    private void TriggerGameOver()
    {
        if (_endTriggered)
            return;

        _endTriggered = true;
        director.TransitionTo<GameOverScreen>(new DissolveCurtain());
    }

    private void TriggerVictory()
    {
        if (_endTriggered)
            return;

        _endTriggered = true;
        director.TransitionTo<VictoryScreen>(new DissolveCurtain());
    }

    private void BuildInvaders()
    {
        float totalWidth = InvaderCols * InvaderWidth + (InvaderCols - 1) * InvaderSpacingX;
        float startX = (GameWidth - totalWidth) / 2f + InvaderWidth / 2f;

        for (int row = 0; row < InvaderRows; row++)
        {
            for (int col = 0; col < InvaderCols; col++)
            {
                var invader = new Invader(row, col)
                {
                    X = startX + col * (InvaderWidth + InvaderSpacingX),
                    Y = InvaderStartY + row * (InvaderHeight + InvaderSpacingY),
                };
                _formation.Children.Add(invader);
            }
        }
    }

    private void BuildShields()
    {
        float shieldWidth = ShieldCols * ShieldBlockSize + (ShieldCols - 1) * ShieldBlockGap;
        float step = GameWidth / (ShieldCount + 1f);

        for (int shield = 0; shield < ShieldCount; shield++)
        {
            float centerX = step * (shield + 1);
            float topLeftX = centerX - shieldWidth / 2f;

            for (int row = 0; row < ShieldRows; row++)
            {
                for (int col = 0; col < ShieldCols; col++)
                {
                    if (row == ShieldRows - 1 && col is 3 or 4)
                        continue;

                    var block = new ShieldBlock
                    {
                        X = topLeftX + col * (ShieldBlockSize + ShieldBlockGap) + ShieldBlockSize / 2f,
                        Y = ShieldY + row * (ShieldBlockSize + ShieldBlockGap) + ShieldBlockSize / 2f
                    };
                    _shields.Children.Add(block);
                }
            }
        }
    }

    protected override void OnDraw(SKCanvas canvas)
    {
        canvas.Clear(BackgroundColor);

        foreach (var star in _stars)
            canvas.DrawRect(star.X, star.Y, 2f, 2f, _starPaint);

        _formation.Draw(canvas);
        _shields.Draw(canvas);
        _playerBullets.Draw(canvas);
        _enemyBullets.Draw(canvas);
        _player.Draw(canvas);

        _scoreText.Text = $"SCORE: {state.Score:0000}";
        canvas.Save(); canvas.Translate(20f, 34f); _scoreText.Draw(canvas); canvas.Restore();

        _livesText.Text = $"LIVES: {state.Lives}";
        float livesWidth = _livesText.MeasureWidth();
        canvas.Save(); canvas.Translate(GameWidth - livesWidth - 20f, 34f); _livesText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, GameHeight - 12f); _controlsText.Draw(canvas); canvas.Restore();

        DrawControlPad(canvas);
    }

    private void DrawControlPad(SKCanvas canvas)
    {
        var appearance = ((ResolvedHudTheme?.Button ?? DefaultButtonAppearance.Default) as DefaultButtonAppearance ?? DefaultButtonAppearance.Default) with
        {
            CornerRadius = 8f,
            BorderWidth = 1.5f,
            BevelSize = 1.5f,
        };
        appearance.DrawDirect(canvas, LeftBtnRect, "<", _touchLeft, fontSize: 22f);
        appearance.DrawDirect(canvas, FireBtnRect, "FIRE", _touchFire, fontSize: 22f);
        appearance.DrawDirect(canvas, RightBtnRect, ">", _touchRight, fontSize: 22f);
    }

    private static void ClearChildren(Actor parent)
    {
        while (parent.ChildCount > 0)
            parent.Children.Remove(parent.Children[^1]);
    }
}
