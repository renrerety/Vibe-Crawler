Okay, AI developers, let's build the core foundation for "Aetherium Depths". Follow these steps precisely. Remember to consult `@game-design-document.md` and `@tech-stack.md` frequently, and adhere to the modularity principle. Always read `@architecture.md` before coding and update it after significant milestones.

## Implementation Plan: Aetherium Depths - Core Systems MVP

**Phase 1: Project Setup & Basic Architecture**

1.  **Step 1.1: Create MonoGame Project**
    *   **Instruction:** Initialize a new MonoGame Cross-Platform Desktop Project (using .NET Core) targeting C#. Name it `AetheriumDepths`. Ensure mobile project heads (iOS/Android) are also set up if the template supports it, or note this for later setup. Configure the project to be usable within the Cursor IDE. 
    
        Establish the following directory structure from the start for modularity:
        ```
        AetheriumDepths/ (Root Solution Folder)
          AetheriumDepths.Core/ (Main Shared Project/Core Logic - PCL or .NET Standard)
            Game1.cs (Or renamed main class, e.g., AetheriumGame.cs)
            Program.cs (Entry point, if separate)
            Core/ (Fundamental systems: StateManager, InputManager abstractions, Camera)
            Gameplay/ (Core game logic: Dungeon, Room, Player, Enemy, CombatManager, WeavingManager)
            Generation/ (Procedural generation logic: DungeonGenerator)
            Entities/ (Base classes/components for Player, Enemy, Items)
            UI/ (Custom UI elements and management - initially simple)
            Content/ (MonoGame Content Pipeline definition - .mgcb file)
          AetheriumDepths.Desktop/ (Desktop Platform Project - Windows/Linux/macOS)
            (Contains platform-specific bootstrap code, references Core)
            Content/ (Compiled content output for Desktop)
          AetheriumDepths.Mobile.Android/ (Android Platform Project)
            (Contains Android-specific bootstrap, Activity, references Core)
            Assets/Content/ (Compiled content output for Android)
          AetheriumDepths.Mobile.iOS/ (iOS Platform Project)
            (Contains iOS-specific bootstrap, AppDelegate, references Core)
            Content/ (Compiled content output for iOS)
        ```
        Place new classes into the relevant folders/namespaces from the start.
    *   **Validation:** The solution builds successfully for the Desktop target. Running the project opens a blank Cornflower Blue window.

2.  **Step 1.2: Implement Basic Game Loop Structure**
    *   **Instruction:** Inside the main `Game1.cs` (or appropriately renamed main game class), identify the core `Initialize`, `LoadContent`, `Update`, and `Draw` methods. Add simple logging (console output) inside `Update` and `Draw` to confirm they are being called each frame.
    *   **Validation:** Running the game shows continuous log messages from both `Update` and `Draw` methods in the console output.

3.  **Step 1.3: Introduce Basic State Management**
    *   **Instruction:** Create a simple state management system. Define an `enum` for game states (e.g., `MainMenu`, `Gameplay`, `Paused`). Implement logic in `Game1` to track the current state and call different `Update` and `Draw` methods based on this state (initially, just placeholder methods or basic logging). Start the game in the `Gameplay` state for now.
    *   **Validation:** The game runs, and logs indicate it is executing the `Update` and `Draw` logic associated with the `Gameplay` state. Ability to programmatically change the state (even via a debug key) and see the logs change confirms the manager works.
    *   **Architecture Documentation:** Create the initial content of `@architecture.md`. Document the basic project structure (directories, core classes like Game1), the initial state machine, chosen namespaces, and fundamental data flow established in Phase 1. This will be a living document that you will update as development progresses.

**Phase 2: Player Entity & Basic Movement**

1.  **Step 2.1: Create Player Class**
    *   **Instruction:** Create a new class `Player`. Give it basic properties: `Vector2 Position`, `Texture2D Sprite`. In `Game1`'s `LoadContent`, load a placeholder square or circle texture for the player. In `Game1`'s `Gameplay` state `Draw` method, draw the player sprite at its position using `SpriteBatch`. Instantiate the Player in `Initialize` or `LoadContent`. 
    
        For placeholder assets, create a basic PNG image (e.g., a white 32x32 square for the player). Add this PNG to the Content.mgcb file in the shared Core project and ensure it is processed by the Content Pipeline. Load it using Content.Load<Texture2D>("PlayerSprite").
    *   **Validation:** Running the game shows the player's placeholder sprite drawn on the screen within the game window when in the `Gameplay` state.

