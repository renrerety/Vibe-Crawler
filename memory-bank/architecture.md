# Aetherium Depths Architecture

**Version:** 0.1
**Date:** 2025-05-04
**Phase:** 1 - Project Setup & Basic Architecture

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
  Gameplay/ (Core game logic - placeholder)
  Generation/ (Procedural generation logic - placeholder)
  Entities/ (Base classes/components - placeholder)
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

## 4. Data Flow

1. The `Program.cs` file initializes the `AetheriumGame` instance and starts the game loop.
2. The `AetheriumGame` initializes core systems, including the `StateManager`.
3. During the game loop's `Update` phase, input is processed and:
   - The current state is updated
   - State transitions may occur based on input (e.g., pressing 'P' to pause)
4. During the game loop's `Draw` phase, rendering is performed based on the current active state.

## 5. Future Considerations

As development progresses, we'll expand this architecture to include:
- Player entity and movement systems
- Combat mechanics and collision detection
- Procedural dungeon generation
- Aetherium Weaving (core game mechanic)
- Cross-platform input handling abstractions for mobile support

The modular design established in this initial phase will facilitate these additions while maintaining clean separation of concerns.
