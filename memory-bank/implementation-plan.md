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


Okay, AI developers, based on the successful completion of the core MVP (Phases 1-5) documented in `@progress.md` and the architecture outlined in `@architecture.md`, we are ready to proceed to the next phase. This phase focuses on significantly enhancing the core gameplay experience by implementing true procedural generation, expanding combat mechanics, fleshing out the Aetherium Weaving system, and adding essential UI feedback.

**Phase 6 Goal:** Transform the placeholder systems into dynamic core gameplay loops. Players should experience varied dungeon layouts, face tangible threats with basic AI, manage their health, and utilize a more developed Aetherium Weaving system with immediate UI feedback.

Remember to **always** consult `@architecture.md` and `@progress.md` before implementing any step. Adhere to the modular structure and update `@architecture.md` after completing significant parts of this phase, especially regarding the new generation algorithm and expanded combat/weaving systems.

## Implementation Plan: Aetherium Depths - Phase 6: Core Gameplay Expansion

**Phase 6.1: Player Health & Basic Enemy Threat**

1.  **Step 6.1.1: Implement Player Health**
    *   **Instruction:** Add `CurrentHealth` and `MaxHealth` properties to the `Player` class. Initialize `CurrentHealth` to `MaxHealth` upon player creation/spawn.
    *   **Validation:** Log the player's `CurrentHealth` to the console. Confirm it initializes correctly.

2.  **Step 6.1.2: Implement Basic Enemy Damage**
    *   **Instruction:** Modify the `Enemy` class or combat logic. For now, implement simple "touch" damage: if the player's bounds collide with an active enemy's bounds (using `CollisionUtility.AABB`), reduce the player's `CurrentHealth` by a fixed amount. Implement a short cooldown after taking damage (invincibility frames, similar to the dodge) to prevent rapid health drain from continuous contact.
    *   **Validation:** Move the player sprite to overlap with an enemy sprite. Observe console logs showing the player's `CurrentHealth` decreasing. Confirm the player doesn't take damage repeatedly during the brief invincibility cooldown after being hit.

3.  **Step 6.1.3: Implement Player Death State**
    *   **Instruction:** In the `Gameplay` state `Update` loop, check if `Player.CurrentHealth` is less than or equal to 0. If so, trigger a state change using the `StateManager` to a new `GameOver` state (create a basic placeholder state that perhaps just displays text or halts gameplay updates).
    *   **Validation:** Allow the player to take damage until health reaches zero. Confirm the game transitions to the `GameOver` state, and gameplay updates (like player movement) cease.

4.  **Step 6.1.4: Implement Basic Enemy AI - Movement**
    *   **Instruction:** Add basic AI behavior to the `Enemy` class's `Update` method. Define a "detection range". If the player is within this range, calculate the direction vector from the enemy to the player and move the enemy along this vector at a defined speed (using `gameTime` for frame independence). If the player is outside the range, the enemy remains stationary (or performs a simple idle behavior later).
    *   **Validation:** Place an enemy and move the player within its detection range. Observe the enemy sprite moving towards the player sprite. Move the player outside the range; observe the enemy stopping.

