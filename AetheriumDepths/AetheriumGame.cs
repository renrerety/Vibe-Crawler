using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using AetheriumDepths.Core;
using AetheriumDepths.Entities;
using AetheriumDepths.Generation;

namespace AetheriumDepths
{
    /// <summary>
    /// The main game class for Aetherium Depths.
    /// </summary>
    public class AetheriumGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game state management
        private StateManager _stateManager;
        
        // Input management
        private InputManager _inputManager;
        
        // Player entity
        private Player _player;
        
        // Enemy entities
        private List<Enemy> _enemies;
        
        // Weaving Altar
        private WeavingAltar _weavingAltar;
        
        // Debug texture for attack hitbox
        private Texture2D _debugTexture;
        
        // Dungeon and generator
        private DungeonGenerator _dungeonGenerator;
        private Dungeon _currentDungeon;
        
        // Game constants
        private const float PlayerSpeed = 200f; // Pixels per second
        private const int EnemyHealth = 3; // Default enemy health
        private const int AttackDamage = 1; // Default attack damage
        private const int DamageBuffMultiplier = 2; // Damage multiplier when buff is active
        private const int AetheriumEssenceReward = 1; // Essence gained per enemy defeated
        private const int AetheriumWeavingCost = 3; // Essence cost for activating a buff
        
        // Interaction constants
        private const float InteractionDistance = 50f; // Distance for interacting with objects

        public AetheriumGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // Set window properties to ensure visibility
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            
            Window.Title = "Aetherium Depths";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize state manager with Gameplay as default
            _stateManager = new StateManager(StateManager.GameState.Gameplay);
            _stateManager.StateChanged += OnStateChanged;
            
            // Initialize input manager
            _inputManager = new InputManager();
            
            // Initialize enemy list
            _enemies = new List<Enemy>();
            
            // Initialize dungeon generator
            _dungeonGenerator = new DungeonGenerator();

            // Initialization logic
            Console.WriteLine("Initializing Aetherium Depths...");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Console.WriteLine("Loading content...");
            
            // Load the player sprite
            Texture2D playerSprite = Content.Load<Texture2D>("PlayerSprite");
            
            // Create player (position will be set when entering Gameplay state)
            _player = new Player(Vector2.Zero, playerSprite);
            
            // Load enemy sprite
            Texture2D enemySprite = Content.Load<Texture2D>("EnemySprite");
            
            // Create one enemy at a fixed position (will be repositioned in GenerateDungeon)
            Vector2 enemyPosition = new Vector2(
                GraphicsDevice.Viewport.Width * 0.75f,
                GraphicsDevice.Viewport.Height * 0.5f);
            _enemies.Add(new Enemy(enemyPosition, enemySprite, EnemyHealth));
            
            // Load dedicated altar sprite
            Texture2D altarSprite = Content.Load<Texture2D>("AltarSprite");
            
            // Create weaving altar (position will be set in GenerateDungeon)
            _weavingAltar = new WeavingAltar(Vector2.Zero, altarSprite);
            
            // Create debug texture for attack hitbox and room drawing
            _debugTexture = new Texture2D(GraphicsDevice, 1, 1);
            _debugTexture.SetData(new[] { Color.White });
            