2.  **Step 2.2: Implement Input Manager & Keyboard Input Handling**
    *   **Instruction:** Create an `InputManager` class in the Core/ directory. This manager should read raw inputs (Keyboard, Mouse, Gamepad for Desktop; Touch for Mobile). Define abstract actions using an enum:
        ```csharp
        public enum GameAction { MoveUp, MoveDown, MoveLeft, MoveRight, Attack, Dodge, Interact }
        ```
        The InputManager's primary role is to map the raw inputs to these GameActions based on the current platform and control scheme. Implement methods like:
        ```csharp
        public bool IsActionPressed(GameAction action)
        public bool IsActionJustPressed(GameAction action)
        public Vector2 GetMovementVector()
        ```
        For now, implement the keyboard input handling within the InputManager. Player and other systems should query the InputManager for actions, not directly read Keyboard.GetState().
    *   **Validation:** Running the game and pressing WASD/Arrow keys results in the correct GameActions being identified via the InputManager.

3.  **Step 2.3: Implement Basic Player Movement**
    *   **Instruction:** In the `Gameplay` state `Update` method, use the InputManager's GetMovementVector() to get the intended direction. Apply this movement direction vector to the `Player.Position`. Multiply by a simple speed variable and `gameTime.ElapsedGameTime.TotalSeconds` for frame-rate independent movement. Store this movement vector in a `Player.LastMovementDirection` property when it's non-zero, as this will be used for attack direction later.
    *   **Validation:** Running the game allows the player sprite to be moved around the screen using the specified keys. Movement speed feels consistent regardless of frame rate fluctuations.

4.  **Step 2.4: Implement Screen Boundaries**
    *   **Instruction:** After calculating the new position in `Update`, clamp the `Player.Position` so the sprite stays fully within the visible screen area (consider sprite dimensions). Use `GraphicsDevice.Viewport.Bounds` to get screen dimensions.
    *   **Validation:** The player sprite stops at the edges of the game window and cannot be moved off-screen.
    *   **Architecture Update:** Update `@architecture.md` to reflect the InputManager abstraction and how it integrates with the Player class.

**Phase 3: Basic Combat Fundamentals**

1.  **Step 3.1: Implement AABB Collision Function**
    *   **Instruction:** Create a static utility class or method that takes two `Rectangle` objects (representing entity bounds) and returns `true` if they intersect (overlap), `false` otherwise. This implements the Axis-Aligned Bounding Box check specified in `@tech-stack.md`.
    *   **Validation:** Write unit tests or use debug commands to call this function with known overlapping and non-overlapping rectangles, verifying it returns the correct boolean result in all cases.

2.  **Step 3.2: Implement Basic Attack Action (Melee)**
    *   **Instruction:** Using the InputManager, detect when the Attack action is pressed. When triggered, create a temporary `Rectangle` representing a short melee attack hitbox in front of the player based on the player's `LastMovementDirection` value. For example, if LastMovementDirection is (1,0), place the hitbox to the right of the player. Set a short timer for the hitbox's duration. Draw this hitbox for debugging.
    *   **Validation:** Pressing the attack key briefly displays the debug rectangle representing the hitbox near the player, positioned correctly based on the last movement direction.

3.  **Step 3.3: Create Basic Enemy Class**
    *   **Instruction:** Create a class `Enemy`. Give it `Vector2 Position`, `Texture2D Sprite`, `Rectangle Bounds`, and `int Health`. Load a different placeholder sprite (e.g., a red 32x32 square). Instantiate one enemy at a fixed position on the screen during `Gameplay` state initialization. Draw the enemy and update its `Bounds` rectangle based on its position.
    *   **Validation:** Running the game shows both the player and the enemy sprite on screen.

4.  **Step 3.4: Implement Attack Collision & Damage**
    *   **Instruction:** In the `Gameplay` `Update` loop, if the player's attack hitbox is active, check for collision with the enemy's `Bounds` using the AABB function (Step 3.1). If collision occurs, reduce the `Enemy.Health` by a fixed amount and immediately deactivate the *current* attack hitbox (to prevent multiple hits from one swing).
    *   **Validation:** Moving the player near the enemy and attacking causes the enemy's health value (log to console or display on screen for debug) to decrease.

5.  **Step 3.5: Implement Enemy Death**
    *   **Instruction:** In the `Gameplay` `Update` loop, check if `Enemy.Health` is less than or equal to 0. If it is, mark the enemy for removal (e.g., set an `IsActive` flag to false). Modify the drawing logic to not draw inactive enemies.
    *   **Validation:** After attacking the enemy enough times, its sprite disappears from the screen.

6.  **Step 3.6: Implement Basic Dodge Action**
    *   **Instruction:** Using the InputManager, detect when the Dodge action is pressed. When triggered, initiate a brief "dodging" state for the player. During this state, move the player rapidly in their current `LastMovementDirection` for a short duration. Add a boolean flag `IsInvincible` to the Player, set to `true` during the dodge duration.
    *   **Validation:** Pressing the dodge key causes the player sprite to perform a quick dash. Temporarily modify the collision check (Step 3.4) to ignore hits if `Player.IsInvincible` is true, confirming invincibility works.
    *   **Architecture Update:** Update `@architecture.md` to document the basic combat system, including attack, collision, and dodge mechanics.

**Phase 4: Procedural Generation (Basic Placeholder)**