5.  **Step 6.1.5: Implement Basic Enemy AI - Attack**
    *   **Instruction:** Add a simple attack cooldown timer to the `Enemy`. If the player is within a smaller "attack range" *and* the cooldown is ready, trigger an attack:
        *   Create a temporary melee attack hitbox (similar to player's, Step 3.2) in the direction of the player.
        *   Start the attack cooldown timer.
        *   (Temporarily disable touch damage from Step 6.1.2 if implementing hitbox damage now, or keep both if desired).
    *   **Validation:** Move the player within the enemy's attack range. Observe the enemy periodically creating its attack hitbox (visible via debug rendering). Ensure the attack only occurs when the cooldown is ready.

6.  **Step 6.1.6: Integrate Enemy Attack with Player Damage**
    *   **Instruction:** In the `Gameplay` `Update` loop, check if an active enemy attack hitbox collides with the player's bounds. If it does *and* the player is not invincible (e.g., from their dodge or post-hit cooldown), reduce the player's `CurrentHealth`. Deactivate the *specific* enemy attack hitbox immediately upon successful hit to prevent multiple damage instances from one attack animation/hitbox duration.
    *   **Validation:** Let an enemy attack the player. Confirm the player's `CurrentHealth` decreases only when the enemy's attack hitbox connects and the player is not invincible.

**Phase 6.2: Foundational Procedural Dungeon Generation**

1.  **Step 6.2.1: Implement BSP Tree Generation Core**
    *   **Instruction:** In the `DungeonGenerator` class, implement the core logic for Binary Space Partitioning (BSP). Create a method that takes a rectangular area and recursively splits it either horizontally or vertically into two sub-areas, based on certain criteria (e.g., random split point within bounds, minimum room size). Store this division in a tree structure (Node class with references to left/right children and its `Rectangle` area).
    *   **Validation:** Call the BSP generation method with initial bounds. Debug print or visualize the resulting tree structure and the `Rectangle` areas associated with each node. Confirm the space is recursively partitioned correctly down to minimum size constraints.

2.  **Step 6.2.2: Generate Rooms from BSP Leaves**
    *   **Instruction:** Traverse the generated BSP tree. For each leaf node (a node with no children), create a `Room` object. The room's bounds should be a slightly smaller `Rectangle` placed randomly (or centered with padding) *within* the leaf node's partition area. Add these generated `Room` objects to the `Dungeon`'s list.
    *   **Validation:** After generating the BSP tree, call the room creation logic. Verify that the `Dungeon` object contains a list of `Room` objects whose bounds are correctly situated within the BSP leaf partitions. Debug render the leaf partitions and the rooms inside them to visually confirm.

3.  **Step 6.2.3: Implement Basic Corridor Connection**
    *   **Instruction:** Implement a simple algorithm to connect the generated rooms. For example, connect the centers of sibling rooms in the BSP tree, or connect adjacent rooms by finding the closest points between their bounds. Store corridor data (e.g., a list of `Rectangle` segments or start/end points). Do not worry about complex pathfinding yet.
    *   **Validation:** Debug render the generated rooms and the calculated corridor paths between them. Visually confirm that rooms are logically connected.

4.  **Step 6.2.4: Integrate Generated Dungeon**
    *   **Instruction:** Modify the `Gameplay` state initialization to call the *new* `DungeonGenerator` methods (BSP, room creation, corridors). Replace the placeholder dungeon rendering with logic that renders the newly generated rooms and corridors. Adjust player collision logic to potentially account for corridor areas if necessary (or simply ensure player stays within room bounds for now).
    *   **Validation:** Start a new game multiple times. Observe a different, randomly generated layout of rooms and corridors rendered each time.

5.  **Step 6.2.5: Adapt Entity Spawning**
    *   **Instruction:** Update the spawning logic. Place the player in a designated starting room (e.g., the first room generated or a room tagged as 'start'). Distribute enemies and the Weaving Altar randomly among the *other* generated rooms, ensuring they spawn within room bounds.
    *   **Validation:** Start new games. Confirm the player always starts inside a valid room. Confirm enemies and the altar appear in different, valid room locations across different generated dungeons.

**Phase 6.3: Aetherium Weaving Expansion**

1.  **Step 6.3.1: Implement Buff Duration**
    *   **Instruction:** Add a `Duration` timer property (e.g., a `float` storing remaining seconds) to the concept of an active buff on the `Player`. When a buff is activated (Step 5.4), set its timer. In the `Player`'s `Update` method, decrement the timer for active buffs using `gameTime.ElapsedGameTime.TotalSeconds`. If a timer reaches zero or below, deactivate the corresponding buff flag (e.g., `HasDamageBuff = false`).
    *   **Validation:** Activate the damage buff via the altar. Observe (via logging or debug display) that the buff flag turns off automatically after the specified duration. Confirm the damage multiplier stops applying after expiration.

2.  **Step 6.3.2: Implement Additional Buff Type (Speed)**
    *   **Instruction:** Add a `HasSpeedBuff` flag and a corresponding `SpeedBuffDuration` timer to the `Player`. Modify the player movement calculation (Step 2.3) to increase movement speed by a multiplier if `HasSpeedBuff` is true.
    *   **Validation:** Activate the (currently hardcoded or debug-selectable) speed buff. Observe the player sprite moving noticeably faster. Confirm the speed returns to normal after the duration expires.

3.  **Step 6.3.3: Implement Basic Buff Selection at Altar**
    *   **Instruction:** Modify the Weaving Altar interaction logic (Step 5.4). When interaction occurs, instead of automatically activating the damage buff, check for specific debug key presses (e.g., '1' for Damage Buff, '2' for Speed Buff). If the corresponding key is pressed and the player has enough essence, deduct the cost and activate *only* the selected buff with its duration.
    *   **Validation:** Interact with the altar. Press '1' - confirm essence is deducted and damage buff activates/timer starts. Interact again. Press '2' - confirm essence is deducted and speed buff activates/timer starts. Ensure activating one doesn't activate the other unless intended and paid for separately.

4.  **Step 6.3.4: Ensure Independent Buff Timers**
    *   **Instruction:** Verify that the duration timers for different buffs are tracked and decremented independently. Update the buff management logic to handle multiple potentially active buffs concurrently.
    *   **Validation:** Activate the damage buff. While it's active, activate the speed buff. Confirm both buffs are active simultaneously (player moves faster *and* deals more damage). Observe each buff expiring individually according to its own timer.

**Phase 6.4: Core UI Implementation (HUD)**

1.  **Step 6.4.1: Create Basic UI Rendering Layer**
    *   **Instruction:** Establish a simple UI rendering system. This might involve creating a `UIManager` class or simply adding a dedicated drawing section in the `Gameplay` state's `Draw` method that uses `SpriteBatch` with screen-space coordinates (unaffected by camera). Load a basic font (`SpriteFont`) via the Content Pipeline.
    *   **Validation:** Draw simple static text (e.g., "Health:") at a fixed screen position using the loaded font. Confirm it appears correctly overlaid on the gameplay scene.

2.  **Step 6.4.2: Render Player Health**
    *   **Instruction:** In the UI rendering layer, draw the player's `CurrentHealth` and `MaxHealth` values as text (e.g., "Health: 75/100"). Alternatively, draw a simple health bar (e.g., two overlapping rectangles drawn with `SpriteBatch` - background red, foreground green scaled by `CurrentHealth / MaxHealth`).
    *   **Validation:** The UI accurately reflects the player's current and max health. Observe the UI updating in real-time as the player takes damage.

3.  **Step 6.4.3: Render Aetherium Essence**
    *   **Instruction:** In the UI rendering layer, draw the player's current `AetheriumEssence` count as text (e.g., "Essence: 15").
    *   **Validation:** The UI accurately displays the player's essence count. Observe the count increasing in real-time when the player collects essence (defeats enemies).

4.  **Step 6.4.4: Render Active Buff Indicators**
    *   **Instruction:** In the UI rendering layer, check which buffs are currently active on the player. For each active buff, display a simple indicator (e.g., small icon loaded via Content Pipeline, or text like "DMG UP", "SPD UP") along with its remaining duration (formatted to one decimal place, e.g., "4.2s"). Position these indicators clearly on the HUD.
    *   **Validation:** Activate buffs via the altar. Observe the corresponding indicators appearing on the HUD with the correct remaining time. Confirm indicators disappear when buffs expire and timers update smoothly.

This concludes the plan for Phase 6. It focuses on making the core loop engaging and laying the groundwork for further expansion based on actual procedural content and more dynamic combat and systems. Remember to rigorously validate each step and keep the codebase modular. Good luck!

**Important Notes:**
- Focus on correctness over speed - there are no set timeframes per phase. Ensure each step works properly before proceeding.
- Use simple PNG files for placeholder assets, processed through the MonoGame content pipeline.
- The InputManager abstraction is crucial for later mobile support - build it properly from the beginning.
- Keep the architecture document updated as a living record of design decisions and structure.