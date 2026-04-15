using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SpaceInvaders.SpaceInvadersConstants;

namespace SkiaSharpGames.SpaceInvaders;

internal sealed class PlayScreen(SpaceInvadersGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _starPaint = new() { Color = SKColors.White.WithAlpha((byte)(255 * 0.5f)), IsAntialias = true };

    private readonly TextSprite _scoreText = new() { Size = 24f, Color = SKColors.White };
    private readonly TextSprite _livesText = new() { Size = 24f, Color = AccentColor };
    private readonly TextSprite _controlsText = new() { Text = "← → move    SPACE / ENTER fire", Size = 18f, Color = HudDimColor, Align = TextAlign.Center };

    private readonly PlayerCannon _player = new();
    private readonly List<Invader> _invaders = [];
    private readonly List<Bullet> _playerBullets = [];
    private readonly List<Bullet> _enemyBullets = [];
    private readonly List<ShieldBlock> _shieldBlocks = [];
    private readonly List<SKPoint> _stars = [];

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
        _invaderFrameB = false;
        _endTriggered = false;
        _invaderDirection = 1f;
        _playerFireCooldown = 0f;
        _invaderMoveTimer = InvaderMoveStartInterval;
        _enemyFireTimer = 0.8f;
        _playerBullets.Clear();
        _enemyBullets.Clear();
        _invaders.Clear();
        _shieldBlocks.Clear();

        _player.X = GameWidth / 2f;
        _player.Y = PlayerY;
        BuildInvaders();
        BuildShields();
    }

    public override void OnPointerMove(float x, float y) => MovePlayerTo(x);

    public override void OnPointerDown(float x, float y)
    {
        MovePlayerTo(x);
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

    public override void Update(float deltaTime)
    {
        if (_endTriggered)
            return;

        if (_leftHeld ^ _rightHeld)
        {
            float direction = _leftHeld ? -1f : 1f;
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
        if (_playerFireCooldown > 0f || _playerBullets.Count > 0 || _endTriggered)
            return;

        _playerFireCooldown = PlayerFireCooldown;

        _playerBullets.Add(new Bullet(fromEnemy: false)
        {
            X = _player.X,
            Y = _player.Y - PlayerHeight / 2f - BulletHeight / 2f - 2f,
            SpeedY = -PlayerBulletSpeed
        });
    }

    private void UpdateInvaderFormation(float deltaTime)
    {
        _invaderMoveTimer -= deltaTime;
        if (_invaderMoveTimer > 0f)
            return;

        int alive = _invaders.Count(i => i.Active);
        if (alive <= 0)
            return;

        float progress = 1f - alive / (float)(InvaderRows * InvaderCols);
        _invaderMoveTimer = InvaderMoveStartInterval + (InvaderMoveMinInterval - InvaderMoveStartInterval) * progress;

        float minX = _invaders.Where(i => i.Active).Min(i => i.X);
        float maxX = _invaders.Where(i => i.Active).Max(i => i.X);
        float nextLeft = minX - InvaderWidth / 2f + InvaderStepX * _invaderDirection;
        float nextRight = maxX + InvaderWidth / 2f + InvaderStepX * _invaderDirection;

        bool hitSide = nextLeft <= InvaderSideMargin || nextRight >= GameWidth - InvaderSideMargin;

        foreach (var invader in _invaders)
        {
            if (!invader.Active)
                continue;

            if (hitSide)
                invader.Y += InvaderStepDown;
            else
                invader.X += InvaderStepX * _invaderDirection;
        }

        if (hitSide)
            _invaderDirection = -_invaderDirection;

        _invaderFrameB = !_invaderFrameB;
    }

    private void UpdateEnemyFire(float deltaTime)
    {
        _enemyFireTimer -= deltaTime;
        if (_enemyFireTimer > 0f)
            return;

        int alive = _invaders.Count(i => i.Active);
        float threat = alive / (float)(InvaderRows * InvaderCols);
        float fireWindow = EnemyFireMinInterval + (EnemyFireMaxInterval - EnemyFireMinInterval) * threat;
        _enemyFireTimer = Random.Shared.NextSingle() * fireWindow + EnemyFireMinInterval;

        var shooter = SelectEnemyShooter();
        if (shooter is null)
            return;

        _enemyBullets.Add(new Bullet(fromEnemy: true)
        {
            X = shooter.X,
            Y = shooter.Y + InvaderHeight / 2f + BulletHeight / 2f + 2f,
            SpeedY = EnemyBulletSpeed
        });
    }

    private Invader? SelectEnemyShooter()
    {
        List<int> columnsWithEnemies = [.. _invaders.Where(i => i.Active).Select(i => i.Col).Distinct()];
        if (columnsWithEnemies.Count == 0)
            return null;

        int column = columnsWithEnemies[Random.Shared.Next(columnsWithEnemies.Count)];
        return _invaders
            .Where(i => i.Active && i.Col == column)
            .OrderByDescending(i => i.Row)
            .FirstOrDefault();
    }

    private void UpdateBullets(float deltaTime)
    {
        for (int i = _playerBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _playerBullets[i];
            bullet.Y += bullet.SpeedY * deltaTime;

            bool consumed = false;
            for (int invaderIndex = 0; invaderIndex < _invaders.Count; invaderIndex++)
            {
                var invader = _invaders[invaderIndex];
                if (!invader.Active ||
                    !CollisionResolver.Overlaps(bullet, bullet.Collider, invader, invader.Collider))
                    continue;

                invader.Active = false;
                state.Score += invader.ScoreValue;
                _playerBullets.RemoveAt(i);
                consumed = true;
                break;
            }

            if (consumed)
                continue;

            for (int shieldIndex = 0; shieldIndex < _shieldBlocks.Count; shieldIndex++)
            {
                var block = _shieldBlocks[shieldIndex];
                if (!block.Active ||
                    !CollisionResolver.Overlaps(bullet, bullet.Collider, block, block.Collider))
                    continue;

                block.Hit();
                _playerBullets.RemoveAt(i);
                consumed = true;
                break;
            }

            if (consumed)
                continue;

            for (int enemyIndex = _enemyBullets.Count - 1; enemyIndex >= 0; enemyIndex--)
            {
                var enemyBullet = _enemyBullets[enemyIndex];
                if (!CollisionResolver.Overlaps(bullet, bullet.Collider, enemyBullet, enemyBullet.Collider))
                    continue;

                _enemyBullets.RemoveAt(enemyIndex);
                _playerBullets.RemoveAt(i);
                consumed = true;
                break;
            }

            if (!consumed && bullet.Y < -20f)
                _playerBullets.RemoveAt(i);
        }

        for (int i = _enemyBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _enemyBullets[i];
            bullet.Y += bullet.SpeedY * deltaTime;

            bool consumed = false;
            for (int shieldIndex = 0; shieldIndex < _shieldBlocks.Count; shieldIndex++)
            {
                var block = _shieldBlocks[shieldIndex];
                if (!block.Active ||
                    !CollisionResolver.Overlaps(bullet, bullet.Collider, block, block.Collider))
                    continue;

                block.Hit();
                _enemyBullets.RemoveAt(i);
                consumed = true;
                break;
            }

            if (consumed)
                continue;

            if (CollisionResolver.Overlaps(bullet, bullet.Collider, _player, _player.Collider))
            {
                _enemyBullets.RemoveAt(i);
                state.Lives--;
                _playerBullets.Clear();

                if (state.Lives <= 0)
                    TriggerGameOver();
            }
            else if (bullet.Y > GameHeight + 20f)
            {
                _enemyBullets.RemoveAt(i);
            }
        }
    }

    private void CheckEndConditions()
    {
        if (_endTriggered)
            return;

        if (_invaders.Any(invader => invader.Active && invader.Y + InvaderHeight / 2f >= PlayerY - 24f))
        {
            TriggerGameOver();
            return;
        }

        if (_invaders.All(invader => !invader.Active))
            TriggerVictory();
    }

    private void TriggerGameOver()
    {
        if (_endTriggered)
            return;

        _endTriggered = true;
        coordinator.TransitionTo<GameOverScreen>(new DissolveTransition());
    }

    private void TriggerVictory()
    {
        if (_endTriggered)
            return;

        _endTriggered = true;
        coordinator.TransitionTo<VictoryScreen>(new DissolveTransition());
    }

    private void BuildInvaders()
    {
        float totalWidth = InvaderCols * InvaderWidth + (InvaderCols - 1) * InvaderSpacingX;
        float startX = (GameWidth - totalWidth) / 2f + InvaderWidth / 2f;

        for (int row = 0; row < InvaderRows; row++)
        {
            for (int col = 0; col < InvaderCols; col++)
            {
                _invaders.Add(new Invader(row, col)
                {
                    X = startX + col * (InvaderWidth + InvaderSpacingX),
                    Y = InvaderStartY + row * (InvaderHeight + InvaderSpacingY),
                });
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

                    _shieldBlocks.Add(new ShieldBlock
                    {
                        X = topLeftX + col * (ShieldBlockSize + ShieldBlockGap) + ShieldBlockSize / 2f,
                        Y = ShieldY + row * (ShieldBlockSize + ShieldBlockGap) + ShieldBlockSize / 2f
                    });
                }
            }
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(BackgroundColor);

        foreach (var star in _stars)
            canvas.DrawRect(star.X, star.Y, 2f, 2f, _starPaint);

        foreach (var invader in _invaders)
        {
            if (invader.Active)
                invader.Draw(canvas, _invaderFrameB);
        }

        foreach (var shield in _shieldBlocks)
            shield.Draw(canvas);

        foreach (var bullet in _playerBullets)
            bullet.Draw(canvas);

        foreach (var bullet in _enemyBullets)
            bullet.Draw(canvas);

        _player.Draw(canvas);

        _scoreText.Text = $"SCORE: {state.Score:0000}";
        _scoreText.Draw(canvas, 20f, 34f);

        _livesText.Text = $"LIVES: {state.Lives}";
        float livesWidth = _livesText.MeasureWidth();
        _livesText.Draw(canvas, GameWidth - livesWidth - 20f, 34f);

        _controlsText.Draw(canvas, GameWidth / 2f, GameHeight - 12f);
    }
}
