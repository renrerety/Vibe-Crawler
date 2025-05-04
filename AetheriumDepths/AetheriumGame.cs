using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using AetheriumDepths.Core;
using AetheriumDepths.Entities;
using AetheriumDepths.Generation;
using AetheriumDepths.Gameplay;
using AetheriumDepths.Gameplay.Hazards;
using AetheriumDepths.Gameplay.Interactables;

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

        // Combat Manager
        private CombatManager _combatManager;
        
        // Loot items
        private List<LootItem> _lootItems = new List<LootItem>();
        private Texture2D _healthPotionTexture;
        
        // Projectile texture
        private Texture2D _projectileTexture;
        
        // Enemy texture variants
        private Texture2D _fastEnemyTexture;
        private Texture2D _rangedEnemyTexture;
        
        // Enemy spawn constants
        private const int FastEnemyHealth = 2; // Fast enemies have less health
        private const int RangedEnemyHealth = 2; // Ranged enemies have same health as fast ones
        private const float EnemySpawnChance = 0.5f; // 50% chance for enemies to drop items

        // Environmental elements
        private List<SpikeTrap> _spikeTraps;
        private List<DestructibleCrate> _crates;
        private List<TreasureChest> _treasureChests;
        private List<Door> _doors;
        
        // Projectiles
        private List<Projectile> _projectiles = new List<Projectile>();
        
        // UI state
        private bool _isNearAltar = false;
        private bool _isNearChest = false;
        private TreasureChest _nearbyChest = null;
        
        // Popup notification
        private string _popupMessage = "";
        private float _popupTimer = 0f;
        private const float POPUP_DURATION = 3.0f; // Show popup for 3 seconds

        public AetheriumGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // Set window size to full HD
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            
            // Create the state manager with default state
            _stateManager = new StateManager();
            _stateManager.ChangeState(StateManager.GameState.Gameplay); // Fixed: use ChangeState method
            
            // Create input manager
            _inputManager = new InputManager();
            
            // Initialize entity lists
            _enemies = new List<Enemy>();
            _projectiles = new List<Projectile>();
            _lootItems = new List<LootItem>();
            
            // Initialize environmental elements lists
            _spikeTraps = new List<SpikeTrap>();
            _crates = new List<DestructibleCrate>();
            _treasureChests = new List<TreasureChest>();
            _doors = new List<Door>();
            
            // Create the dungeon generator
            _dungeonGenerator = new DungeonGenerator();
            
            // Create combat manager
            _combatManager = new CombatManager(
                null, // Will set this after player is initialized
                _enemies,
                EnemyTouchDamage,
                AttackDamage,
                DamageBuffMultiplier,
                AetheriumEssenceReward);
                
            // Subscribe to enemy killed event
            _combatManager.EnemyKilled += OnEnemyKilled;
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
            
            // Load projectile texture
            try
            {
                _projectileTexture = Content.Load<Texture2D>("ProjectileSprite");
                Console.WriteLine("Projectile texture loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading projectile texture: {ex.Message}");
                // Create a simple placeholder
                _projectileTexture = new Texture2D(GraphicsDevice, 8, 8);
                Color[] colorData = new Color[64];
                for (int i = 0; i < 64; i++)
                {
                    colorData[i] = Color.White;
                }
                _projectileTexture.SetData(colorData);
            }
            
            // Create player with projectile texture
            _player = new Player(Vector2.Zero, playerSprite, _projectileTexture);
            
            // Load enemy sprites
            Texture2D enemySprite = Content.Load<Texture2D>("EnemySprite");
            
            // Load or create fast enemy sprite
            try
            {
                _fastEnemyTexture = Content.Load<Texture2D>("FastEnemySprite");
                Console.WriteLine("Fast enemy texture loaded successfully");
            }
            catch (Exception)
            {
                // Use base enemy sprite with a color tint in the Draw method instead
                _fastEnemyTexture = enemySprite;
            }
            
            // Load or create ranged enemy sprite
            try
            {
                _rangedEnemyTexture = Content.Load<Texture2D>("RangedEnemySprite");
                Console.WriteLine("Ranged enemy texture loaded successfully");
            }
            catch (Exception)
            {
                // Use base enemy sprite with a color tint in the Draw method instead
                _rangedEnemyTexture = enemySprite;
            }
            
            // Load health potion texture
            try
            {
                _healthPotionTexture = Content.Load<Texture2D>("HealthPotionSprite");
                Console.WriteLine("Health potion texture loaded successfully");
            }
            catch (Exception)
            {
                // Create a simple red placeholder
                _healthPotionTexture = new Texture2D(GraphicsDevice, 16, 16);
                Color[] colorData = new Color[256];
                for (int i = 0; i < 256; i++)
                {
                    colorData[i] = Color.Red;
                }
                _healthPotionTexture.SetData(colorData);
            }
            
            // Load environmental element textures
            
            // Load spike trap textures
            Texture2D spikeTrapsArmedTexture, spikeTrapsDisarmedTexture;
            try
            {
                spikeTrapsArmedTexture = Content.Load<Texture2D>("SpikeTrapArmedSprite");
                spikeTrapsDisarmedTexture = Content.Load<Texture2D>("SpikeTrapDisarmedSprite");
                Console.WriteLine("Spike trap textures loaded successfully");
            }
            catch (Exception)
            {
                // Create simple placeholders
                spikeTrapsArmedTexture = new Texture2D(GraphicsDevice, 32, 32);
                spikeTrapsDisarmedTexture = new Texture2D(GraphicsDevice, 32, 32);
                
                // Armed texture (red with spikes)
                Color[] armedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    // Create a pattern to represent spikes
                    int x = i % 32;
                    int y = i / 32;
                    
                    // Make the edges and center spikes appear red
                    if (x < 3 || x > 28 || y < 3 || y > 28 || 
                        (x >= 13 && x <= 18 && y >= 13 && y <= 18))
                    {
                        armedColorData[i] = Color.Red;
                    }
                    else
                    {
                        armedColorData[i] = Color.DarkRed;
                    }
                }
                spikeTrapsArmedTexture.SetData(armedColorData);
                
                // Disarmed texture (dark gray)
                Color[] disarmedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    disarmedColorData[i] = Color.DarkGray;
                }
                spikeTrapsDisarmedTexture.SetData(disarmedColorData);
            }
            
            // Load crate texture
            Texture2D crateTexture;
            try
            {
                crateTexture = Content.Load<Texture2D>("CrateSprite");
                Console.WriteLine("Crate texture loaded successfully");
            }
            catch (Exception)
            {
                // Create a simple brown placeholder
                crateTexture = new Texture2D(GraphicsDevice, 32, 32);
                Color[] colorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    colorData[i] = Color.SaddleBrown;
                }
                crateTexture.SetData(colorData);
            }
            
            // Load treasure chest textures
            Texture2D chestClosedTexture, chestOpenTexture;
            try
            {
                chestClosedTexture = Content.Load<Texture2D>("ChestClosedSprite");
                chestOpenTexture = Content.Load<Texture2D>("ChestOpenSprite");
                Console.WriteLine("Chest textures loaded successfully");
            }
            catch (Exception)
            {
                // Create simple placeholders
                chestClosedTexture = new Texture2D(GraphicsDevice, 32, 32);
                chestOpenTexture = new Texture2D(GraphicsDevice, 32, 32);
                
                // Closed texture (gold/brown)
                Color[] closedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    closedColorData[i] = Color.Goldenrod;
                }
                chestClosedTexture.SetData(closedColorData);
                
                // Open texture (gold with dark center)
                Color[] openColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    int x = i % 32;
                    int y = i / 32;
                    
                    if (x >= 8 && x <= 24 && y >= 8 && y <= 24)
                    {
                        openColorData[i] = Color.Black;
                    }
                    else
                    {
                        openColorData[i] = Color.Goldenrod;
                    }
                }
                chestOpenTexture.SetData(openColorData);
            }
            
            // Load door textures
            Texture2D doorLockedTexture, doorUnlockedTexture;
            try
            {
                doorLockedTexture = Content.Load<Texture2D>("DoorLockedSprite");
                doorUnlockedTexture = Content.Load<Texture2D>("DoorUnlockedSprite");
                Console.WriteLine("Door textures loaded successfully");
            }
            catch (Exception)
            {
                // Create simple placeholders
                doorLockedTexture = new Texture2D(GraphicsDevice, 32, 32);
                doorUnlockedTexture = new Texture2D(GraphicsDevice, 32, 32);
                
                // Locked texture (dark brown with gold keyhole)
                Color[] lockedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    int x = i % 32;
                    int y = i / 32;
                    
                    if (x >= 13 && x <= 19 && y >= 13 && y <= 19)
                    {
                        lockedColorData[i] = Color.Gold;
                    }
                    else
                    {
                        lockedColorData[i] = Color.Brown;
                    }
                }
                doorLockedTexture.SetData(lockedColorData);
                
                // Unlocked texture (lighter brown, no keyhole)
                Color[] unlockedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    unlockedColorData[i] = Color.SandyBrown;
                }
                doorUnlockedTexture.SetData(unlockedColorData);
            }
            
            // Load key texture
            Texture2D keyTexture;
            try
            {
                keyTexture = Content.Load<Texture2D>("KeySprite");
                Console.WriteLine("Key texture loaded successfully");
            }
            catch (Exception)
            {
                // Create a simple gold key placeholder
                keyTexture = new Texture2D(GraphicsDevice, 16, 16);
                Color[] colorData = new Color[256];
                for (int i = 0; i < 256; i++)
                {
                    colorData[i] = Color.Gold;
                }
                keyTexture.SetData(colorData);
            }
            
            // Initialize enemy list
            _enemies = new List<Enemy>();
            
            // Create enemies in the GenerateDungeon method
            
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
            
            // Initialize Combat Manager after entities are created
            _combatManager = new CombatManager(
                _player,
                _enemies,
                EnemyTouchDamage,
                AttackDamage,
                DamageBuffMultiplier,
                AetheriumEssenceReward,
                DamageInvincibilityDuration);
                
            // Subscribe to enemy killed event
            _combatManager.EnemyKilled += OnEnemyKilled;
        }
        
        /// <summary>
        /// Generates a new dungeon and positions the player and enemies.
        /// </summary>
        private void GenerateDungeon()
        {
            // Clear existing enemies and loot
            _enemies.Clear();
            _lootItems.Clear();
            
            // Clear environmental elements
            _spikeTraps.Clear();
            _crates.Clear();
            _treasureChests.Clear();
            _doors.Clear();
            
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
            
            // Spawn enemies in rooms (except the starting room)
            if (_currentDungeon?.Rooms.Count > 1)
            {
                // Get all rooms except the starting room
                var availableRooms = new List<Room>(_currentDungeon.Rooms);
                if (_currentDungeon.StartingRoom != null)
                {
                    availableRooms.Remove(_currentDungeon.StartingRoom);
                }
                
                // Find the treasure room if it exists
                Room treasureRoom = null;
                foreach (var room in availableRooms)
                {
                    if (room.Type == RoomType.Treasure)
                    {
                        treasureRoom = room;
                        break;
                    }
                }
                
                // Remove treasure room from available rooms for enemy spawning
                if (treasureRoom != null)
                {
                    availableRooms.Remove(treasureRoom);
                }
                
                // Position weaving altar in a random room (not starting or treasure)
                if (availableRooms.Count > 0)
                {
                    // Choose a room for the altar
                    Room altarRoom = availableRooms[new Random().Next(availableRooms.Count)];
                    availableRooms.Remove(altarRoom); // No enemies in altar room
                    
                    // Position altar in the room center
                    Vector2 altarPosition = altarRoom.Center;
                    
                    // Adjust for altar sprite center
                    altarPosition.X -= (_weavingAltar.Sprite?.Width ?? 0) / 2;
                    altarPosition.Y -= (_weavingAltar.Sprite?.Height ?? 0) / 2;
                    
                    _weavingAltar.Position = altarPosition;
                    
                    Console.WriteLine($"Positioned altar at {altarPosition}");
                }
                
                // Spawn treasure chest in the treasure room
                if (treasureRoom != null)
                {
                    // Retrieve chest textures
                    Texture2D chestClosedTexture = null;
                    Texture2D chestOpenTexture = null;
                    
                    try
                    {
                        chestClosedTexture = Content.Load<Texture2D>("ChestClosedSprite");
                        chestOpenTexture = Content.Load<Texture2D>("ChestOpenSprite");
                    }
                    catch
                    {
                        // Create placeholders if they don't exist yet
                        chestClosedTexture = new Texture2D(GraphicsDevice, 32, 32);
                        Color[] closedColorData = new Color[1024];
                        for (int i = 0; i < 1024; i++) closedColorData[i] = Color.Goldenrod;
                        chestClosedTexture.SetData(closedColorData);
                        
                        chestOpenTexture = new Texture2D(GraphicsDevice, 32, 32);
                        Color[] openColorData = new Color[1024];
                        for (int i = 0; i < 1024; i++)
                        {
                            int x = i % 32;
                            int y = i / 32;
                            if (x >= 8 && x <= 24 && y >= 8 && y <= 24) openColorData[i] = Color.Black;
                            else openColorData[i] = Color.Goldenrod;
                        }
                        chestOpenTexture.SetData(openColorData);
                    }
                    
                    // Position chest in the treasure room center
                    Vector2 chestPosition = treasureRoom.Center;
                    chestPosition.X -= (chestClosedTexture?.Width ?? 0) / 2;
                    chestPosition.Y -= (chestClosedTexture?.Height ?? 0) / 2;
                    
                    TreasureChest chest = new TreasureChest(chestPosition, chestClosedTexture, chestOpenTexture);
                    _treasureChests.Add(chest);
                    
                    Console.WriteLine($"Positioned treasure chest in treasure room at {chestPosition}");
                    
                    // Place a door at the entrance to the treasure room
                    // This requires finding a corridor connected to the treasure room
                    PlaceDoorToRoom(treasureRoom);
                }
                
                // Spawn enemies in the remaining rooms
                foreach (Room room in availableRooms)
                {
                    // Decide how many enemies to spawn based on room size
                    int maxEnemies = Math.Max(1, room.Bounds.Width * room.Bounds.Height / 300000); // 1-3 enemies based on room size
                    int enemiesInRoom = new Random().Next(1, maxEnemies + 1);
                    
                    for (int i = 0; i < enemiesInRoom; i++)
                    {
                        // Random position within the room
                        int enemyX = new Random().Next(room.Bounds.X + 50, room.Bounds.X + room.Bounds.Width - 50);
                        int enemyY = new Random().Next(room.Bounds.Y + 50, room.Bounds.Y + room.Bounds.Height - 50);
                        
                        // Create a random enemy type
                        Enemy enemy = null;
                        int enemyType = new Random().Next(3); // 0 = basic, 1 = fast, 2 = ranged
                        
                        switch (enemyType)
                        {
                            case 0: // Basic enemy
                                enemy = new Enemy(
                                    new Vector2(enemyX, enemyY),
                                    Content.Load<Texture2D>("EnemySprite"),
                                    EnemyHealth);
                                break;
                            case 1: // Fast enemy
                                enemy = new FastEnemy(
                                    new Vector2(enemyX, enemyY),
                                    _fastEnemyTexture,
                                    FastEnemyHealth);
                                break;
                            case 2: // Ranged enemy
                                enemy = new RangedEnemy(
                                    new Vector2(enemyX, enemyY),
                                    _rangedEnemyTexture,
                                    _projectileTexture,
                                    RangedEnemyHealth);
                                break;
                        }
                        
                        if (enemy != null)
                        {
                            _enemies.Add(enemy);
                        }
                    }
                    
                    // Spawn some crates in the room
                    SpawnCratesInRoom(room);
                    
                    // Spawn some spike traps in the room
                    SpawnSpikeTrapsInRoom(room);
                }
            }
        }
        
        /// <summary>
        /// Places destructible crates in a room.
        /// </summary>
        /// <param name="room">The room to place crates in.</param>
        private void SpawnCratesInRoom(Room room)
        {
            // Retrieve crate texture
            Texture2D crateTexture = null;
            
            try
            {
                crateTexture = Content.Load<Texture2D>("CrateSprite");
            }
            catch
            {
                // Create a placeholder if it doesn't exist yet
                crateTexture = new Texture2D(GraphicsDevice, 32, 32);
                Color[] colorData = new Color[1024];
                for (int i = 0; i < 1024; i++) colorData[i] = Color.SaddleBrown;
                crateTexture.SetData(colorData);
            }
            
            // Decide how many crates to spawn based on room size
            int maxCrates = Math.Max(2, room.Bounds.Width * room.Bounds.Height / 250000); // 2-4 crates based on room size
            int cratesInRoom = new Random().Next(2, maxCrates + 1);
            
            for (int i = 0; i < cratesInRoom; i++)
            {
                // Random position within the room (away from the center)
                int crateX, crateY;
                
                // Avoid center of the room (where players/enemies/altars might be)
                do
                {
                    crateX = new Random().Next(room.Bounds.X + 50, room.Bounds.X + room.Bounds.Width - 50);
                    crateY = new Random().Next(room.Bounds.Y + 50, room.Bounds.Y + room.Bounds.Height - 50);
                } while (Vector2.Distance(new Vector2(crateX, crateY), room.Center) < 100);
                
                DestructibleCrate crate = new DestructibleCrate(
                    new Vector2(crateX, crateY),
                    crateTexture,
                    1); // Always set health to 1 for easy destruction
                
                _crates.Add(crate);
            }
            
            Console.WriteLine($"Spawned {cratesInRoom} crates in room at {room.Bounds}");
            
            // Register all crate bounds as obstacles
            UpdateCrateObstacles();
        }
        
        /// <summary>
        /// Updates the dungeon's obstacle list with the bounds of all non-destroyed crates
        /// </summary>
        private void UpdateCrateObstacles()
        {
            // Clear existing obstacle bounds
            _currentDungeon.ClearObstacleBounds();
            
            // Add bounds of all non-destroyed crates
            foreach (var crate in _crates)
            {
                if (!crate.IsDestroyed)
                {
                    _currentDungeon.AddObstacleBound(crate.Bounds);
                }
            }
        }
        
        /// <summary>
        /// Places spike traps in a room.
        /// </summary>
        /// <param name="room">The room to place spike traps in.</param>
        private void SpawnSpikeTrapsInRoom(Room room)
        {
            // Skip spike traps in starting room for player safety
            if (room == _currentDungeon.StartingRoom)
            {
                return;
            }
            
            // Retrieve spike trap textures
            Texture2D spikeTrapsArmedTexture = null;
            Texture2D spikeTrapsDisarmedTexture = null;
            
            try
            {
                spikeTrapsArmedTexture = Content.Load<Texture2D>("SpikeTrapArmedSprite");
                spikeTrapsDisarmedTexture = Content.Load<Texture2D>("SpikeTrapDisarmedSprite");
            }
            catch
            {
                // Create placeholders if they don't exist yet
                spikeTrapsArmedTexture = new Texture2D(GraphicsDevice, 32, 32);
                Color[] armedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    int x = i % 32;
                    int y = i / 32;
                    if (x < 3 || x > 28 || y < 3 || y > 28 || (x >= 13 && x <= 18 && y >= 13 && y <= 18))
                        armedColorData[i] = Color.Red;
                    else
                        armedColorData[i] = Color.DarkRed;
                }
                spikeTrapsArmedTexture.SetData(armedColorData);
                
                spikeTrapsDisarmedTexture = new Texture2D(GraphicsDevice, 32, 32);
                Color[] disarmedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++) disarmedColorData[i] = Color.DarkGray;
                spikeTrapsDisarmedTexture.SetData(disarmedColorData);
            }
            
            // Decide how many traps to spawn based on room size
            int maxTraps = Math.Max(1, room.Bounds.Width * room.Bounds.Height / 300000); // 1-3 traps based on room size
            int trapsInRoom = new Random().Next(1, maxTraps + 1);
            
            for (int i = 0; i < trapsInRoom; i++)
            {
                // Random position within the room
                int trapX = new Random().Next(room.Bounds.X + 50, room.Bounds.X + room.Bounds.Width - 50);
                int trapY = new Random().Next(room.Bounds.Y + 50, room.Bounds.Y + room.Bounds.Height - 50);
                
                // Randomize trap timings
                float activeDuration = 0.5f + (float)new Random().NextDouble() * 1.0f; // 0.5 to 1.5 seconds
                float cooldownDuration = 1.0f + (float)new Random().NextDouble() * 2.0f; // 1 to 3 seconds
                
                SpikeTrap trap = new SpikeTrap(
                    new Vector2(trapX, trapY),
                    spikeTrapsArmedTexture,
                    spikeTrapsDisarmedTexture);
                
                trap.ActiveDuration = activeDuration;
                trap.CooldownDuration = cooldownDuration;
                
                _spikeTraps.Add(trap);
            }
            
            Console.WriteLine($"Spawned {trapsInRoom} spike traps in room at {room.Bounds}");
        }
        
        /// <summary>
        /// Places a door at the entrance to a room.
        /// </summary>
        /// <param name="room">The room to place a door for.</param>
        private void PlaceDoorToRoom(Room room)
        {
            // Retrieve door textures
            Texture2D doorLockedTexture = null;
            Texture2D doorUnlockedTexture = null;
            
            try
            {
                doorLockedTexture = Content.Load<Texture2D>("DoorLockedSprite");
                doorUnlockedTexture = Content.Load<Texture2D>("DoorUnlockedSprite");
            }
            catch
            {
                // Create placeholders if they don't exist yet
                doorLockedTexture = new Texture2D(GraphicsDevice, 32, 32);
                Color[] lockedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++)
                {
                    int x = i % 32;
                    int y = i / 32;
                    if (x >= 13 && x <= 19 && y >= 13 && y <= 19) lockedColorData[i] = Color.Gold;
                    else lockedColorData[i] = Color.Brown;
                }
                doorLockedTexture.SetData(lockedColorData);
                
                doorUnlockedTexture = new Texture2D(GraphicsDevice, 32, 32);
                Color[] unlockedColorData = new Color[1024];
                for (int i = 0; i < 1024; i++) unlockedColorData[i] = Color.SandyBrown;
                doorUnlockedTexture.SetData(unlockedColorData);
            }
            
            // Find a random corridor connecting to this room
            // This is a simplified approach - in a real implementation, you would analyze
            // the dungeon structure to find actual corridor-room connections
            
            // For now, we'll place the door at a position near the room edge
            Random random = new Random();
            int side = random.Next(4); // 0 = top, 1 = right, 2 = bottom, 3 = left
            
            Vector2 doorPosition;
            switch (side)
            {
                case 0: // Top
                    doorPosition = new Vector2(
                        room.Bounds.X + room.Bounds.Width / 2,
                        room.Bounds.Y + 20);
                    break;
                case 1: // Right
                    doorPosition = new Vector2(
                        room.Bounds.X + room.Bounds.Width - 20,
                        room.Bounds.Y + room.Bounds.Height / 2);
                    break;
                case 2: // Bottom
                    doorPosition = new Vector2(
                        room.Bounds.X + room.Bounds.Width / 2,
                        room.Bounds.Y + room.Bounds.Height - 20);
                    break;
                case 3: // Left
                default:
                    doorPosition = new Vector2(
                        room.Bounds.X + 20,
                        room.Bounds.Y + room.Bounds.Height / 2);
                    break;
            }
            
            // Adjust for door sprite center
            doorPosition.X -= (doorLockedTexture?.Width ?? 0) / 2;
            doorPosition.Y -= (doorLockedTexture?.Height ?? 0) / 2;
            
            Door door = new Door(doorPosition, doorLockedTexture, doorUnlockedTexture);
            _doors.Add(door);
            
            Console.WriteLine($"Placed door to {room.Type} room at {doorPosition}");
            
            // Also place a key somewhere in the dungeon in a non-treasure room
            SpawnKeyInRandomRoom(room);
        }
        
        /// <summary>
        /// Spawns a key in a random room (not the specified room).
        /// </summary>
        /// <param name="excludedRoom">The room that should not contain the key.</param>
        private void SpawnKeyInRandomRoom(Room excludedRoom)
        {
            // Retrieve key texture
            Texture2D keyTexture = null;
            
            try
            {
                keyTexture = Content.Load<Texture2D>("KeySprite");
            }
            catch
            {
                // Create a placeholder if it doesn't exist yet
                keyTexture = new Texture2D(GraphicsDevice, 16, 16);
                Color[] colorData = new Color[256];
                for (int i = 0; i < 256; i++) colorData[i] = Color.Gold;
                keyTexture.SetData(colorData);
            }
            
            // Find a valid room to place the key (not the starting room or the excluded room)
            var availableRooms = new List<Room>(_currentDungeon.Rooms);
            availableRooms.Remove(_currentDungeon.StartingRoom);
            availableRooms.Remove(excludedRoom);
            
            if (availableRooms.Count > 0)
            {
                // Choose a random room for the key
                Room keyRoom = availableRooms[new Random().Next(availableRooms.Count)];
                
                // Random position within the room
                int keyX = new Random().Next(keyRoom.Bounds.X + 100, keyRoom.Bounds.X + keyRoom.Bounds.Width - 100);
                int keyY = new Random().Next(keyRoom.Bounds.Y + 100, keyRoom.Bounds.Y + keyRoom.Bounds.Height - 100);
                
                // Create a key item
                LootItem keyItem = new LootItem(new Vector2(keyX, keyY), keyTexture, LootType.Key);
                _lootItems.Add(keyItem);
                
                Console.WriteLine($"Placed key in room at position {keyX}, {keyY}");
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

        /// <summary>
        /// Event handler for enemy killed events, handles loot drops.
        /// </summary>
        /// <param name="killedPosition">The position where the enemy was killed.</param>
        private void OnEnemyKilled(Vector2 killedPosition)
        {
            // Random chance to spawn a health potion
            if (Random.Shared.NextDouble() < EnemySpawnChance)
            {
                // Create health potion at the enemy's position
                var healthPotion = new LootItem(killedPosition, _healthPotionTexture, LootType.HealthPotion);
                _lootItems.Add(healthPotion);
                
                Console.WriteLine("Health potion dropped at enemy death location");
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
            
            // Update player
            _player.Update(gameTime, _inputManager, _currentDungeon);
            
            // DEBUG: Press F1 to destroy all crates (test crate destruction)
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                foreach (var crate in _crates)
                {
                    if (!crate.IsDestroyed)
                    {
                        bool wasDestroyed = crate.TakeDamage(100); // Ensure destruction
                        if (wasDestroyed)
                        {
                            Console.WriteLine("DEBUG: Crate destroyed by F1 key!");
                            
                            if (crate.ShouldDropLoot())
                            {
                                _lootItems.Add(new LootItem(
                                    crate.Position,
                                    _healthPotionTexture,
                                    LootType.HealthPotion));
                                
                                _popupMessage = "Crate destroyed by debug key! Found a Health Potion!";
                                _popupTimer = POPUP_DURATION;
                            }
                        }
                    }
                }
                // Update crate obstacles
                UpdateCrateObstacles();
            }
            
            // Transfer player's projectiles to the game's projectile list
            List<Projectile> playerProjectiles = _player.GetActiveProjectiles();
            for (int i = 0; i < playerProjectiles.Count; i++)
            {
                if (!_projectiles.Contains(playerProjectiles[i]))
                {
                    _projectiles.Add(playerProjectiles[i]);
                    Console.WriteLine("Added player projectile to game projectiles");
                }
            }
            
            // Check for player attacks hitting crates
            if (_player.IsAttacking)
            {
                // Check for player attacks hitting enemies
                for (int i = _enemies.Count - 1; i >= 0; i--)
                {
                    if (_enemies[i].IsActive && _player.AttackHitbox.Intersects(_enemies[i].Bounds))
                    {
                        // Apply damage to the enemy
                        _combatManager.ApplyPlayerAttackDamage(_enemies[i]);
                        Console.WriteLine($"Player hit enemy with melee attack!");
                    }
                }
                
                for (int i = _crates.Count - 1; i >= 0; i--)
                {
                    if (!_crates[i].IsDestroyed && _player.AttackHitbox.Intersects(_crates[i].Bounds))
                    {
                        bool wasDestroyed = _crates[i].TakeDamage(AttackDamage);
                        if (wasDestroyed && _crates[i].ShouldDropLoot())
                        {
                            // Spawn loot when crate is destroyed
                            int lootChance = new Random().Next(100);
                            if (lootChance < 70) // 70% chance of health potion
                            {
                                _lootItems.Add(new LootItem(
                                    _crates[i].Position,
                                    _healthPotionTexture,
                                    LootType.HealthPotion));
                                    
                                _popupMessage = "Crate destroyed! Found a Health Potion!";
                                _popupTimer = POPUP_DURATION;
                            }
                        }
                        
                        // Update obstacle list when a crate is damaged (it might be destroyed)
                        UpdateCrateObstacles();
                    }
                }
            }
            
            // Update camera to follow player with smooth interpolation
            _camera.MoveToTarget(_player.Position, CameraLerpFactor);
            
            // Update enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i].IsActive)
                {
                    _enemies[i].Update(_player.Position, deltaTime, _currentDungeon);
                    
                    // Check for enemy-player collision for touch damage
                    // Only if player is not invincible and enemy isn't attacking (to avoid double damage when attacking)
                    if (!_player.IsInvincible && _damageInvincibilityTimer <= 0 && _enemies[i].Bounds.Intersects(_player.Bounds))
                    {
                        // Apply enemy touch damage to player
                        _player.TakeDamage(EnemyTouchDamage);
                        
                        // Set player damage invincibility
                        _damageInvincibilityTimer = DamageInvincibilityDuration;
                        
                        Console.WriteLine($"Player touched by enemy! Took {EnemyTouchDamage} damage.");
                    }
                    
                    // Check for enemy attacks hitting player
                    if (_enemies[i].IsAttacking && !_player.IsInvincible && _damageInvincibilityTimer <= 0 && 
                        _enemies[i].AttackHitbox.Intersects(_player.Bounds))
                    {
                        // Apply enemy attack damage to player
                        _player.TakeDamage(EnemyTouchDamage);
                        
                        // Set player damage invincibility
                        _damageInvincibilityTimer = DamageInvincibilityDuration;
                        
                        Console.WriteLine($"Player hit by enemy attack! Took {EnemyTouchDamage} damage.");
                    }
                }
                else
                {
                    _enemies.RemoveAt(i);
                }
            }
            
            // Update projectiles
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _projectiles[i];
                
                if (!projectile.IsActive)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }
                
                // Store the old position for collision detection
                Vector2 oldPosition = projectile.Position;
                
                // Update projectile position
                projectile.Update(gameTime, _currentDungeon);
                
                // For player projectiles, check all possible collisions
                if (projectile.IsPlayerOwned)
                {
                    bool hitSomething = false;
                    
                    // DIRECT CRATE COLLISION CHECK - Check all crates first
                    for (int j = 0; j < _crates.Count; j++)
                    {
                        if (!_crates[j].IsDestroyed)
                        {
                            // Calculate distance between projectile and crate center
                            Vector2 crateCenter = new Vector2(
                                _crates[j].Position.X + (_crates[j].Sprite?.Width ?? 0) / 2,
                                _crates[j].Position.Y + (_crates[j].Sprite?.Height ?? 0) / 2
                            );
                            
                            float distance = Vector2.Distance(projectile.Position, crateCenter);
                            
                            // Use full crate width as collision radius
                            float collisionRadius = Math.Max((_crates[j].Sprite?.Width ?? 0), (_crates[j].Sprite?.Height ?? 0));
                            
                            // Check if projectile is close enough to crate or intersects with it
                            if (distance < collisionRadius || projectile.Bounds.Intersects(_crates[j].Bounds))
                            {
                                // Force destroy the crate with maximum damage
                                Console.WriteLine($"DIRECT HIT! Projectile at {projectile.Position} hit crate at {_crates[j].Position}, distance: {distance}");
                                
                                // Apply massive damage to ensure destruction
                                bool wasDestroyed = _crates[j].TakeDamage(1000);
                                
                                // Double-check if crate is now marked as destroyed
                                if (!_crates[j].IsDestroyed)
                                {
                                    _crates[j].TakeDamage(1000); // Try again with even more damage
                                    Console.WriteLine("Applied additional damage to ensure destruction");
                                }
                                
                                if (_crates[j].IsDestroyed && _crates[j].ShouldDropLoot())
                                {
                                    // Spawn loot
                                    _lootItems.Add(new LootItem(
                                        _crates[j].Position,
                                        _healthPotionTexture,
                                        LootType.HealthPotion));
                                    
                                    _popupMessage = "Crate destroyed by spell! Found a Health Potion!";
                                    _popupTimer = POPUP_DURATION;
                                }
                                
                                // Update obstacle list to remove the destroyed crate
                                UpdateCrateObstacles();
                                
                                // Deactivate projectile
                                projectile.Deactivate();
                                hitSomething = true;
                                break;
                            }
                        }
                    }
                    
                    // If didn't hit a crate, check enemies
                    if (!hitSomething)
                    {
                        for (int j = 0; j < _enemies.Count; j++)
                        {
                            if (_enemies[j].IsActive && projectile.Bounds.Intersects(_enemies[j].Bounds))
                            {
                                // Damage the enemy
                                _combatManager.ApplyDamageToEnemy(_enemies[j], projectile.Damage);
                                Console.WriteLine($"Enemy hit by projectile! Took {projectile.Damage} damage!");
                                
                                // Deactivate the projectile
                                projectile.Deactivate();
                                hitSomething = true;
                                break;
                            }
                        }
                    }
                }
                // Handle enemy projectiles hitting player
                else if (!projectile.IsPlayerOwned && !_player.IsInvincible && _damageInvincibilityTimer <= 0 && 
                         projectile.Bounds.Intersects(_player.Bounds))
                {
                    // Player takes damage from enemy projectiles
                    _player.TakeDamage(projectile.Damage);
                    
                    // Trigger invincibility
                    _damageInvincibilityTimer = DamageInvincibilityDuration;
                    
                    // Deactivate the projectile
                    projectile.Deactivate();
                }
            }
            
            // Update spike traps
            foreach (var trap in _spikeTraps)
            {
                trap.Update(gameTime);
                
                // Check for player collision with active traps
                if (trap.IsActive && !trap.IsEntityOnDamageCooldown(_player) && 
                    trap.Bounds.Intersects(_player.Bounds) && !_player.IsInvincible)
                {
                    _player.TakeDamage(trap.Damage);
                    trap.SetEntityDamageCooldown(_player);
                    Console.WriteLine($"Player hit by spike trap! Took {trap.Damage} damage.");
                }
                
                // Check for enemy collision with active traps
                foreach (var enemy in _enemies)
                {
                    if (trap.IsActive && !trap.IsEntityOnDamageCooldown(enemy) &&
                        trap.Bounds.Intersects(enemy.Bounds))
                    {
                        // Use combat manager to apply damage to enemy
                        _combatManager.ApplyDamageToEnemy(enemy, trap.Damage);
                        trap.SetEntityDamageCooldown(enemy);
                        Console.WriteLine($"Enemy hit by spike trap! Took {trap.Damage} damage.");
                    }
                }
            }
            
            // Check for treasure chest interaction
            _isNearChest = false;
            _nearbyChest = null;
            
            foreach (var chest in _treasureChests)
            {
                if (Vector2.Distance(_player.Position, chest.Position) < InteractionDistance)
                {
                    _isNearChest = true;
                    _nearbyChest = chest;
                    
                    if (_inputManager.IsActionJustPressed(InputManager.GameAction.Interact))
                    {
                        if (chest.Open() && chest.ShouldDropLoot())
                        {
                            // Spawn multiple loot items when chest is opened
                            // 1-3 health potions
                            int potionCount = new Random().Next(1, 4);
                            for (int i = 0; i < potionCount; i++)
                            {
                                float offsetX = (float)(new Random().NextDouble() * 60 - 30);
                                float offsetY = (float)(new Random().NextDouble() * 60 - 30);
                                Vector2 potionPos = new Vector2(chest.Position.X + offsetX, chest.Position.Y + offsetY);
                                _lootItems.Add(new LootItem(potionPos, _healthPotionTexture, LootType.HealthPotion));
                            }
                            
                            // Give some Aetherium Essence
                            int essenceAmount = new Random().Next(5, 11); // 5-10 essence
                            _player.AddAetheriumEssence(essenceAmount);
                            
                            // Show popup notification
                            _popupMessage = $"Obtained {potionCount} Health Potion{(potionCount > 1 ? "s" : "")} and {essenceAmount} Aetherium Essence!";
                            _popupTimer = POPUP_DURATION;
                            
                            Console.WriteLine($"Treasure chest opened! Found {potionCount} health potions and {essenceAmount} essence.");
                        }
                    }
                }
            }
            
            // Check for door interaction
            foreach (var door in _doors)
            {
                if (door.IsLocked && 
                    Vector2.Distance(_player.Position, door.Position) < InteractionDistance &&
                    _inputManager.IsActionJustPressed(InputManager.GameAction.Interact))
                {
                    if (_player.KeyCount > 0)
                    {
                        _player.UseKey();
                        door.Unlock();
                        Console.WriteLine("Door unlocked with a key!");
                    }
                    else
                    {
                        Console.WriteLine("This door is locked. You need a key to unlock it.");
                    }
                }
            }
            
            // Process loot collection
            for (int i = _lootItems.Count - 1; i >= 0; i--)
            {
                LootItem item = _lootItems[i];
                if (item.IsActive && item.Bounds.Intersects(_player.Bounds))
                {
                    // Process the item based on its type
                    switch (item.Type)
                    {
                        case LootType.HealthPotion:
                            // Health potions restore health
                            _player.RestoreHealth(LootItem.HealthPotionAmount);
                            _popupMessage = $"Obtained Health Potion! Restored {LootItem.HealthPotionAmount} health.";
                            _popupTimer = POPUP_DURATION;
                            Console.WriteLine($"Collected health potion. Restored {LootItem.HealthPotionAmount} health!");
                            break;
                            
                        case LootType.Key:
                            // Keys are added to the player's inventory
                            _player.AddKey();
                            _popupMessage = "Obtained Key!";
                            _popupTimer = POPUP_DURATION;
                            Console.WriteLine("Collected a key!");
                            break;
                    }
                    
                    // Mark the item as collected
                    item.Collect();
                    _lootItems.RemoveAt(i);
                }
            }
            
            // Check for interaction with weaving altar
            if (Vector2.Distance(_player.Position, _weavingAltar.Position) < InteractionDistance)
            {
                if (_inputManager.IsActionJustPressed(InputManager.GameAction.Interact))
                {
                    // Show the interaction prompt for weaving altar
                    _isNearAltar = true;
                    
                    // Check if player is pressing one of the buff activation keys
                    if (_inputManager.CheckKeyJustPressed(Keys.D1) && _player.AetheriumEssence >= AetheriumWeavingCost)
                    {
                        if (_player.ActivateDamageBuff(AetheriumWeavingCost))
                        {
                            Console.WriteLine("Damage buff activated!");
                        }
                    }
                    else if (_inputManager.CheckKeyJustPressed(Keys.D2) && _player.AetheriumEssence >= AetheriumWeavingCost)
                    {
                        if (_player.ActivateSpeedBuff(AetheriumWeavingCost))
                        {
                            Console.WriteLine("Speed buff activated!");
                        }
                    }
                }
            }
            else
            {
                _isNearAltar = false;
            }
            
            // Update combat-related timers
            if (_damageInvincibilityTimer > 0)
            {
                _damageInvincibilityTimer -= deltaTime;
            }
            
            // Update popup timer
            if (_popupTimer > 0)
            {
                _popupTimer -= deltaTime;
                if (_popupTimer <= 0)
                {
                    _popupMessage = "";
                }
            }
            
            // Process game state transitions
            if (_player.CurrentHealth <= 0)
            {
                _stateManager.ChangeState(StateManager.GameState.GameOver);
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
            GraphicsDevice.Clear(Color.Black);
            
            // Begin sprite batch with camera transformation for world rendering
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                _camera.GetTransformMatrix());
            
            // Draw the dungeon
            DrawDungeon();
            
            // Draw environmental elements
            
            // Draw spike traps
            foreach (var trap in _spikeTraps)
            {
                trap.Draw(_spriteBatch);
            }
            
            // Draw crates
            foreach (var crate in _crates)
            {
                crate.Draw(_spriteBatch);
            }
            
            // Draw treasure chests
            foreach (var chest in _treasureChests)
            {
                chest.Draw(_spriteBatch);
            }
            
            // Draw doors
            foreach (var door in _doors)
            {
                door.Draw(_spriteBatch);
            }
            
            // Draw weaving altar
            if (_weavingAltar != null)
            {
                _spriteBatch.Draw(
                    _weavingAltar.Sprite,
                    _weavingAltar.Position,
                    null,
                    Color.White);
            }
            
            // Draw loot items
            foreach (var lootItem in _lootItems)
            {
                lootItem.Draw(_spriteBatch);
            }
            
            // Draw projectiles
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(_spriteBatch);
            }
            
            // Draw enemies
            foreach (Enemy enemy in _enemies)
            {
                // Draw the enemy
                enemy.Draw(_spriteBatch);
                
                // Draw attack hitbox if debugging
                if (enemy.IsAttacking)
                {
                    enemy.DrawAttackHitbox(_spriteBatch, _debugTexture);
                }
            }
            
            // Draw player
            _player.Draw(_spriteBatch, _combatManager.PlayerDamageInvincibilityTimer);
            
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
            Rectangle uiBackground = new Rectangle(10, 10, 240, 200);
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
            
            // === MANA DISPLAY ===
            int yPos = 65;
            
            // Draw mana bar label
            if (_gameFont != null)
            {
                _spriteBatch.DrawString(_gameFont, "MANA", new Vector2(20, yPos), Color.White);
            }
            
            yPos += 20;
            
            // Draw mana bar
            int manaBarWidth = 200;
            Rectangle manaBarBackground = new Rectangle(20, yPos, manaBarWidth, 20);
            Rectangle manaBarFill = new Rectangle(
                20, 
                yPos, 
                (int)(manaBarWidth * ((float)_player.CurrentMana / _player.MaxMana)), 
                20);
            
            // Background (dark blue)
            _spriteBatch.Draw(_debugTexture, manaBarBackground, new Color((byte)0, (byte)0, (byte)100, (byte)200));
            // Fill (bright blue)
            _spriteBatch.Draw(_debugTexture, manaBarFill, new Color((byte)0, (byte)100, (byte)255, (byte)220));
            
            // Draw mana text
            if (_gameFont != null)
            {
                string manaText = $"{_player.CurrentMana}/{_player.MaxMana}";
                Vector2 textSize = _gameFont.MeasureString(manaText);
                Vector2 textPosition = new Vector2(
                    manaBarBackground.X + (manaBarBackground.Width - textSize.X) / 2,
                    manaBarBackground.Y + (manaBarBackground.Height - textSize.Y) / 2);
                _spriteBatch.DrawString(_gameFont, manaText, textPosition, Color.White);
            }
            
            // === SPELL COOLDOWN ===
            yPos += 30;
            
            // Draw spell cooldown indicator
            float spellCooldown = _player.GetSpellCooldownRemaining();
            if (_gameFont != null)
            {
                string spellText;
                Color spellColor;
                
                if (spellCooldown <= 0)
                {
                    spellText = "Spell: READY";
                    spellColor = Color.Cyan;
                }
                else
                {
                    spellText = $"Spell: {spellCooldown:F1}s";
                    spellColor = Color.Gray;
                }
                
                _spriteBatch.DrawString(_gameFont, spellText, new Vector2(20, yPos), spellColor);
            }
            
            // === ESSENCE DISPLAY ===
            yPos += 30;
            
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
            yPos += 30;
            
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
                
                // Draw loot items on minimap
                foreach (var lootItem in _lootItems)
                {
                    if (lootItem.IsActive)
                    {
                        int lootDotSize = 3;
                        Rectangle lootDot = new Rectangle(
                            (int)(minimapCenter.X + (lootItem.Position.X - worldCenter.X) * scale) - lootDotSize / 2,
                            (int)(minimapCenter.Y + (lootItem.Position.Y - worldCenter.Y) * scale) - lootDotSize / 2,
                            lootDotSize,
                            lootDotSize);
                        
                        // Choose color based on loot type
                        Color lootColor = lootItem.Type == LootType.HealthPotion ? Color.Red : Color.White;
                        _spriteBatch.Draw(_debugTexture, lootDot, lootColor);
                    }
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
                            
                            // Use different colors for enemy types
                            Color enemyColor = Color.Red; // Default red for basic enemies
                            
                            if (enemy is RangedEnemy)
                            {
                                enemyColor = Color.OrangeRed; // Orange for ranged enemies
                            }
                            else if (enemy is FastEnemy)
                            {
                                enemyColor = Color.Lime; // Green for fast enemies
                            }
                            
                            _spriteBatch.Draw(_debugTexture, enemyDot, enemyColor);
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
            if (_isNearAltar)
            {
                // Draw interaction prompt panel
                Rectangle promptBackground = new Rectangle(
                    GraphicsDevice.Viewport.Width / 2 - 150,
                    GraphicsDevice.Viewport.Height - 100,
                    300,
                    70);
                _spriteBatch.Draw(_debugTexture, promptBackground, new Color((byte)0, (byte)0, (byte)0, (byte)180));
                
                // Draw interaction prompt text
                if (_gameFont != null)
                {
                    string promptText = "Press E to interact with Altar";
                    Vector2 textSize = _gameFont.MeasureString(promptText);
                    Vector2 textPosition = new Vector2(
                        promptBackground.X + (promptBackground.Width - textSize.X) / 2,
                        promptBackground.Y + 10);
                    _spriteBatch.DrawString(_gameFont, promptText, textPosition, Color.White);
                    
                    string buffText = "Press 1 for Damage Buff (3 Essence)\nPress 2 for Speed Buff (3 Essence)";
                    Vector2 buffTextSize = _gameFont.MeasureString(buffText);
                    Vector2 buffTextPosition = new Vector2(
                        promptBackground.X + (promptBackground.Width - buffTextSize.X) / 2,
                        promptBackground.Y + 35);
                    _spriteBatch.DrawString(_gameFont, buffText, buffTextPosition, Color.LightGray);
                }
            }
            
            // If near a chest, draw interaction prompt
            if (_isNearChest && _nearbyChest != null)
            {
                // Draw interaction prompt panel
                Rectangle promptBackground = new Rectangle(
                    GraphicsDevice.Viewport.Width / 2 - 150,
                    GraphicsDevice.Viewport.Height - 100,
                    300,
                    70);
                _spriteBatch.Draw(_debugTexture, promptBackground, new Color((byte)0, (byte)0, (byte)0, (byte)180));
                
                // Draw interaction prompt text
                if (_gameFont != null)
                {
                    string promptText = _nearbyChest.IsOpen 
                        ? "Treasure Chest is Open" 
                        : "Press E to Open Treasure Chest";
                    
                    Vector2 textSize = _gameFont.MeasureString(promptText);
                    Vector2 textPosition = new Vector2(
                        promptBackground.X + (promptBackground.Width - textSize.X) / 2,
                        promptBackground.Y + 25);
                    
                    Color textColor = _nearbyChest.IsOpen ? Color.Gray : Color.Gold;
                    _spriteBatch.DrawString(_gameFont, promptText, textPosition, textColor);
                }
            }
            
            // Draw spell casting prompt in bottom right
            if (_gameFont != null)
            {
                string spellPrompt = "Press Q to cast spell";
                Vector2 promptSize = _gameFont.MeasureString(spellPrompt);
                Vector2 promptPos = new Vector2(
                    GraphicsDevice.Viewport.Width - promptSize.X - 20,
                    GraphicsDevice.Viewport.Height - promptSize.Y - 20);
                
                // Draw with appropriate color based on spell readiness
                Color promptColor = _player.IsSpellReady ? Color.Cyan : Color.Gray;
                _spriteBatch.DrawString(_gameFont, spellPrompt, promptPos, promptColor);
            }
            
            // Draw popup notification if active
            if (_popupTimer > 0 && !string.IsNullOrEmpty(_popupMessage) && _gameFont != null)
            {
                // Calculate popup position (center of screen, top third)
                Vector2 textSize = _gameFont.MeasureString(_popupMessage);
                Vector2 position = new Vector2(
                    (GraphicsDevice.Viewport.Width - textSize.X) / 2,
                    GraphicsDevice.Viewport.Height / 3);
                
                // Draw background panel
                Rectangle panel = new Rectangle(
                    (int)position.X - 20,
                    (int)position.Y - 10,
                    (int)textSize.X + 40,
                    (int)textSize.Y + 20);
                
                _spriteBatch.Draw(_debugTexture, panel, new Color((byte)0, (byte)0, (byte)0, (byte)200));
                
                // Calculate fade effect for last second
                float alpha = 1.0f;
                if (_popupTimer < 1.0f)
                {
                    alpha = _popupTimer; // Linear fade out during last second
                }
                
                // Draw text with fade effect
                Color textColor = new Color(Color.Gold.R, Color.Gold.G, Color.Gold.B, (byte)(alpha * 255));
                _spriteBatch.DrawString(_gameFont, _popupMessage, position, textColor);
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