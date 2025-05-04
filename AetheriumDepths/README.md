# Aetherium Depths

## Development Status
Phase 8: Enhanced Combat & Enemy Variety

## Required Actions Before Running
Before you can build and run the game, you'll need to:

1. Create the following image files:
   - `AetheriumDepths/Content/PlayerSprite.png`: A simple 32x32 white square to represent the player
   - `AetheriumDepths/Content/EnemySprite.png`: A simple 32x32 red square to represent enemies
   - `AetheriumDepths/Content/AltarSprite.png`: A 32x32 purple square to represent the weaving altar
   - `AetheriumDepths/Content/ProjectileSprite.png`: A simple 8x8 white circle for projectiles
   - `AetheriumDepths/Content/HealthPotionSprite.png`: A simple 16x16 red square for health potions
   - `AetheriumDepths/Content/GameFont.spritefont`: A SpriteFont file for UI text

## How to Play
- Use WASD or Arrow Keys to move the player character
- Space to perform melee attack
- Q to cast spell (costs mana)
- Left Shift to dodge (temporary invincibility)
- E to interact with objects (like weaving altars)
- When at a weaving altar, press 1 for Damage Buff or 2 for Speed Buff
- R key regenerates the dungeon (for testing)
- P key toggles between Gameplay and Paused states
- M key goes to Main Menu
- Escape key exits the game

## Implementation Progress
See `memory-bank/progress.md` for current implementation status. 