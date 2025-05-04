Okay, I've updated the GDD to reflect the platform targets and defer the specific technology choices.

```markdown
# Game Design Document: Aetherium Depths

**Version:** 0.2
**Date:** 2023-10-27

## 1. Overview

**Title:** Aetherium Depths (Working Title)

**Genre:** 2D Top-Down Real-Time Action Roguelike / Dungeon Crawler

**Elevator Pitch:** Delve into the ever-shifting `Aetherium Depths`, battling monstrous creatures and challenging bosses in fast-paced, real-time combat. Collect powerful equipment, spells, and skills within randomly generated dungeons. Utilize the unique **Aetherium Weaving** system during runs to temporarily enhance your abilities, and invest persistent resources in the Hub to grow stronger for future expeditions.

**Target Audience:** Fans of roguelikes/lites (Hades, Enter the Gungeon, Dead Cells, Binding of Isaac), action RPGs, and players seeking challenging, replayable experiences with progression on PC and Mobile.

## 2. Core Gameplay Loop

1.  **Prepare (Hub):** Spend meta-currency, talk to NPCs, select starting loadout (if applicable).
2.  **Enter Dungeon:** Start a new run in a randomly generated dungeon floor.
3.  **Explore:** Navigate rooms, avoiding traps and discovering secrets.
4.  **Combat:** Engage enemies in real-time combat using weapons, spells, and skills.
5.  **Loot & Enhance:** Defeat enemies and bosses to gain equipment, spells, skills, and temporary resources (`Aetherium Essence`). Use `Aetherium Weaving` to augment gear/abilities mid-run.
6.  **Boss Battle:** Defeat the floor boss to progress deeper or complete the run segment.
7.  **Death / Victory:**
    *   **Death:** Lose run-specific progress (loot, temporary enhancements), return to Hub with collected meta-currency (`Echo Shards`).
    *   **Victory (Segment/Full):** Return to Hub with collected meta-currency and potentially unlock new content.
8.  **Meta Progression (Hub):** Spend `Echo Shards` to unlock permanent upgrades, new items/skills for future runs, or cosmetic changes.
9.  **Repeat.**

## 3. Key Features

### 3.1. Randomly Generated Dungeons
*   Each run features uniquely generated dungeon layouts composed of interconnected rooms.
*   Room types include: Combat Arenas, Treasure Rooms, Shops (mid-run vendors), Mini-boss Rooms, Puzzle/Trap Rooms, Secret Rooms, Boss Arena.
*   Increasing difficulty and different environmental themes/hazards on deeper floors.

### 3.2. Real-Time Combat
*   Top-down perspective.
*   Player controls a single character.
*   **Core Actions:** Move, Basic Attack (weapon dependent), Dodge/Dash (invincibility frames), Use Spell/Skill (cooldown/resource based). Controls adapted for both PC (Keyboard/Mouse/Controller) and Mobile (Touchscreen).
*   Variety of enemies with distinct attack patterns, behaviours, and resistances/vulnerabilities.
*   Challenging multi-stage boss encounters at the end of each major dungeon section.

### 3.3. Loot & Progression (In-Run)
*   **Equipment:** Weapons (melee/ranged), Armor (defense/utility), Accessories (passive effects). Stat variations and unique modifiers. Rarity tiers (Common, Uncommon, Rare, Epic, Legendary).
*   **Spells:** Active abilities consumed via mana or cooldowns (e.g., Fireball, Heal, Teleport). Found or bought during runs.
*   **Skills:** Passive buffs or triggered effects (e.g., increased crit chance, life steal on hit, chance to stun).

### 3.4. Unique Selling Proposition: Aetherium Weaving
*   **Concept:** Players collect `Aetherium Essence` (a temporary, run-specific resource) from defeated enemies and special nodes.
*   **Mechanic:** At specific "Weaving Altars" found within the dungeon, or potentially via a dedicated UI button with cooldown/cost, players can spend `Aetherium Essence` to **temporarily** augment their *current* equipment, spells, or skills for the *duration of the run*.
*   **Examples:**
    *   Weave onto Sword: Adds temporary fire damage or increased attack speed.
    *   Weave onto Fireball Spell: Increases AoE radius or adds a burning DoT effect.
    *   Weave onto Dodge Skill: Leaves behind a damaging trail or grants a brief speed boost after dodging.
*   **Differentiation:** This adds a layer of dynamic, run-specific crafting and decision-making *beyond* just finding loot. Players must choose how to best spend their limited `Aetherium Essence` based on their current build and upcoming challenges. These enhancements are powerful but *always* lost upon run completion (death or victory), ensuring each run feels different.

### 3.5. Meta Progression
*   Players collect `Echo Shards` (persistent meta-currency) primarily from bosses and special encounters.
*   Spend `Echo Shards` in the Hub to unlock:
    *   **Permanent Stat Upgrades:** Small increases to base health, damage, resource regeneration, etc.
    *   **New Item/Spell/Skill Unlocks:** Adds new potential items to the loot pool found within dungeons.
    *   **Starting Bonuses:** Unlock different starting weapons or minor advantages.
    *   **Hub Upgrades:** Unlock new vendors or enhance existing ones.
    *   **Gameplay Modifiers:** Potential difficulty modifiers or challenge run options (e.g., Glass Cannon mode).

### 3.6. The Hub
*   A safe zone between runs.
*   **Dungeon Entrance:** Portal or gateway to start a new run.
*   **NPC Vendors:**
    *   **The Runesmith:** Spends `Echo Shards` for permanent upgrades.
    *   **The Curator:** Unlocks new items/spells/skills for the dungeon loot pool using `Echo Shards`.
    *   **The Cartographer (Unlockable):** Provides insight into dungeon layouts or potential starting boosts for a cost.
    *   **The Stylist (Cosmetic/Optional):** Change character appearance (if applicable).
*   **Codex/Bestiary:** Tracks discovered items, enemies, lore snippets.
*   **Training Dummy:** Allows testing of basic attacks and abilities.

## 4. Art Style & Audio

*   **Art:** Clean 2D visuals. Pixel art or stylized vector art. Focus on clear visual feedback for attacks, effects, and enemy tells. Atmospheric lighting and distinct biome aesthetics. UI design must be scalable and usable on both PC and smaller mobile screens.
*   **Audio:** Satisfying combat sound effects (hits, spells, dodges). Distinct audio cues for enemy attacks. Atmospheric ambient tracks for exploration, ramping up for combat encounters. Epic boss music.

## 5. Technology

*   **Platform:** PC (Windows, potentially macOS/Linux) and Mobile (iOS, Android).
*   **Engine & Specific Tech Stack:** To be determined and detailed in a separate technical design document. Considerations must include cross-platform capabilities and performance optimization for mobile devices.

## 6. Monetization (Assumed)

*   **PC:** Premium one-time purchase.
*   **Mobile:** Flexible - potentially Premium, Freemium (Ads + IAP for cosmetics/convenience, *not* pay-to-win), or Demo + Full Game Unlock. Monetization strategy needs further definition.
*   No microtransactions affecting core gameplay balance across any platform. Potential for future cosmetic DLC or larger content expansions.

```

**Changes Made:**

*   Updated **Target Audience** to mention PC and Mobile.
*   Added a note in **Real-Time Combat** about adapting controls for both platforms.
*   Updated **Art Style** to mention UI scalability for different screen sizes.
*   Removed specific engine suggestions from the **Technology** section.
*   Explicitly listed **PC** and **Mobile (iOS, Android)** as target platforms.
*   Added a note that the specific Engine/Tech Stack will be defined elsewhere.
*   Adjusted the **Monetization** section to reflect potential differences between PC and Mobile models while maintaining the core principle of no pay-to-win.