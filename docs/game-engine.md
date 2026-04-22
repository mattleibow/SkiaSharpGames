# SkiaSharp Game Engine Guide

This engine is intentionally small. It is built to make arcade-style 2D games easy to write, not to become a full physics or ECS framework.

The target games are:

- **Breakout** - one fast ball, paddle bounce, bricks, falling power-ups
- **Castle Attack** - waves of enemies, arrows, wall damage, simple projectiles
- **Sink Sub** - moving ship, falling depth charges, rising mines, submarines below

## Design goals

1. **One clear update loop** - move things, detect hits, respond, then draw.
2. **One position model** - entities own world position; helper components never keep a second copy.
3. **One collision model** - colliders describe shape, rigidbodies describe motion, the collision helper does the math.
4. **Minimal boilerplate** - the common game case should read like gameplay code, not engine code.
5. **Predictable behavior** - simple arcade collisions are preferred over heavy simulation.

## Mental model

Every moving thing in the game should follow the same pattern:

```csharp
ball.Rigidbody.SetVelocity(140f, -220f);
ball.Rigidbody.Step(ball, deltaTime);

if (CollisionResolver.TryGetHit(ball, ball.Collider, paddle, paddle.Collider, out var hit))
{
    ball.Rigidbody.Bounce(hit);
}
```

That is the core loop:

1. **update motion**
2. **ask what hit**
3. **decide game rules**
4. **apply bounce / damage / score / spawn effects**
5. **draw**

## Core building blocks

### Game

`Game` is the host object used by the Blazor view. It:

- owns the logical game size
- forwards input to the active screen
- calls `Update(deltaTime)` and `Draw(canvas, width, height)`
- keeps game rendering in game-space coordinates

### GameScreen

Each game is split into screens such as:

- start screen
- play screen
- victory screen
- game over screen

`GameScreen` handles gameplay state and rendering for that mode. Screens are switched by `IScreenCoordinator`.

### ScreenCoordinator

`ScreenCoordinator` owns screen-stack orchestration:

- active screen selection
- transition progress and transition drawing
- overlay push/pop
- active input routing
- screen lifecycle callbacks (`OnActivating`, `OnActivated`, `OnDeactivating`, `OnDeactivated`, pause/resume)

Screen logic should stay in the screen; cross-screen flow stays in the coordinator.

### Entity

`Entity` is the single source of truth for world position:

- `X` and `Y` are the entity center in game-space
- `Active` controls whether it should still participate in the game

An entity does **not** try to be a full engine object. It is just the anchor that physics, collisions, and drawing read from.

Entity children are useful for ownership-driven rendering composition (for example, floating combat text as a text-label child entity instead of ad-hoc screen draw calls).

### Rigidbody2D

`Rigidbody2D` is a lightweight motion helper:

- stores velocity
- advances an entity during `Step(...)`
- provides bounce / reflect helpers for arcade collisions

This is not a full rigidbody solver. There is no mass graph, no joints, and no hidden simulation loop.

### Collider2D

Colliders define the shape of an entity for hit testing. The engine currently uses:

- `CircleCollider`
- `RectCollider`

Both are centered on the owning entity and may use offsets when needed.

This means the coordinate rules stay consistent across games. The same origin is used for:

- rendering
- movement
- collision checks

### CollisionResolver

`CollisionResolver` handles overlap checks and collision details. It should answer questions like:

- did these two things overlap?
- what was the hit normal?
- should this bounce horizontally or vertically?
- did the entity hit the playfield bounds?

Game rules stay in the game code. Collision math stays in the engine.

### Timers and animation helpers

The engine also includes small helpers for common arcade behavior:

- `CountdownTimer` for cooldowns and temporary power-ups
- `AnimatedFloat` for smooth UI or gameplay value changes
- `LoopedAnimation` for shimmer, pulse, and repeated effects

## World and bounds

The playfield is a fixed logical rectangle. A game may choose its own size, but the rules inside it are stable.

Typical boundary rules:

- Breakout: bounce off left, right, and top; falling below the bottom loses a life
- Sink Sub: the ship is clamped to the surface lane; submarines stay in the underwater zone and reverse direction at bounds
- Castle Attack: enemies move toward the castle line while projectiles follow their own motion rules

The engine should provide explicit helpers for those bounds so each game does not re-implement wall math.

## What the engine is **not**

To keep it maintainable, this engine does **not** try to be:

- a general-purpose ECS
- a realistic physics simulator
- a scene graph
- a content pipeline
- a AAA rendering stack

If a helper does not make Breakout, Castle Attack, or Sink Sub easier to build, it probably does not belong here.

## Game-specific notes

### Breakout

Best served by:

- circle ball collider
- box paddle and brick colliders
- wall bounds
- explicit post-collision response for score, power-ups, and paddle-angle control

### Castle Attack

Best served by:

- entity lists for enemies, arrows, rocks, and effects
- simple box/circle collisions
- timers for spawn waves and special weapon cooldown state
- direct rule code for wall damage and unit behavior

### Sink Sub

Best served by:

- surface ship entity with horizontal movement
- downward moving depth charges with a max active count
- submarines that patrol and periodically launch upward mines
- collision checks between charges/subs and mines/ship

This makes Sink Sub a strong validation game for the engine because it mixes patrol movement, hazards, and projectile limits without requiring complicated physics.
