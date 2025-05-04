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
        
        // Font for UI text
        private SpriteFont _gameFont;
        
        // Dungeon and generator
        private DungeonGenerator _dungeonGenerator;
        private Dungeon _currentDungeon;
        
        // Game constants
        private const float PlayerSpeed = 200f; // Pixels per second
        private const int EnemyHealth = 3; // Default enemy health
        private const int AttackDamage = 1; // Default attack damage
        private const int EnemyTouchDamage = 10; // Damage dealt when enemy touches player
        private const int DamageBuffMultiplier = 2; // Damage multiplier when buff is active
        private const int AetheriumEssenceReward = 1; // Essence gained per enemy defeated
        private const int AetheriumWeavingCost = 3; // Essence cost for activating a buff
        
        // Interaction constants
        private const float InteractionDistance = 50f; // Distance for interacting with objects
        
        // Player invincibility cooldown after taking damage
        private const float DamageInvincibilityDuration = 1.0f; // Time in seconds
        private float _damageInvincibilityTimer = 0f; // Current remaining invincibility time

        // Camera
        private Camera2D _camera;
        
        // Camera following parameters
        private const float CameraLerpFactor = 0.1f; // Smooth following factor

        public AetheriumGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // Set window properties to ensure visibility
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
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
            
            // Initialize camera with the viewport
            _camera = new Camera2D(GraphicsDevice.Viewport);

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
            
            // Load font for UI
            try
            {
                _gameFont = Content.Load<SpriteFont>("GameFont");
                Console.WriteLine("Font loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading font: {ex.Message}");
                // Create a placeholder if the font fails to load
                _gameFont = null;
            }
            
            // Generate dungeon for the first time
            GenerateDungeon();
        }
        
        /// <summary>
        /// Generates a new dungeon and positions the player and enemies.
        /// </summary>
        private void GenerateDungeon()
        {
            // Generate a new dungeon using BSP
            _currentDungeon = _dungeonGenerator.GenerateBSPDungeon(GraphicsDevice.Viewport.Bounds);
            
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
                int altarRoomIndex = _currentDungeon.Rooms.Count <= 2 ? 0 : _currentDungeon.Rooms.Count / 2;
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
                case StateManager.GameState.GameOver:
                    UpdateGameOver(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            // Draw logic based on current state
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
                case StateManager.GameState.GameOver:
                    DrawGameOver(gameTime);
                    break;
            }
            
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
            
            // Get input actions from the input manager
            bool isAttackPressed = _inputManager.IsActionJustPressed(InputManager.GameAction.Attack);
            bool isDodgePressed = _inputManager.IsActionJustPressed(InputManager.GameAction.Dodge);
            bool isInteractPressed = _inputManager.IsActionJustPressed(InputManager.GameAction.Interact);
            bool isRegeneratePressed = Keyboard.GetState().IsKeyDown(Keys.R);
            
            // Check for game regeneration
            if (isRegeneratePressed)
            {
                GenerateDungeon();
                return;
            }
            
            // Process input for player movement
            Vector2 movementVector = _inputManager.GetMovementVector();
            
            // Update player position
            _player.Move(movementVector, PlayerSpeed, deltaTime, _currentDungeon);
            
            // Update camera to follow player with smooth interpolation
            _camera.MoveToTarget(_player.Position, CameraLerpFactor);
            
            // Update player state (attack cooldown, dodge duration, etc.)
            _player.Update(deltaTime);
            
            // Process player attack
            if (isAttackPressed)
            {
                _player.Attack();
            }
            
            // Process player dodge
            if (isDodgePressed)
            {
                _player.Dodge();
            }
            
            // Update damage invincibility timer
            if (_damageInvincibilityTimer > 0)
            {
                _damageInvincibilityTimer -= deltaTime;
                if (_damageInvincibilityTimer < 0)
                {
                    _damageInvincibilityTimer = 0;
                }
            }
            
            // Check for weaving altar interaction
            if (isInteractPressed)
            {
                Rectangle playerBounds = _player.Bounds;
                Rectangle altarBounds = _weavingAltar.Bounds;
                
                if (CollisionUtility.CheckAABBCollision(playerBounds, altarBounds))
                {
                    Console.WriteLine("Interacting with Weaving Altar");
                    
                    // Check for specific buff selection key presses
                    bool damageBuffSelected = Keyboard.GetState().IsKeyDown(Keys.D1);
                    bool speedBuffSelected = Keyboard.GetState().IsKeyDown(Keys.D2);
                    
                    if (damageBuffSelected)
                    {
                        bool success = _player.ActivateDamageBuff(AetheriumWeavingCost);
                        if (success)
                        {
                            Console.WriteLine($"Activated Damage Buff for {AetheriumWeavingCost} essence");
                        }
                        else
                        {
                            Console.WriteLine($"Not enough essence to activate Damage Buff. Need {AetheriumWeavingCost}.");
                        }
                    }
                    else if (speedBuffSelected)
                    {
                        bool success = _player.ActivateSpeedBuff(AetheriumWeavingCost);
                        if (success)
                        {
                            Console.WriteLine($"Activated Speed Buff for {AetheriumWeavingCost} essence");
                        }
                        else
                        {
                            Console.WriteLine($"Not enough essence to activate Speed Buff. Need {AetheriumWeavingCost}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Press 1 for Damage Buff or 2 for Speed Buff");
                    }
                }
            }
            
            // Update enemies
            foreach (Enemy enemy in _enemies)
            {
                if (enemy.IsActive)
                {
                    // Update enemy AI and position
                    enemy.Update(_player.Position, deltaTime, _currentDungeon);
                    
                    // Check for enemy touch damage (if not in invincibility frames)
                    if (!_player.IsInvincible && _damageInvincibilityTimer <= 0 && 
                        CollisionUtility.CheckAABBCollision(_player.Bounds, enemy.Bounds))
                    {
                        bool isPlayerAlive = _player.TakeDamage(EnemyTouchDamage);
                        if (!isPlayerAlive)
                        {
                            // Player died, transition to game over state
                            _stateManager.ChangeState(StateManager.GameState.GameOver);
                            return;
                        }
                        
                        // Start invincibility frames
                        _damageInvincibilityTimer = DamageInvincibilityDuration;
                    }
                    
                    // Check for enemy attack damage
                    if (enemy.IsAttacking && !_player.IsInvincible && _damageInvincibilityTimer <= 0 && 
                        CollisionUtility.CheckAABBCollision(_player.Bounds, enemy.AttackHitbox))
                    {
                        bool isPlayerAlive = _player.TakeDamage(EnemyTouchDamage);
                        if (!isPlayerAlive)
                        {
                            // Player died, transition to game over state
                            _stateManager.ChangeState(StateManager.GameState.GameOver);
                            return;
                        }
                        
                        // Start invincibility frames
                        _damageInvincibilityTimer = DamageInvincibilityDuration;
                    }
                }
            }
            
            // Check for player attack collisions
            if (_player.IsAttacking)
            {
                foreach (Enemy enemy in _enemies)
                {
                    if (enemy.IsActive)
                    {
                        if (CollisionUtility.CheckAABBCollision(enemy.Bounds, _player.AttackHitbox))
                        {
                            // Calculate damage, accounting for damage buff
                            int damage = AttackDamage;
                            if (_player.HasDamageBuff)
                            {
                                damage *= DamageBuffMultiplier;
                            }
                            
                            // Apply damage to enemy
                            enemy.TakeDamage(damage);
                            
                            // Deactivate the current attack to prevent multiple hits
                            _player.DeactivateAttack();
                            
                            // If enemy was killed, grant essence
                            if (!enemy.IsActive)
                            {
                                _player.AddAetheriumEssence(AetheriumEssenceReward);
                            }
                            
                            break; // Only damage one enemy per attack
                        }
                    }
                }
            }
        }

        private void UpdatePaused(GameTime gameTime)
        {
            Console.WriteLine($"Updating Paused state: {gameTime.TotalGameTime}");
        }

        private void UpdateGameOver(GameTime gameTime)
        {
            // Check for restart input
            if (_inputManager.IsActionJustPressed(InputManager.GameAction.Interact))
            {
                // Reset the game and return to Gameplay state
                ResetGame();
                _stateManager.ChangeState(StateManager.GameState.Gameplay);
            }
            
            Console.WriteLine($"Updating GameOver state: {gameTime.TotalGameTime}");
        }

        private void DrawMainMenu(GameTime gameTime)
        {
            Console.WriteLine($"Drawing MainMenu state: {gameTime.TotalGameTime}");
        }

        private void DrawGameplay(GameTime gameTime)
        {
            // Start drawing the game world with camera transform
            _spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointClamp, 
                null, 
                null, 
                null, 
                _camera.GetTransformMatrix()); // Apply camera transform
                
            // Draw dungeon
            DrawDungeon();
            
            // Draw the weaving altar
            _weavingAltar.Draw(_spriteBatch);
            
            // Draw enemies
            foreach (Enemy enemy in _enemies)
            {
                enemy.Draw(_spriteBatch);
                
                // Draw attack hitbox if debugging
                if (enemy.IsAttacking)
                {
                    enemy.DrawAttackHitbox(_spriteBatch, _debugTexture);
                }
            }
            
            // Draw player
            _player.Draw(_spriteBatch, _damageInvincibilityTimer);
            
            // Draw attack hitbox if debugging
            if (_player.IsAttacking)
            {
                _player.DrawAttackHitbox(_spriteBatch, _debugTexture);
            }
            
            _spriteBatch.End();
            
            // Draw UI elements without camera transform (screen space)
            _spriteBatch.Begin();
            DrawGameplayUI();
            _spriteBatch.End();
        }
        
        /// <summary>
        /// Draws the current dungeon rooms.
        /// </summary>
        private void DrawDungeon()
        {
            if (_currentDungeon == null) return;
            
            // Draw corridors
            foreach (Rectangle corridor in _currentDungeon.Corridors)
            {
                _spriteBatch.Draw(_debugTexture, corridor, new Color((byte)100, (byte)100, (byte)100, (byte)150));
            }
            
            // Draw each room
            foreach (Room room in _currentDungeon.Rooms)
            {
                // Draw room outline with thinner, more transparent lines
                DrawRectangleOutline(room.Bounds, new Color((byte)255, (byte)255, (byte)255, (byte)120), 1);
                
                // Fill the room with a slight color
                Rectangle innerRect = new Rectangle(
                    room.Bounds.X + 1, 
                    room.Bounds.Y + 1, 
                    room.Bounds.Width - 2, 
                    room.Bounds.Height - 2);
                _spriteBatch.Draw(_debugTexture, innerRect, new Color((byte)50, (byte)50, (byte)70, (byte)50));
            }
            
            // Debug visualization: Draw BSP tree partitions if available
            if (_currentDungeon.LeafNodes != null)
            {
                foreach (BSPNode leaf in _currentDungeon.LeafNodes)
                {
                    // Draw leaf node boundaries with a different color
                    DrawRectangleOutline(leaf.Area, new Color((byte)50, (byte)200, (byte)50, (byte)80), 1);
                }
            }
        }
        
        /// <summary>
        /// Draws gameplay UI elements like the essence counter.
        /// </summary>
        private void DrawGameplayUI()
        {
            // Draw a translucent panel for UI elements
            Rectangle uiBackground = new Rectangle(10, 10, 240, 150);
            _spriteBatch.Draw(_debugTexture, uiBackground, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            
            // === HEALTH DISPLAY ===
            // Draw health bar label
            if (_gameFont != null)
            {
                _spriteBatch.DrawString(_gameFont, "HEALTH", new Vector2(20, 15), Color.White);
            }
            
            // Draw health bar
            int healthBarWidth = 200;
            Rectangle healthBarBackground = new Rectangle(20, 35, healthBarWidth, 20);
            Rectangle healthBarFill = new Rectangle(
                20, 
                35, 
                (int)(healthBarWidth * ((float)_player.CurrentHealth / _player.MaxHealth)), 
                20);
            
            // Background (dark red)
            _spriteBatch.Draw(_debugTexture, healthBarBackground, new Color((byte)100, (byte)0, (byte)0, (byte)200));
            // Fill (bright green)
            _spriteBatch.Draw(_debugTexture, healthBarFill, new Color((byte)0, (byte)220, (byte)0, (byte)220));
            
            // Draw health text
            if (_gameFont != null)
            {
                string healthText = $"{_player.CurrentHealth}/{_player.MaxHealth}";
                Vector2 textSize = _gameFont.MeasureString(healthText);
                Vector2 textPosition = new Vector2(
                    healthBarBackground.X + (healthBarBackground.Width - textSize.X) / 2,
                    healthBarBackground.Y + (healthBarBackground.Height - textSize.Y) / 2);
                _spriteBatch.DrawString(_gameFont, healthText, textPosition, Color.White);
            }
            
            // === ESSENCE DISPLAY ===
            int yPos = 65;
            
            // Draw essence icon
            Rectangle essenceIcon = new Rectangle(20, yPos + 2, 16, 16);
            _spriteBatch.Draw(_debugTexture, essenceIcon, new Color((byte)180, (byte)100, (byte)255, (byte)255)); // Purple for essence
            
            // Draw essence text
            if (_gameFont != null)
            {
                string essenceText = $"Essence: {_player.AetheriumEssence}";
                _spriteBatch.DrawString(_gameFont, essenceText, new Vector2(45, yPos), Color.White);
            }
            
            // === ACTIVE BUFFS ===
            yPos = 95;
            
            // Draw active buffs header
            if (_gameFont != null && (_player.HasDamageBuff || _player.HasSpeedBuff))
            {
                _spriteBatch.DrawString(_gameFont, "ACTIVE BUFFS:", new Vector2(20, yPos), Color.White);
                yPos += 25;
            }
            
            // Check for damage buff
            if (_player.HasDamageBuff)
            {
                // Draw buff icon
                Rectangle buffIcon = new Rectangle(20, yPos + 2, 16, 16);
                _spriteBatch.Draw(_debugTexture, buffIcon, Color.Yellow); // Yellow for damage buff
                
                // Draw buff text
                if (_gameFont != null)
                {
                    float duration = _player.GetBuffDuration(BuffType.Damage);
                    string buffText = $"Damage Buff: {duration:F1}s";
                    _spriteBatch.DrawString(_gameFont, buffText, new Vector2(45, yPos), Color.Yellow);
                }
                
                yPos += 25; // Space for next buff
            }
            
            // Check for speed buff
            if (_player.HasSpeedBuff)
            {
                // Draw buff icon
                Rectangle buffIcon = new Rectangle(20, yPos + 2, 16, 16);
                _spriteBatch.Draw(_debugTexture, buffIcon, Color.LightGreen); // Green for speed buff
                
                // Draw buff text
                if (_gameFont != null)
                {
                    float duration = _player.GetBuffDuration(BuffType.Speed);
                    string buffText = $"Speed Buff: {duration:F1}s";
                    _spriteBatch.DrawString(_gameFont, buffText, new Vector2(45, yPos), Color.LightGreen);
                }
            }
            
            // === MINIMAP ===
            // Only draw if we have a dungeon
            if (_currentDungeon != null)
            {
                // Minimap configuration
                int minimapWidth = 200;
                int minimapHeight = 150;
                int minimapX = GraphicsDevice.Viewport.Width - minimapWidth - 10;
                int minimapY = 10;
                float minimapScale = 0.2f; // Scale factor for the minimap
                
                // Calculate the scaling factors between the game world and minimap
                Rectangle gameWorldBounds = CalculateGameWorldBounds();
                float xScale = (float)minimapWidth / gameWorldBounds.Width;
                float yScale = (float)minimapHeight / gameWorldBounds.Height;
                float scale = Math.Min(xScale, yScale) * 0.9f; // Use the smaller scale and leave some margin
                
                // Draw minimap background
                Rectangle minimapBackground = new Rectangle(minimapX, minimapY, minimapWidth, minimapHeight);
                _spriteBatch.Draw(_debugTexture, minimapBackground, new Color((byte)10, (byte)10, (byte)40, (byte)200));
                
                // Draw minimap label
                if (_gameFont != null)
                {
                    _spriteBatch.DrawString(_gameFont, "MINIMAP", new Vector2(minimapX + 10, minimapY + 5), Color.White);
                }
                
                // Calculate the offset to center the map in the minimap area
                Vector2 minimapCenter = new Vector2(minimapX + minimapWidth / 2, minimapY + minimapHeight / 2);
                Vector2 worldCenter = new Vector2(
                    gameWorldBounds.X + gameWorldBounds.Width / 2,
                    gameWorldBounds.Y + gameWorldBounds.Height / 2);
                
                // Draw corridors
                foreach (Rectangle corridor in _currentDungeon.Corridors)
                {
                    // Transform corridor from world space to minimap space
                    Rectangle minimapCorridor = new Rectangle(
                        (int)(minimapCenter.X + (corridor.X - worldCenter.X) * scale),
                        (int)(minimapCenter.Y + (corridor.Y - worldCenter.Y) * scale),
                        (int)(corridor.Width * scale),
                        (int)(corridor.Height * scale));
                    
                    _spriteBatch.Draw(_debugTexture, minimapCorridor, new Color((byte)100, (byte)100, (byte)100, (byte)150));
                }
                
                // Draw rooms
                foreach (Room room in _currentDungeon.Rooms)
                {
                    // Transform room from world space to minimap space
                    Rectangle minimapRoom = new Rectangle(
                        (int)(minimapCenter.X + (room.Bounds.X - worldCenter.X) * scale),
                        (int)(minimapCenter.Y + (room.Bounds.Y - worldCenter.Y) * scale),
                        (int)(room.Bounds.Width * scale),
                        (int)(room.Bounds.Height * scale));
                    
                    // Draw room outline
                    Color roomColor = Color.LightGray;
                    
                    // Highlight special rooms
                    if (room == _currentDungeon.StartingRoom)
                    {
                        roomColor = Color.LightBlue; // Starting room
                    }
                    
                    _spriteBatch.Draw(_debugTexture, minimapRoom, new Color(roomColor.R, roomColor.G, roomColor.B, (byte)100));
                }
                
                // Draw player position on minimap
                int playerDotSize = 5;
                Rectangle playerDot = new Rectangle(
                    (int)(minimapCenter.X + (_player.Position.X - worldCenter.X) * scale) - playerDotSize / 2,
                    (int)(minimapCenter.Y + (_player.Position.Y - worldCenter.Y) * scale) - playerDotSize / 2,
                    playerDotSize,
                    playerDotSize);
                _spriteBatch.Draw(_debugTexture, playerDot, Color.White);
                
                // Draw enemy positions on minimap
                foreach (Enemy enemy in _enemies)
                {
                    if (enemy.IsActive)
                    {
                        int enemyDotSize = 4;
                        Rectangle enemyDot = new Rectangle(
                            (int)(minimapCenter.X + (enemy.Position.X - worldCenter.X) * scale) - enemyDotSize / 2,
                            (int)(minimapCenter.Y + (enemy.Position.Y - worldCenter.Y) * scale) - enemyDotSize / 2,
                            enemyDotSize,
                            enemyDotSize);
                        _spriteBatch.Draw(_debugTexture, enemyDot, Color.Red);
                    }
                }
                
                // Draw altar position on minimap
                int altarDotSize = 4;
                Rectangle altarDot = new Rectangle(
                    (int)(minimapCenter.X + (_weavingAltar.Position.X - worldCenter.X) * scale) - altarDotSize / 2,
                    (int)(minimapCenter.Y + (_weavingAltar.Position.Y - worldCenter.Y) * scale) - altarDotSize / 2,
                    altarDotSize,
                    altarDotSize);
                _spriteBatch.Draw(_debugTexture, altarDot, new Color((byte)180, (byte)100, (byte)255)); // Purple for altar
            }
            
            // If in altar range, draw interaction prompt
            if (CollisionUtility.CheckAABBCollision(_player.Bounds, _weavingAltar.Bounds))
            {
                if (_gameFont != null)
                {
                    string altarText = "Press E + (1-2) to weave";
                    Vector2 textSize = _gameFont.MeasureString(altarText);
                    Vector2 textPosition = new Vector2(
                        (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                        GraphicsDevice.Viewport.Height - 50);
                    
                    // Draw background for better visibility
                    Rectangle textBackground = new Rectangle(
                        (int)textPosition.X - 10,
                        (int)textPosition.Y - 5,
                        (int)textSize.X + 20,
                        (int)textSize.Y + 10);
                    _spriteBatch.Draw(_debugTexture, textBackground, new Color((byte)0, (byte)0, (byte)0, (byte)180));
                    
                    // Draw prompt text
                    _spriteBatch.DrawString(_gameFont, altarText, textPosition, Color.White);
                }
            }
        }
        
        /// <summary>
        /// Calculates the bounding rectangle of the entire game world.
        /// </summary>
        /// <returns>A rectangle encompassing all rooms and corridors.</returns>
        private Rectangle CalculateGameWorldBounds()
        {
            if (_currentDungeon == null || _currentDungeon.Rooms.Count == 0)
            {
                return GraphicsDevice.Viewport.Bounds;
            }
            
            // Start with the first room's bounds
            int minX = _currentDungeon.Rooms[0].Bounds.X;
            int minY = _currentDungeon.Rooms[0].Bounds.Y;
            int maxX = minX + _currentDungeon.Rooms[0].Bounds.Width;
            int maxY = minY + _currentDungeon.Rooms[0].Bounds.Height;
            
            // Expand bounds to include all rooms
            foreach (Room room in _currentDungeon.Rooms)
            {
                minX = Math.Min(minX, room.Bounds.X);
                minY = Math.Min(minY, room.Bounds.Y);
                maxX = Math.Max(maxX, room.Bounds.X + room.Bounds.Width);
                maxY = Math.Max(maxY, room.Bounds.Y + room.Bounds.Height);
            }
            
            // Expand bounds to include all corridors
            foreach (Rectangle corridor in _currentDungeon.Corridors)
            {
                minX = Math.Min(minX, corridor.X);
                minY = Math.Min(minY, corridor.Y);
                maxX = Math.Max(maxX, corridor.X + corridor.Width);
                maxY = Math.Max(maxY, corridor.Y + corridor.Height);
            }
            
            // Add some padding
            const int padding = 20;
            minX -= padding;
            minY -= padding;
            maxX += padding;
            maxY += padding;
            
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
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

        private void DrawGameOver(GameTime gameTime)
        {
            // Draw the "Game Over" text in the center of the screen
            string gameOverText = "GAME OVER - Press 'E' to restart";
            Vector2 textSize = new Vector2(300, 50); // Approximate size, would use font.MeasureString in a real game
            
            Vector2 position = new Vector2(
                (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                (GraphicsDevice.Viewport.Height - textSize.Y) / 2);
            
            // Draw a background rectangle for the text
            Rectangle textBg = new Rectangle(
                (int)position.X - 10,
                (int)position.Y - 10,
                (int)textSize.X + 20,
                (int)textSize.Y + 20);
            
            _spriteBatch.Draw(_debugTexture, textBg, new Color((byte)0, (byte)0, (byte)0, (byte)180));
            
            // Would draw the text with a font here
            // _spriteBatch.DrawString(font, gameOverText, position, Color.White);
            
            Console.WriteLine($"Drawing GameOver state: {gameTime.TotalGameTime}");
        }
        
        /// <summary>
        /// Resets the game state for a new game.
        /// </summary>
        private void ResetGame()
        {
            // Reset player health
            if (_player != null)
            {
                // Because we don't have a direct way to reset health, recreate the player
                Vector2 position = _player.Position;
                Texture2D sprite = _player.Sprite;
                _player = new Player(position, sprite);
            }
            
            // Reset damage invincibility timer
            _damageInvincibilityTimer = 0f;
            
            // Generate a new dungeon with new entity positions
            GenerateDungeon();
            
            Console.WriteLine("Game reset for a new run");
        }

        #endregion
    }
} 