# Aetherium Depths Architecture

**Version:** 0.5
**Date:** 2025-05-08
**Phase:** 5 - Aetherium Weaving (Core Hook - Minimal) + Preparing for Next Phase

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
    WeavingAltar.cs (Aetherium Weaving altar)
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

- **States**: MainMenu, Gameplay, Paused, GameOver
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
- **Health System**: CurrentHealth and MaxHealth tracking with damage handling
- **Attack System**: Temporary hitbox creation in facing direction
- **Dodge System**: Temporary speed boost and invincibility
- **Aetherium Essence**: Resource used for weaving abilities
- **Buff System**: Tracks active buffs (currently only damage buff)
- **State Management**: Tracking attack, dodge, and buff states
- **Visual Feedback**: Visual indicators for damage, invincibility, and buff states

### 3.5 Combat System

The combat system consists of several components working together:

- **Collision Detection**: AABB (Axis-Aligned Bounding Box) collision checks via CollisionUtility
- **Attack Mechanics**: Temporary hitboxes created in player's and enemy's facing direction
- **Damage System**: Health tracking and damage application to enemies and player
- **Invincibility Frames**: Temporary invulnerability during dodge actions and after taking damage
- **Death Handling**: GameOver state transition when player health reaches zero

### 3.6 Enemy Entity (Enemy.cs)

The `Enemy` class handles enemy-specific behaviors:

- **Basic Properties**: Position, sprite, health, and active state
- **AI Movement**: Detection range and player-seeking behavior
- **AI Combat**: Attack range, cooldown, and attack hitbox generation
- **State Management**: Tracking attack state and cooldowns
- **Death Handling**: Resource rewards and deactivation upon defeat

### 3.7 Game State Flow

The game state flow has been expanded to include:

- **Main Menu**: Starting point for the game (currently placeholder)
- **Gameplay**: Active gameplay with player control and enemy interactions
- **Paused**: Temporarily halts gameplay updates but maintains visual state
- **Game Over**: Displayed when player health reaches zero, with restart option

**Game Over Flow**:
1. Player health reaches zero
2. StateManager transitions to GameOver state
3. Game halts normal updates but displays Game Over screen
4. Player can press 'E' to restart
5. Game resets player health, enemy state, and regenerates dungeon
6. StateManager transitions back to Gameplay state

### 3.8 Dungeon Generation System

The dungeon generation system creates and manages the game's level structure:

- **Room Class**: Stores room bounds and provides utility methods like finding the center
- **Dungeon Class**: Contains a collection of rooms and corridors and identifies the starting room
- **BSPNode Class**: Represents nodes in the Binary Space Partitioning tree used for procedural generation
- **DungeonGenerator**: Creates procedurally generated dungeons using a BSP algorithm
- **Rendering**: Rooms and corridors are rendered during gameplay
- **Player Spawning**: Player spawns in the center of the first room
- **Enemy Placement**: Enemies are positioned in rooms for encounters
- **Altar Placement**: Weaving altar is positioned in a separate room

#### BSP-based Procedural Generation

The game now uses a Binary Space Partitioning (BSP) approach for procedural dungeon generation:

1. **BSP Tree Creation**:
   - Start with a single rectangular space representing the entire dungeon area
   - Recursively split this space either horizontally or vertically
   - Split direction is determined by area aspect ratio or random choice
   - Stop splitting when minimum leaf size is reached or max depth is attained
   - Each leaf node in the resulting tree represents a potential room location

2. **Room Generation**:
   - For each leaf node, create a room that fits within its boundaries
   - Room size is determined by a percentage of the leaf size (with minimum constraints)
   - Room position is randomly determined within the leaf boundaries
   - This approach ensures rooms don't overlap while maintaining organic placement

3. **Corridor Connection**:
   - Corridors connect rooms based on the BSP tree structure
   - Sibling nodes in the tree have their rooms connected
   - Connections use an L-shaped corridor system (horizontal + vertical segments)
   - Corridor width is configurable for gameplay balance

4. **Entity Placement**:
   - The player starts in the first room (index 0)
   - Enemies are placed in rooms farthest from the player's starting position
   - The weaving altar is placed in a centrally-located room
   - This ensures a progression in the game's exploration path

#### Visualization and Debugging

The system includes debugging visualization features:
- Leaf node boundaries are rendered in green
- Room boundaries are rendered in white with semi-transparent fills
- Corridors are rendered in gray
- The regeneration key (R) allows runtime testing of different dungeon layouts

