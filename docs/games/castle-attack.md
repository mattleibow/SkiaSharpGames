# Castle Attack gameplay guide

## Overview

Castle Attack is a siege-defense game inspired by the Stronghold promotional mini-game concept:

- archers defend the walls
- workers build the keep
- enemies attack in waves
- special weapons can save a bad round

## Objective

Finish building the keep before the castle defenses fail and the lord is defeated.

## Controls

| Input | Action |
|------|--------|
| Left / Right arrows | Adjust aim |
| Space | Fire a volley |
| Up arrow | Convert a worker into an archer |
| Down arrow | Convert an archer into a worker |
| Z | Boiling oil |
| X | Mangonel strike |
| C | Flaming logs |
| Touch controls | Mobile equivalents for aiming, firing, and specials |

## Gameplay rules

- Three defensive wall lines stand between the enemy and the keep.
- Archers fire along a shared trajectory, so aim management matters.
- Workers increase keep construction speed but reduce immediate defense.
- At least one worker must remain.
- If all walls fall, the lord makes a last stand.
- Unused special weapons are strategically valuable because they can turn a wave instantly.

## Enemy roster

The prototype includes several of the intended enemy roles from the design brief:

- fast light infantry
- heavier melee units
- ranged attackers
- siege attackers
- special event cow target

## Engine patterns used

Castle Attack stresses the engine differently from Breakout:

- multiple active entity lists
- timed spawns and cooldowns
- simple projectile motion
- collider checks for arrow and enemy interactions
- overlay screens for victory and defeat

## Screenshots

| Desktop | Mobile |
|---------|--------|
| ![Desktop loading](../screenshots/castle-attack/desktop-loading.png) | ![Mobile loading](../screenshots/castle-attack/mobile-loading.png) |
| ![Desktop gameplay](../screenshots/castle-attack/desktop-gameplay.png) | ![Mobile gameplay](../screenshots/castle-attack/mobile-gameplay.png) |
