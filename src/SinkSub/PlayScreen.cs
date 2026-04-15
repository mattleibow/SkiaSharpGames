using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.SinkSub;

internal sealed class PlayScreen(SinkSubGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private static readonly SKPaint _fillPaint = new() { IsAntialias = true };

    private readonly TextSprite _scoreText = new() { Size = 22f };
    private readonly TextSprite _livesText = new() { Size = 22f };
    private readonly TextSprite _waveText = new() { Size = 22f };
    private readonly TextSprite _chargeText = new() { Size = 22f, Color = AccentColor, Align = TextAlign.Right };
    private readonly TextSprite _instructionsText = new() { Size = 16f, Color = DimColor, Align = TextAlign.Center, Text = "Z = left charge    X / Space = right charge" };
    private readonly TextSprite _waveIncomingText = new() { Size = 28f, Color = AccentColor, Align = TextAlign.Center };

    private readonly Ship _ship = new();
    private readonly Entity _submarines = new();
    private readonly Entity _depthCharges = new();
    private readonly Entity _mines = new();

    private bool _leftHeld;
    private bool _rightHeld;
    private bool _useLeftTube = true;
    private bool _wavePending;
    private bool _gameOverShown;
    private CountdownTimer _nextWaveTimer;

    public override void OnActivating()
    {
        state.Score = 0;
        state.Lives = 3;
        state.Wave = 0;
        _leftHeld = false;
        _rightHeld = false;
        _wavePending = false;
        _gameOverShown = false;
        ClearChildren(_depthCharges);
        ClearChildren(_mines);
        ClearChildren(_submarines);
        _ship.X = GameWidth / 2f;
        _ship.Y = ShipY;
        StartNextWave();
    }

    public override void OnPointerMove(float x, float y) => MoveShipTo(x);

    public override void OnPointerDown(float x, float y)
    {
        bool dropLeft = x < _ship.X;
        MoveShipTo(x);
        DropCharge(dropLeft);
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
            case "z":
            case "Z":
                DropCharge(leftSide: true);
                break;
            case "x":
            case "X":
                DropCharge(leftSide: false);
                break;
            case " ":
                DropCharge(_useLeftTube);
                _useLeftTube = !_useLeftTube;
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
        UpdateShip(deltaTime);
        UpdateCharges(deltaTime);
        UpdateSubmarines(deltaTime);
        UpdateMines(deltaTime);

        if (!_wavePending && _submarines.Children.All(s => !s.Active))
        {
            _wavePending = true;
            _nextWaveTimer.Set(1.2f);
        }

        if (_wavePending && _nextWaveTimer.Tick(deltaTime))
            StartNextWave();
    }

    private void UpdateShip(float deltaTime)
    {
        float dx = 0f;
        if (_leftHeld) dx -= ShipSpeed * deltaTime;
        if (_rightHeld) dx += ShipSpeed * deltaTime;

        if (dx != 0f)
            MoveShipTo(_ship.X + dx);
    }

    private void MoveShipTo(float x) =>
        _ship.X = Math.Clamp(x, ShipWidth / 2f, GameWidth - ShipWidth / 2f);

    private void DropCharge(bool leftSide)
    {
        if (_depthCharges.ChildCount >= MaxCharges || _gameOverShown)
            return;

        var charge = new DepthCharge
        {
            X = _ship.X + (leftSide ? -ChargeSpawnOffsetX : ChargeSpawnOffsetX),
            Y = _ship.Y + ChargeSpawnOffsetY
        };
        charge.Rigidbody.SetVelocity(0f, ChargeSpeed);
        _depthCharges.AddChild(charge);
    }

    private void UpdateCharges(float deltaTime)
    {
        _depthCharges.Update(deltaTime);

        var charges = _depthCharges.Children;
        for (int i = charges.Count - 1; i >= 0; i--)
        {
            var charge = (DepthCharge)charges[i];
            if (!charge.Active) continue;

            if (_submarines.FindChildCollision(charge, out _) is Submarine sub)
            {
                sub.Active = false;
                state.Score += 100;
                charge.Active = false;
                continue;
            }

            if (charge.Y > GameHeight + 20f)
                charge.Active = false;
        }

        _depthCharges.RemoveInactiveChildren();
    }

    private void UpdateSubmarines(float deltaTime)
    {
        _submarines.Update(deltaTime);

        foreach (var child in _submarines.Children)
        {
            if (child is not Submarine sub || !sub.Active)
                continue;

            if (sub.X < SubWidth / 2f)
            {
                sub.X = SubWidth / 2f;
                sub.Reverse();
            }
            else if (sub.X > GameWidth - SubWidth / 2f)
            {
                sub.X = GameWidth - SubWidth / 2f;
                sub.Reverse();
            }

            if (sub.TickMineTimer(deltaTime) && _mines.ChildCount < 12)
            {
                var mine = new Mine
                {
                    X = sub.X,
                    Y = sub.Y - 12f
                };
                mine.Rigidbody.SetVelocity(0f, -MineSpeed);
                _mines.AddChild(mine);
            }
        }
    }

    private void UpdateMines(float deltaTime)
    {
        _mines.Update(deltaTime);

        var mines = _mines.Children;
        for (int i = mines.Count - 1; i >= 0; i--)
        {
            var mine = (Mine)mines[i];
            if (!mine.Active) continue;

            if (mine.Overlaps(_ship))
            {
                mine.Active = false;
                state.Lives--;

                if (state.Lives <= 0 && !_gameOverShown)
                {
                    _gameOverShown = true;
                    coordinator.PushOverlay<GameOverScreen>();
                }
            }
            else if (mine.Y < -20f)
            {
                mine.Active = false;
            }
        }

        _mines.RemoveInactiveChildren();
    }

    private void StartNextWave()
    {
        _wavePending = false;
        state.Wave++;
        ClearChildren(_submarines);

        int subCount = Math.Min(2 + state.Wave, 5);
        for (int i = 0; i < subCount; i++)
        {
            float y = SubMinY + i * 55f + Random.Shared.NextSingle() * 22f;
            y = Math.Clamp(y, SubMinY, SubMaxY);

            int direction = i % 2 == 0 ? 1 : -1;
            float x = direction > 0 ? 60f : GameWidth - 60f;
            float speed = 92f + state.Wave * 10f + i * 8f;

            var sub = new Submarine();
            sub.Reset(x, y, speed, direction, 1f + Random.Shared.NextSingle() * 2.4f);
            _submarines.AddChild(sub);
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(SkyColor);

        _fillPaint.Color = WaterColor;
        canvas.DrawRect(SKRect.Create(0f, WaterlineY, GameWidth, GameHeight - WaterlineY), _fillPaint);
        _fillPaint.Color = DeepWaterColor.WithAlpha((byte)(255 * 0.55f));
        canvas.DrawRect(SKRect.Create(0f, WaterlineY + 180f, GameWidth, GameHeight - WaterlineY - 180f), _fillPaint);
        _fillPaint.Color = SKColors.White.WithAlpha((byte)(255 * 0.6f));
        canvas.DrawRect(SKRect.Create(0f, WaterlineY - 4f, GameWidth, 4f), _fillPaint);

        _fillPaint.Color = SKColors.White.WithAlpha((byte)(255 * 0.18f));
        for (int i = 0; i < 8; i++)
            canvas.DrawRect(SKRect.Create(30f + i * 110f, WaterlineY + 20f + (i % 2) * 10f, 60f, 2f), _fillPaint);

        _ship.Draw(canvas);
        _depthCharges.Draw(canvas);
        _submarines.Draw(canvas);
        _mines.Draw(canvas);

        _scoreText.Text = $"Score: {state.Score}";
        canvas.Save(); canvas.Translate(20f, 32f); _scoreText.Draw(canvas); canvas.Restore();

        _livesText.Text = $"Lives: {state.Lives}";
        canvas.Save(); canvas.Translate(20f, 60f); _livesText.Draw(canvas); canvas.Restore();

        _waveText.Text = $"Wave: {state.Wave}";
        canvas.Save(); canvas.Translate(20f, 88f); _waveText.Draw(canvas); canvas.Restore();

        _chargeText.Text = $"Charges: {_depthCharges.ChildCount}/{MaxCharges}";
        canvas.Save(); canvas.Translate(GameWidth - 20f, 32f); _chargeText.Draw(canvas); canvas.Restore();

        canvas.Save(); canvas.Translate(GameWidth / 2f, 32f); _instructionsText.Draw(canvas); canvas.Restore();

        if (_wavePending)
        {
            _waveIncomingText.Text = $"Wave {state.Wave + 1} incoming...";
            canvas.Save(); canvas.Translate(GameWidth / 2f, 170f); _waveIncomingText.Draw(canvas); canvas.Restore();
        }
    }

    private static void ClearChildren(Entity parent)
    {
        while (parent.ChildCount > 0)
            parent.RemoveChild(parent.Children[^1]);
    }
}