1.  **Step 4.1: Define Room/Dungeon Data Structures**
    *   **Instruction:** Create simple classes/structs: `Room` (containing a `Rectangle` for its bounds) and `Dungeon` (containing a `List<Room>`).
    *   **Validation:** The code containing these data structures compiles successfully.

2.  **Step 4.2: Implement Simplistic Dungeon Generator**
    *   **Instruction:** Create a `DungeonGenerator` class. Implement a basic method that creates a `Dungeon` object containing a small, fixed number (e.g., 3-5) of `Room` objects with hardcoded sizes and positions. Do not implement any random placement or complex algorithms yet. For example:
    ```csharp
    Rooms.Add(new Room(new Rectangle(100, 100, 200, 150)));
    Rooms.Add(new Room(new Rectangle(400, 100, 150, 200)));
    Rooms.Add(new Room(new Rectangle(250, 350, 180, 180)));
    ```
    Ensure these rooms fit within logical game boundaries.
    *   **Validation:** Calling the generation method returns a `Dungeon` object containing the expected number of `Room` objects with valid `Rectangle` data.

3.  **Step 4.3: Render Generated Dungeon Layout**
    *   **Instruction:** In the `Gameplay` state, call the `DungeonGenerator` upon entering the state. Store the returned `Dungeon`. In the `Draw` method, iterate through the `Dungeon.Rooms` list and draw each room's `Rectangle` outline using basic line drawing functions or a simple border texture. Use a `Camera` or simple offset for positioning if needed.
    *   **Validation:** Running the game displays the outlines of the generated rooms on the screen.

4.  **Step 4.4: Spawn Player in Starting Room**
    *   **Instruction:** Modify the `Gameplay` state initialization to set the `Player.Position` to the center of the first room generated by the `DungeonGenerator`.
    *   **Validation:** The player sprite now appears inside the boundary of the first generated room outline.
    *   **Architecture Update:** Update `@architecture.md` to document the dungeon generation approach and how rooms are represented and rendered.

**Phase 5: Aetherium Weaving (Core Hook - Minimal)**

1.  **Step 5.1: Implement Aetherium Essence Resource**
    *   **Instruction:** Add an `int AetheriumEssence` property to the `Player` class, initialized to 0.
    *   **Validation:** Player object correctly tracks the essence value.

2.  **Step 5.2: Grant Essence on Enemy Death**
    *   **Instruction:** Modify the enemy death logic (Step 3.5). When an enemy is marked for removal due to health reaching 0, increase the `Player.AetheriumEssence` by a fixed amount (e.g., 1).
    *   **Validation:** Killing an enemy increases the player's essence count (verify via logging or debug display).

3.  **Step 5.3: Create Placeholder Weaving Altar**
    *   **Instruction:** Create a simple `WeavingAltar` class with a `Vector2 Position` and `Rectangle Bounds`. Load a distinct placeholder sprite (e.g., a purple 32x32 square). Instantiate one altar within one of the generated rooms (fixed position for now). Draw the altar.
    *   **Validation:** The altar sprite appears in the designated room.

4.  **Step 5.4: Implement Basic Weaving Interaction**
    *   **Instruction:** Using the InputManager, implement a basic interaction system to detect when the Interact action is pressed while the player's bounds overlap with altar's bounds. If interaction occurs and `Player.AetheriumEssence` is above a certain cost (e.g., 3), deduct the cost and apply a *single, hardcoded* temporary buff to the player (e.g., set a `Player.HasDamageBuff` flag to true).
    *   **Validation:** Moving the player to the altar and interacting deducts essence (if sufficient) and sets the buff flag.

5.  **Step 5.5: Implement Basic Buff Effect & Visual Indicator**
    *   **Instruction:** Modify the damage calculation logic (Step 3.4). If `Player.HasDamageBuff` is true, increase the damage dealt to the enemy. For now, the buff is permanent until the game restarts (temporary duration comes later). Implement a visual indicator by tinting the player's sprite to a noticeable color (e.g., light blue or yellow) when the `HasDamageBuff` flag is true. Use the color parameter in SpriteBatch.Draw(). Reset to Color.White when the buff is not active.
    *   **Validation:** After interacting with the altar, attacks deal increased damage to enemies and the player sprite visibly changes color to indicate the active buff.
    *   **Architecture Update:** Update `@architecture.md` to document the Aetherium Weaving system, including essence collection, altar interaction, and buff implementation.

This completes the core MVP implementation. Future phases will build upon this foundation, adding more complex generation, enemy AI, diverse loot, the Hub, meta-progression, proper UI, mobile controls, and full Aetherium Weaving options. Remember to update `@architecture.md` at the completion of each major phase as indicated.

**Important Notes:**
- Focus on correctness over speed - there are no set timeframes per phase. Ensure each step works properly before proceeding.
- Use simple PNG files for placeholder assets, processed through the MonoGame content pipeline.
- The InputManager abstraction is crucial for later mobile support - build it properly from the beginning.
- Keep the architecture document updated as a living record of design decisions and structure.