            // Generate dungeon for the first time
            GenerateDungeon();
        }
        
        /// <summary>
        /// Generates a new dungeon and positions the player and enemies.
        /// </summary>
        private void GenerateDungeon()
        {
            // Generate a new dungeon
            _currentDungeon = _dungeonGenerator.GenerateSimpleDungeon(GraphicsDevice.Viewport.Bounds);
            
            // Position player in the center of the starting room
            if (_currentDungeon?.StartingRoom != null)
            {
                Vector2 startPosition = _currentDungeon.StartingRoom.Center;
                
                // Adjust for player sprite center
                startPosition.X -= (_player.Sprite?.Width ?? 0) / 2;
                startPosition.Y -= (_player.Sprite?.Height ?? 0) / 2;
                
                _player.Position = startPosition;
                
                Console.WriteLine($"Positioned player at {startPosition} in starting room");
            }
            
            // Position one enemy in the last room
            if (_currentDungeon?.Rooms.Count > 0 && _enemies.Count > 0)
            {
                Room lastRoom = _currentDungeon.Rooms[_currentDungeon.Rooms.Count - 1];
                Vector2 enemyPosition = lastRoom.Center;
                
                // Adjust for enemy sprite center
                enemyPosition.X -= (_enemies[0].Sprite?.Width ?? 0) / 2;
                enemyPosition.Y -= (_enemies[0].Sprite?.Height ?? 0) / 2;
                
                _enemies[0].Position = enemyPosition;
                _enemies[0].Reactivate(EnemyHealth); // Reactivate the enemy with default health
                
                Console.WriteLine($"Positioned enemy at {enemyPosition} in last room");
            }
            
            // Position weaving altar in a random middle room
            if (_currentDungeon?.Rooms.Count > 1)
            {
                // Get a room from the middle of the array (not first, not last)
                int altarRoomIndex = _currentDungeon.Rooms.Count <= 2 ? 0 : 1;
                Room altarRoom = _currentDungeon.Rooms[altarRoomIndex];
                Vector2 altarPosition = altarRoom.Center;
                
                // Adjust for altar sprite center
                altarPosition.X -= (_weavingAltar.Sprite?.Width ?? 0) / 2;
                altarPosition.Y -= (_weavingAltar.Sprite?.Height ?? 0) / 2;
                
                _weavingAltar.Position = altarPosition;
                
                Console.WriteLine($"Positioned weaving altar at {altarPosition} in room {altarRoomIndex}");
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Update input manager
            _inputManager.Update();
            
            // Check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Check for debug state changes
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                // Toggle between Gameplay and Paused states
                if (_stateManager.CurrentState == StateManager.GameState.Gameplay)
                    _stateManager.ChangeState(StateManager.GameState.Paused);
                else if (_stateManager.CurrentState == StateManager.GameState.Paused)
                    _stateManager.ChangeState(StateManager.GameState.Gameplay);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.M))
            {
                // Switch to MainMenu state
                _stateManager.ChangeState(StateManager.GameState.MainMenu);
            }
            // Add new debug command to regenerate the dungeon
            else if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                GenerateDungeon();
                Console.WriteLine("Regenerated dungeon");
            }

            // Update state manager
            _stateManager.Update(gameTime);

            // Update based on current state
            switch (_stateManager.CurrentState)
            {
                case StateManager.GameState.MainMenu:
                    UpdateMainMenu(gameTime);
                    break;
                case StateManager.GameState.Gameplay:
                    UpdateGameplay(gameTime);
                    break;
                case StateManager.GameState.Paused:
                    UpdatePaused(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            // Draw based on current state
            switch (_stateManager.CurrentState)
            {
                case StateManager.GameState.MainMenu:
                    DrawMainMenu(gameTime);
                    break;
                case StateManager.GameState.Gameplay:
                    DrawGameplay(gameTime);
                    break;
                case StateManager.GameState.Paused:
                    DrawPaused(gameTime);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Event handler for state change events.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments containing the new state.</param>
        private void OnStateChanged(object sender, StateManager.GameState newState)
        {
            Console.WriteLine($"Game state changed to: {newState}");
            
            if (newState == StateManager.GameState.Gameplay)
            {
                // When entering gameplay state, ensure dungeon is generated
                if (_currentDungeon == null)
                {
                    GenerateDungeon();
                }
            }
        }

        #region State-specific Update and Draw methods

        private void UpdateMainMenu(GameTime gameTime)
        {
            Console.WriteLine($"Updating MainMenu state: {gameTime.TotalGameTime}");
        }

        private void UpdateGameplay(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Get the movement vector from the input manager
            Vector2 movementVector = _inputManager.GetMovementVector();
            
            // Move the player
            _player.Move(
                movementVector, 
                PlayerSpeed, 
                deltaTime,
                GraphicsDevice.Viewport.Bounds);
                
            // Update player state
            _player.Update(deltaTime);
            
            // Check for attack input
            if (_inputManager.IsActionJustPressed(InputManager.GameAction.Attack))
            {
                _player.Attack();
                Console.WriteLine("Player attacked!");
            }
            
            // Check for dodge input
            if (_inputManager.IsActionJustPressed(InputManager.GameAction.Dodge))
            {
                _player.Dodge();
                Console.WriteLine("Player dodged!");
            }
            
            // Check for interact input
            if (_inputManager.IsActionJustPressed(InputManager.GameAction.Interact))
            {
                // Check if player is near the weaving altar
                if (CollisionUtility.CheckAABBCollision(_player.Bounds, _weavingAltar.Bounds))
                {
                    // Try to activate damage buff by spending essence
                    _player.ActivateDamageBuff(AetheriumWeavingCost);
                    Console.WriteLine("Player interacted with weaving altar!");
                }
            }
            
            // Check for attack collision with enemies
            if (_player.IsAttacking)
            {
                foreach (Enemy enemy in _enemies)
                {
                    if (enemy.IsActive && CollisionUtility.CheckAABBCollision(_player.AttackHitbox, enemy.Bounds))
                    {
                        // Calculate damage based on buff status
                        int damage = _player.HasDamageBuff ? AttackDamage * DamageBuffMultiplier : AttackDamage;
                        
                        // Deal damage to the enemy
                        enemy.TakeDamage(damage);
                        Console.WriteLine($"Hit enemy! Enemy health: {enemy.Health}");
                        
                        // Check if enemy died
                        if (enemy.Health <= 0)
                        {
                            // Grant Aetherium essence on enemy death
                            _player.AddAetheriumEssence(AetheriumEssenceReward);
                        }
                        
                        // Deactivate the current attack (prevent multiple hits from one swing)
                        _player.DeactivateAttack();
                        break;
                    }
                }
            }
            
            // Clean up inactive enemies (not needed with a single enemy, but good practice)
            _enemies.RemoveAll(enemy => !enemy.IsActive);
            
            Console.WriteLine($"Updating Gameplay state: {gameTime.TotalGameTime}, Player at: {_player.Position}");
        }

        private void UpdatePaused(GameTime gameTime)
        {
            Console.WriteLine($"Updating Paused state: {gameTime.TotalGameTime}");
        }

        private void DrawMainMenu(GameTime gameTime)
        {
            Console.WriteLine($"Drawing MainMenu state: {gameTime.TotalGameTime}");
        }

        private void DrawGameplay(GameTime gameTime)
        {
            // Draw the dungeon rooms
            DrawDungeon();
            
            // Draw the weaving altar
            _weavingAltar.Draw(_spriteBatch);
            
            // Draw enemies
            foreach (Enemy enemy in _enemies)
            {
                enemy.Draw(_spriteBatch);
            }
            
            // Draw the player
            _player.Draw(_spriteBatch);
            
            // Draw attack hitbox if attacking (for debugging)
            _player.DrawAttackHitbox(_spriteBatch, _debugTexture);
            
            // Draw UI elements (essence counter, buff indicators)
            DrawGameplayUI();
            
            Console.WriteLine($"Drawing Gameplay state: {gameTime.TotalGameTime}");
        }
        
        /// <summary>
        /// Draws the current dungeon rooms.
        /// </summary>
        private void DrawDungeon()
        {
            if (_currentDungeon == null) return;
            
            // Draw each room
            foreach (Room room in _currentDungeon.Rooms)
            {
                // Draw room outline with a thickness of 2 pixels
                DrawRectangleOutline(room.Bounds, Color.White, 2);
            }
        }
        
        /// <summary>
        /// Draws gameplay UI elements like the essence counter.
        /// </summary>
        private void DrawGameplayUI()
        {
            // This is a placeholder for future UI implementation
            // In a real game, you would draw text showing the essence count, buff icons, etc.
            // For now, we'll leave this empty as UI is not part of the current task
        }
        
        /// <summary>
        /// Draws a rectangle outline with the specified color and thickness.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="thickness">The thickness of the outline in pixels.</param>
        private void DrawRectangleOutline(Rectangle rectangle, Color color, int thickness)
        {
            // Top
            _spriteBatch.Draw(_debugTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_debugTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
            // Left
            _spriteBatch.Draw(_debugTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Right
            _spriteBatch.Draw(_debugTexture, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }

        private void DrawPaused(GameTime gameTime)
        {
            // Still draw the game elements but with a pause overlay
            DrawDungeon();
            
            // Draw enemies
            foreach (Enemy enemy in _enemies)
            {
                enemy.Draw(_spriteBatch);
            }
            
            // Draw the player
            _player.Draw(_spriteBatch);
            
            Console.WriteLine($"Drawing Paused state: {gameTime.TotalGameTime}");
        }

        #endregion
    }
} 