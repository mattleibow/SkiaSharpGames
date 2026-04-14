# Sink Sub gameplay guide

## Overview

Sink Sub is a naval arcade prototype built directly from the design brief:

- the player controls a battleship at the surface
- submarines patrol below
- depth charges fall slowly
- mines float up toward the ship

## Objective

Sink as many submarines as possible while avoiding the mines they launch.

## Controls

| Input | Action |
|------|--------|
| Mouse / touch move | Move the ship left and right |
| Z | Drop a charge from the left side |
| X | Drop a charge from the right side |
| Space | Alternate depth-charge drop |
| Click / tap on title screen | Start or restart |

## Gameplay rules

- A maximum of **four depth charges** can be active at once.
- Submarines patrol horizontally and reverse at the playfield edges.
- Submarines periodically launch mines that rise toward the surface.
- A depth charge instantly destroys a submarine on contact.
- If a mine hits the ship, the player loses a life.
- The game progresses in waves and continues until all lives are lost.

## Why this game matters for the engine

Sink Sub validates the engine against a third style of game:

- player movement in a constrained lane
- falling hazards/projectiles
- enemy patrol behavior
- collision-driven scoring and damage
- wave progression without heavy physics

That makes it a good proof that the simplified engine is flexible without becoming complicated.

## Screenshots

| Desktop | Mobile |
|---------|--------|
| ![Desktop loading](../screenshots/sink-sub/desktop-loading.png) | ![Mobile loading](../screenshots/sink-sub/mobile-loading.png) |
| ![Desktop gameplay](../screenshots/sink-sub/desktop-gameplay.png) | ![Mobile gameplay](../screenshots/sink-sub/mobile-gameplay.png) |