This procedural approach replaced the initial placeholder system that used hardcoded room positions, offering greater variety and expandability.

### 3.9 Aetherium Weaving System

The Aetherium Weaving system is the core game mechanic that allows players to spend collected essence to gain temporary buffs:

- **Aetherium Essence**: A resource collected from defeated enemies
- **Weaving Altar**: An interactive object where weaving can be performed
- **Interaction System**: AABB collision-based detection for altar interaction
- **Buff System**: Multiple buff types (damage, speed) with independent durations
- **Visual Feedback**: Color changes on the player sprite to indicate active buffs
- **Buff UI**: Simple interface elements showing active buffs and their remaining duration

#### Weaving Implementation

The current implementation provides a functional foundation for the Aetherium Weaving system:

1. The `Player` class tracks `AetheriumEssence` collected from defeated enemies
2. Defeating an enemy grants a fixed amount of essence
3. The `WeavingAltar` class represents a physical location where weaving can occur
4. When the player is near the altar and presses the Interact key (E), a buff selection is presented
5. Pressing a number key (1 for damage, 2 for speed) while interacting will activate the corresponding buff
6. If the player has sufficient essence, it's deducted and the selected buff is activated with a duration
7. The active buffs apply modifiers to player capabilities:
   - Damage buff: Multiplies attack damage
   - Speed buff: Increases movement speed
8. Buffs expire automatically after their duration ends
9. The player's sprite changes color to indicate buff status:
   - Yellow: Damage buff active
   - Green: Speed buff active
   - Orange: Both buffs active
10. UI elements display active buffs with their remaining durations

#### Buff Management System

The buff management system supports independent tracking of multiple buff types:

1. Each buff type has a dedicated duration timer tracked in the `Player` class
2. Duration timers are decremented each frame using delta time for frame-rate independence
3. When a timer reaches zero, the corresponding buff is automatically deactivated
4. The `BuffType` enum allows for easy identification and future expansion of buff types
5. The `GetBuffDuration` method provides remaining time for display in the UI
6. Visual indicators change based on the combination of active buffs

## 4. Data Flow

1. The `Program.cs` file initializes the `AetheriumGame` instance and starts the game loop.
2. The `AetheriumGame` initializes core systems, including the `StateManager`, `InputManager`, and `DungeonGenerator`.
3. When entering the `Gameplay` state, the `DungeonGenerator` creates a new dungeon layout.
4. The player is positioned in the starting room, enemies are placed in other rooms, and the weaving altar is positioned in a middle room.
5. During the game loop's `Update` phase:
   - The `InputManager` processes raw input and maps to game actions
   - The player moves and performs actions based on input
   - Collision detection checks for attacks hitting enemies
   - When enemies are defeated, Aetherium Essence is granted to the player
   - Interaction with the weaving altar is detected and processed
   - The current state is updated, and state transitions may occur
6. During the game loop's `Draw` phase:
   - The dungeon layout is rendered as room outlines
   - The weaving altar is drawn with a distinctive color
   - Entities (player, enemies) are drawn with appropriate visual state indicators
   - Debug visuals like attack hitboxes are displayed
   - Rendering is performed based on the current active state

## 5. Future Architecture Expansion

### 5.1 Enhanced Entity Component System

For the next phase of development, we plan to transition toward a more formalized Entity Component System (ECS) architecture to improve scalability and maintainability:

- **Entity Manager**: Central registry for all game entities with unique IDs
- **Component System**: Modular components that can be attached to entities:
  - TransformComponent (position, rotation, scale)
  - RenderComponent (sprite, animations, effects)
  - CollisionComponent (hitboxes, collision response)
  - CombatComponent (health, damage, attack patterns)
  - AIComponent (behavior trees, pathfinding)
  - BuffComponent (active effects, durations)
- **System Processors**: Dedicated systems that process specific component types:
  - RenderSystem (handles drawing all entities with RenderComponents)
  - PhysicsSystem (manages collision detection and response)
  - AISystem (updates AI behaviors and decisions)
  - BuffSystem (manages buff application, duration, and removal)

This approach will allow for greater code reuse, easier addition of new entity types, and better performance through more targeted system updates.

### 5.2 Procedural Generation Enhancement

The next iteration of the dungeon generation system will include:

- **Room Type System**: Different room templates based on purpose (combat, treasure, boss, altar)
- **Corridor Generator**: Methods to connect rooms with logical pathways
- **Biome System**: Different visual and gameplay themes for dungeon areas
- **Constraint-Based Generation**: Rules to ensure playable and balanced layouts
- **Seed-Based Generation**: Support for reproducible random dungeons

