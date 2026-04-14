using SkiaSharp;
using SkiaSharpGames.GameEngine;
using static SkiaSharpGames.BlazorApp.Games.SinkSub.SinkSubConstants;

namespace SkiaSharpGames.BlazorApp.Games.SinkSub;

internal sealed class PlayScreen(SinkSubGameState state, IScreenCoordinator coordinator) : GameScreen
{
    private readonly Ship _ship = new();
    private readonly List<Submarine> _submarines = [];
    private readonly List<DepthCharge> _charges = [];
    private readonly List<Mine> _mines = [];

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
        _charges.Clear();
        _mines.Clear();
        _submarines.Clear();
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

        if (!_wavePending && _submarines.All(s => !s.Active))
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
        if (_charges.Count >= MaxCharges || _gameOverShown)
            return;

        var charge = new DepthCharge
        {
            X = _ship.X + (leftSide ? -ChargeSpawnOffsetX : ChargeSpawnOffsetX),
            Y = _ship.Y + ChargeSpawnOffsetY
        };
        charge.Rigidbody.SetVelocity(0f, ChargeSpeed);
        _charges.Add(charge);
    }

    private void UpdateCharges(float deltaTime)
    {
        for (int i = _charges.Count - 1; i >= 0; i--)
        {
            var charge = _charges[i];
            charge.Rigidbody.Step(charge, deltaTime);

            bool consumed = false;
            for (int s = 0; s < _submarines.Count; s++)
            {
                var sub = _submarines[s];
                if (!sub.Active ||
                    !CollisionResolver.Overlaps(charge, charge.Collider, sub, sub.Collider))
                    continue;

                sub.Active = false;
                state.Score += 100;
                _charges.RemoveAt(i);
                consumed = true;
                break;
            }

            if (!consumed && charge.Y > GameHeight + 20f)
                _charges.RemoveAt(i);
        }
    }

    private void UpdateSubmarines(float deltaTime)
    {
        foreach (var sub in _submarines)
        {
            if (!sub.Active)
                continue;

            sub.Rigidbody.Step(sub, deltaTime);

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

            if (sub.TickMineTimer(deltaTime) && _mines.Count < 12)
            {
                var mine = new Mine
                {
                    X = sub.X,
                    Y = sub.Y - 12f
                };
                mine.Rigidbody.SetVelocity(0f, -MineSpeed);
                _mines.Add(mine);
            }
        }
    }

    private void UpdateMines(float deltaTime)
    {
        for (int i = _mines.Count - 1; i >= 0; i--)
        {
            var mine = _mines[i];
            mine.Rigidbody.Step(mine, deltaTime);

            if (CollisionResolver.Overlaps(mine, mine.Collider, _ship, _ship.Collider))
            {
                _mines.RemoveAt(i);
                state.Lives--;

                if (state.Lives <= 0 && !_gameOverShown)
                {
                    _gameOverShown = true;
                    coordinator.PushOverlay<GameOverScreen>();
                }
            }
            else if (mine.Y < -20f)
            {
                _mines.RemoveAt(i);
            }
        }
    }

    private void StartNextWave()
    {
        _wavePending = false;
        state.Wave++;
        _submarines.Clear();

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
            _submarines.Add(sub);
        }
    }

    public override void Draw(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(SkyColor);

        DrawHelper.FillRect(canvas, 0f, WaterlineY, GameWidth, GameHeight - WaterlineY, WaterColor);
        DrawHelper.FillRect(canvas, 0f, WaterlineY + 180f, GameWidth, GameHeight - WaterlineY - 180f, DeepWaterColor, 0.55f);
        DrawHelper.FillRect(canvas, 0f, WaterlineY - 4f, GameWidth, 4f, SKColors.White, 0.6f);

        for (int i = 0; i < 8; i++)
            DrawHelper.FillRect(canvas, 30f + i * 110f, WaterlineY + 20f + (i % 2) * 10f, 60f, 2f, SKColors.White, 0.18f);

        _ship.Draw(canvas);

        foreach (var charge in _charges)
            charge.Sprite.Draw(canvas, charge.X, charge.Y);

        foreach (var sub in _submarines)
        {
            if (sub.Active)
                sub.Draw(canvas);
        }

        foreach (var mine in _mines)
            mine.Sprite.Draw(canvas, mine.X, mine.Y);

        DrawHelper.DrawText(canvas, $"Score: {state.Score}", 22f, SKColors.White, 20f, 32f);
        DrawHelper.DrawText(canvas, $"Lives: {state.Lives}", 22f, SKColors.White, 20f, 60f);
        DrawHelper.DrawText(canvas, $"Wave: {state.Wave}", 22f, SKColors.White, 20f, 88f);

        string chargeText = $"Charges: {_charges.Count}/{MaxCharges}";
        float chargeWidth = DrawHelper.MeasureText(chargeText, 22f);
        DrawHelper.DrawText(canvas, chargeText, 22f, AccentColor, GameWidth - chargeWidth - 20f, 32f);

        DrawHelper.DrawCenteredText(canvas, "Z = left charge    X / Space = right charge", 16f, DimColor, GameWidth / 2f, 32f);

        if (_wavePending)
            DrawHelper.DrawCenteredText(canvas, $"Wave {state.Wave + 1} incoming...", 28f, AccentColor, GameWidth / 2f, 170f);
    }
}
