# Breakout gameplay guide

## Overview

Breakout is the classic arcade brick-breaker:

- keep the ball in play
- break every brick
- catch falling power-ups
- survive with your remaining lives

## Objective

Clear the full brick field before losing all three lives.

## Controls

| Input | Action |
|------|--------|
| Mouse / touch move | Move the paddle |
| Click / tap on title screen | Start or restart |

## Gameplay rules

- The ball bounces off the paddle, bricks, and top/side walls.
- Falling below the bottom of the screen costs one life.
- The paddle angle influences the outgoing ball direction.
- Bricks score more points in higher rows.
- Some bricks spawn power-ups:
  - **Strong Ball** - pierces through bricks for a short time
  - **Big Paddle** - enlarges the paddle temporarily

## Engine patterns used

Breakout is the clearest example of the engine's core loop:

1. move the ball with `Rigidbody2D.Step(...)`
2. detect collisions with `CollisionResolver`
3. decide what the hit means
4. bounce with `Rigidbody2D.Bounce(...)`
5. update score, lives, and power-up state

## Screenshots

| Desktop | Mobile |
|---------|--------|
| ![Desktop loading](../screenshots/breakout/desktop-loading.png) | ![Mobile loading](../screenshots/breakout/mobile-loading.png) |
| ![Desktop gameplay](../screenshots/breakout/desktop-gameplay.png) | ![Mobile gameplay](../screenshots/breakout/mobile-gameplay.png) |