The enhanced system will maintain the existing interface (GenerateDungeon method returning a Dungeon object) but with significantly more sophisticated internal algorithms.

### 5.3 UI Framework

A dedicated UI framework will be implemented to handle:

- **UI State Management**: Separate from but coordinated with the main game state
- **Component-Based UI**: Reusable elements (buttons, panels, sliders)
- **Layout System**: Responsive positioning for different screen sizes
- **Animation System**: Smooth transitions and effects for UI elements
- **Input Handling**: UI-specific input processing with event propagation

The UI framework will be designed with mobile touch input in mind from the start to ensure good UX across all platforms.

### 5.4 Asset Management System

To support dynamic loading/unloading and optimize resource usage:

- **Asset Cache**: Smart caching of frequently used resources
- **Lazy Loading**: On-demand loading of assets when needed
- **Asset Bundles**: Grouped assets for efficient loading/unloading
- **Memory Management**: Automatic cleanup of unused resources
- **Platform-Specific Asset Variants**: Different resolution/format assets per platform

### 5.5 Save/Load System

A robust save/load system to persist player progress:

- **Serialization Framework**: Efficient binary or JSON serialization
- **Save Slots**: Support for multiple save files
- **Progressive Saving**: Save critical data frequently, full state less often
- **Cross-Platform Compatibility**: Ensure saves work across different devices
- **Cloud Sync**: Preparation for future cloud save integration

## 6. Technical Debt and Refactoring Needs

As we prepare for the next phase, we've identified several areas that require refactoring to ensure long-term maintainability:

### 6.1 State Management Refactoring

The current state management implementation is simple but will become limiting as game complexity increases:

- **State Stack**: Replace single state tracking with a stack to support nested states
- **State Transitions**: Add proper transition animations and effects
- **Sub-States**: Support for states within states (e.g., inventory UI within gameplay)
- **State Persistence**: Maintain relevant state data when switching between states

### 6.2 Decoupling Game Logic

Some game systems are currently too tightly coupled:

- **Player-Combat Decoupling**: Move combat logic out of the Player class into a dedicated system
- **Entity-Rendering Separation**: Split entity data from rendering logic
- **State-Specific Logic Isolation**: Better encapsulation of state-specific behavior

### 6.3 Configuration Externalization

Currently, many game parameters are hardcoded:

- **External Configuration Files**: Move constants to JSON/XML configuration
- **Runtime Configuration**: Support for changing settings without recompilation
- **Platform-Specific Defaults**: Different default settings per platform

### 6.4 Input System Enhancement

The input system needs expansion for full cross-platform support:

- **Touch Input Abstractions**: Gesture recognition and mapping
- **Input Context Awareness**: Different input mappings based on game state
- **Advanced Input Buffering**: Improved response to rapid/queued inputs

## 7. System Integration for Next Phase

### 7.1 Integration Points

The following systems will need careful integration in the next phase:

- **Player Health & Enemy Combat**: Player damage-taking, death states, and game over conditions
- **Procedural Generation & Game Progression**: Room difficulty scaling, level transitions
- **Buff System & UI**: Visual representation of active buffs, timers, and essence count
- **Essence Types & Weaving Interface**: More complex resource management and interaction

### 7.2 Cross-Cutting Concerns

Several aspects affect multiple systems and require coordinated implementation:

- **Performance Monitoring**: Tracking frame rates and identifying bottlenecks
- **Debug Tools**: In-game debugging features for testing procedural generation
- **Error Handling**: Robust error recovery to prevent crashes
- **Accessibility**: Ensuring the game is playable by users with different abilities

### 7.3 Testing Strategy

As complexity increases, a more formalized testing approach is needed:

- **Unit Tests**: For core algorithmic components like procedural generation
- **Integration Tests**: For ensuring systems work together correctly
- **Automated Playtesting**: Scripts to simulate common player actions
- **Performance Benchmarks**: Tests to ensure good performance across target devices

## 8. Conclusion

The Aetherium Depths architecture has established a solid foundation through the first five phases of development. The modular design approach taken from the beginning has positioned us well for the upcoming expansion. Our focus for the next phase will be on enhancing the procedural generation, expanding the combat system, completing the Aetherium Weaving mechanics, and implementing core UI elements.

By addressing the technical debt areas identified and carefully planning the integration of new systems, we will maintain code quality while delivering the expanded feature set. The architectural improvements outlined in this document will guide our development efforts and ensure the game remains scalable and maintainable as it grows in complexity.
