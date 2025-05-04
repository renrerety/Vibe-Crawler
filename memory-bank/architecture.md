# Aetherium Depths Architecture

**Version:** 0.3
**Date:** 2025-05-06
**Phase:** 4 - Procedural Generation (Basic Placeholder)

## 1. Overview

This document outlines the architecture of the Aetherium Depths game, a 2D top-down real-time action roguelike / dungeon crawler. The project uses MonoGame as its core framework and follows a modular design approach to support cross-platform development.

## 2. Project Structure

The project is organized into the following directory structure:

```
AetheriumDepths/ (Root Project Folder)
  AetheriumGame.cs (Main game class)
  Program.cs (Entry point)
  Core/ (Fundamental systems)
    StateManager.cs (Game state management)
    InputManager.cs (Input handling)
    CollisionUtility.cs (Collision detection)
  Gameplay/ (Core game logic - placeholder)
  Generation/ (Procedural generation logic)
    Room.cs (Room representation)
    Dungeon.cs (Dungeon container)
    DungeonGenerator.cs (Dungeon generation logic)
  Entities/ (Base classes/components)
    Player.cs (Player entity)
    Enemy.cs (Enemy entity)
  UI/ (Custom UI elements - placeholder)
  Content/ (MonoGame Content Pipeline definition)
```

Future development will expand this structure to support additional platform targets (Android/iOS) through dedicated projects.

## 3. Key Components

### 3.1. Core Game Loop (AetheriumGame.cs)

The `AetheriumGame` class extends MonoGame's `Game` class and implements the main game loop:
- **Initialize**: Sets up core systems and components
- **LoadContent**: Loads game assets (textures, sounds, etc.)
- **Update**: Processes game logic based on the current state
- **Draw**: Renders the game based on the current state

This main game class uses the StateManager to control which states are active and delegates specific update and draw logic to state-specific methods.

### 3.2. State Management (StateManager.cs)

The `StateManager` class provides a mechanism for handling different game states and transitions between them:

- **States**: MainMenu, Gameplay, Paused
- **Current State**: Tracks which state is active
- **State Transitions**: Handles switching between states with appropriate events

The state manager allows the game to have distinct behaviors in different states while maintaining a clean separation of concerns. It uses an event system to notify subscribers of state changes.

### 3.3. Input Management (InputManager.cs)

The `InputManager` class abstracts input handling across different devices:

- **Input Mapping**: Maps raw input (keyboard, gamepad) to game actions
- **GameAction Enum**: Defines all possible game actions (MoveUp, MoveDown, MoveLeft, MoveRight, Attack, Dodge, Interact)
- **Movement Vector**: Provides a normalized vector for player movement

This abstraction simplifies cross-platform development and allows for easy control scheme reconfiguration.

### 3.4. Player Entity (Player.cs)

The `Player` class handles player-specific behaviors:

- **Position and Sprite**: Basic rendering properties
- **Movement**: Direction-based movement with speed control
- **Attack System**: Temporary hitbox creation in facing direction
- **Dodge System**: Temporary speed boost and invincibility
- **State Management**: Tracking attack and dodge states with timers

### 3.5 Combat System

The combat system consists of several components working together:

- **Collision Detection**: AABB (Axis-Aligned Bounding Box) collision checks via CollisionUtility
- **Attack Mechanics**: Temporary hitboxes created in player's facing direction
- **Damage System**: Health tracking and damage application to enemies
- **Invincibility Frames**: Temporary invulnerability during dodge actions

### 3.6 Dungeon Generation System

The dungeon generation system creates and manages the game's level structure:

- **Room Class**: Stores room bounds and provides utility methods like finding the center
- **Dungeon Class**: Contains a collection of rooms and identifies the starting room
- **DungeonGenerator**: Creates a predefined layout of rooms (currently hardcoded, but designed for future procedural expansion)
- **Rendering**: Rooms are rendered as outlines during gameplay
- **Player Spawning**: Player spawns in the center of the first room
- **Enemy Placement**: Enemies are positioned in rooms for encounters

#### Dungeon Generation Approach

The current implementation uses a simplified placeholder approach:
1. The `DungeonGenerator` creates a `Dungeon` with 4 hardcoded rooms
2. Each `Room` is defined by a `Rectangle` specifying its position and size
3. Rooms are validated to ensure they fit within the viewport
4. The player is positioned in the center of the first room
5. The enemy is positioned in the center of the last room

The placeholder system is designed to be replaced with proper procedural generation in future iterations, maintaining the same interface so game code won't need major changes.

## 4. Data Flow

1. The `Program.cs` file initializes the `AetheriumGame` instance and starts the game loop.
2. The `AetheriumGame` initializes core systems, including the `StateManager`, `InputManager`, and `DungeonGenerator`.
3. When entering the `Gameplay` state, the `DungeonGenerator` creates a new dungeon layout.
4. The player is positioned in the starting room, and enemies are placed in other rooms.
5. During the game loop's `Update` phase:
   - The `InputManager` processes raw input and maps to game actions
   - The player moves and performs actions based on input
   - Collision detection checks for attacks hitting enemies
   - The current state is updated, and state transitions may occur
6. During the game loop's `Draw` phase:
   - The dungeon layout is rendered as room outlines
   - Entities (player, enemies) are drawn
   - Debug visuals like attack hitboxes are displayed
   - Rendering is performed based on the current active state

## 5. Future Considerations

As development progresses, we'll expand this architecture to include:
- True procedural dungeon generation algorithms
- Enemy AI and behavior
- Aetherium Weaving (core game mechanic)
- Cross-platform input handling abstractions for mobile support

The modular design established in this initial phase will facilitate these additions while maintaining clean separation of concerns.
