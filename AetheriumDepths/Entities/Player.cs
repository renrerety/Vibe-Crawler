using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AetheriumDepths.Generation;
using System.Collections.Generic;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents the player character in the game.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Current position of the player in the game world.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The texture used to render the player.
        /// </summary>
        public Texture2D Sprite { get; set; }

        /// <summary>
        /// The last non-zero movement direction of the player.
        /// Used for attack direction.
        /// </summary>
        public Vector2 LastMovementDirection { get; private set; } = new Vector2(0, 1); // Default to facing down

        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            Sprite?.Width ?? 0, 
            Sprite?.Height ?? 0);

        /// <summary>
        /// Amount of Aetherium Essence the player has collected.
        /// Used for weaving abilities at altars.
        /// </summary>
        public int AetheriumEssence { get; private set; } = 0;

        /// <summary>
        /// Flag indicating if the player has an active damage buff from weaving.
        /// </summary>
        public bool HasDamageBuff { get; private set; } = false;
        
        /// <summary>
        /// Remaining duration of the damage buff in seconds.
        /// </summary>
        private float _damageBuffDuration = 0f;
        
        /// <summary>
        /// Flag indicating if the player has an active speed buff from weaving.
        /// </summary>
        public bool HasSpeedBuff { get; private set; } = false;
        
        /// <summary>
        /// Remaining duration of the speed buff in seconds.
        /// </summary>
        private float _speedBuffDuration = 0f;
        
        /// <summary>
        /// Duration of activated buffs in seconds.
        /// </summary>
        private const float BUFF_DURATION = 10f;
        
        /// <summary>
        /// Speed multiplier when speed buff is active.
        /// </summary>
        private const float SPEED_BUFF_MULTIPLIER = 1.5f;

        /// <summary>
        /// The player's current health points.
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// The player's maximum health points.
        /// </summary>
        public int MaxHealth { get; private set; } = 100; // Default max health

        /// <summary>
        /// Flag indicating if an attack is currently active.
        /// </summary>
        public bool IsAttacking { get; private set; }

        /// <summary>
        /// Timer for the duration of the attack.
        /// </summary>
        private float _attackTimer;

        /// <summary>
        /// Duration of the attack in seconds.
        /// </summary>
        private const float ATTACK_DURATION = 0.2f;

        /// <summary>
        /// The hitbox for the current attack.
        /// </summary>
        public Rectangle AttackHitbox { get; private set; }

        /// <summary>
        /// The size of the attack hitbox.
        /// </summary>
        private const int ATTACK_SIZE = 32;
        
        /// <summary>
        /// Flag indicating if a dodge is currently active.
        /// </summary>
        public bool IsDodging { get; private set; }
        
        /// <summary>
        /// Flag indicating if the player is currently invincible.
        /// </summary>
        public bool IsInvincible => IsDodging; // Currently only invincible during dodges
        
        /// <summary>
        /// Timer for the duration of the dodge.
        /// </summary>
        private float _dodgeTimer;
        
        /// <summary>
        /// Duration of the dodge in seconds.
        /// </summary>
        private const float DODGE_DURATION = 0.3f;
        
        /// <summary>
        /// Speed multiplier during dodge.
        /// </summary>
        private const float DODGE_SPEED_MULTIPLIER = 2.5f;

        /// <summary>
        /// Current mana for spell casting.
        /// </summary>
        public int CurrentMana { get; private set; }

        /// <summary>
        /// Maximum mana capacity.
        /// </summary>
        public int MaxMana { get; private set; } = 100; // Default max mana

        /// <summary>
        /// Timer for spell cooldown.
        /// </summary>
        private float _spellCooldownTimer = 0f;

        /// <summary>
        /// Duration of the spell cooldown in seconds.
        /// </summary>
        private const float SPELL_COOLDOWN_DURATION = 0.7f;

        /// <summary>
        /// Flag indicating if a spell cast is ready.
        /// </summary>
        public bool IsSpellReady => _spellCooldownTimer <= 0f;

        /// <summary>
        /// Cost in mana to cast a spell.
        /// </summary>
        private const int SPELL_MANA_COST = 10;

        /// <summary>
        /// List of active projectiles fired by the player.
        /// </summary>
        private List<Projectile> _activeProjectiles = new List<Projectile>();

        /// <summary>
        /// The texture used for projectiles fired by the player.
        /// </summary>
        private Texture2D _projectileTexture;

        /// <summary>
        /// Number of keys the player has collected.
        /// </summary>
        public int KeyCount { get; private set; } = 0;

        /// <summary>
        /// Creates a new player at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the player.</param>
        /// <param name="sprite">The sprite texture for the player.</param>
        /// <param name="projectileTexture">The texture for projectiles fired by the player.</param>
        public Player(Vector2 position, Texture2D sprite, Texture2D projectileTexture = null)
        {
            Position = position;
            Sprite = sprite;
            _projectileTexture = projectileTexture;
            CurrentHealth = MaxHealth; // Initialize current health to max health
            CurrentMana = MaxMana; // Initialize current mana to max mana
            Console.WriteLine($"Player created with {CurrentHealth}/{MaxHealth} health and {CurrentMana}/{MaxMana} mana");
        }

        /// <summary>
        /// Updates the player's position based on the movement vector.
        /// </summary>
        /// <param name="movementVector">The direction to move in.</param>
        /// <param name="speed">The speed at which to move.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        public void Move(Vector2 movementVector, float speed, float deltaTime, Dungeon dungeon)
        {
            // Apply movement
            float effectiveSpeed = speed;
            
            // Apply speed buff if active
            if (HasSpeedBuff)
            {
                effectiveSpeed *= SPEED_BUFF_MULTIPLIER;
            }
            
            // Apply dodge speed boost if dodging
            if (IsDodging)
            {
                effectiveSpeed *= DODGE_SPEED_MULTIPLIER;
            }
            
            // If dodging with no movement vector, use last movement direction
            Vector2 effectiveMovementVector = movementVector;
            if (IsDodging && movementVector == Vector2.Zero && LastMovementDirection != Vector2.Zero)
            {
                effectiveMovementVector = LastMovementDirection;
            }
            
            // Calculate the proposed new position
            Vector2 proposedPosition = Position + (effectiveMovementVector * effectiveSpeed * deltaTime);
            
            // Calculate the bounds at the proposed position
            Rectangle proposedBounds = new Rectangle(
                (int)proposedPosition.X,
                (int)proposedPosition.Y,
                Sprite.Width,
                Sprite.Height);
                
            // Check if the proposed movement is valid within the dungeon
            if (dungeon.IsMovementValid(proposedBounds))
            {
                // If the movement is valid, update the position
                Position = proposedPosition;
                
                // If movement direction is non-zero, update LastMovementDirection
                if (movementVector != Vector2.Zero)
                {
                    LastMovementDirection = Vector2.Normalize(movementVector);
                }
            }
            else
            {
                // If movement is invalid, try to slide along walls by attempting X and Y movement separately
                
                // Try moving only in X direction
                Vector2 proposedXPosition = new Vector2(
                    Position.X + (effectiveMovementVector.X * effectiveSpeed * deltaTime),
                    Position.Y);
                    
                Rectangle proposedXBounds = new Rectangle(
                    (int)proposedXPosition.X,
                    (int)proposedXPosition.Y,
                    Sprite.Width,
                    Sprite.Height);
                    
                if (dungeon.IsMovementValid(proposedXBounds))
                {
                    Position = proposedXPosition;
                }
                
                // Try moving only in Y direction
                Vector2 proposedYPosition = new Vector2(
                    Position.X,
                    Position.Y + (effectiveMovementVector.Y * effectiveSpeed * deltaTime));
                    
                Rectangle proposedYBounds = new Rectangle(
                    (int)proposedYPosition.X,
                    (int)proposedYPosition.Y,
                    Sprite.Width,
                    Sprite.Height);
                    
                if (dungeon.IsMovementValid(proposedYBounds))
                {
                    Position = proposedYPosition;
                }
                
                // Update LastMovementDirection if the movement vector is non-zero
                if (movementVector != Vector2.Zero)
                {
                    LastMovementDirection = Vector2.Normalize(movementVector);
                }
            }
        }

        /// <summary>
        /// Updates the player state and handles input.
        /// </summary>
        /// <param name="gameTime">The game time information.</param>
        /// <param name="inputManager">The input manager.</param>
        /// <param name="dungeon">The current dungeon for movement validation.</param>
        public void Update(GameTime gameTime, Core.InputManager inputManager, Generation.Dungeon dungeon)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Handle movement input
            Vector2 movementVector = inputManager.GetMovementVector();
            Move(movementVector, 200f, deltaTime, dungeon); // 200f is PlayerSpeed
            
            // Handle attack input
            if (inputManager.IsActionJustPressed(Core.InputManager.GameAction.Attack))
            {
                Attack();
            }
            
            // Handle dodge input
            if (inputManager.IsActionJustPressed(Core.InputManager.GameAction.Dodge))
            {
                Dodge();
            }
            
            // Handle spell input
            if (inputManager.IsActionJustPressed(Core.InputManager.GameAction.UseSpell))
            {
                CastSpell();
            }
            
            // Update active projectiles
            UpdateProjectiles(deltaTime, dungeon);
            
            // Update timers
            
            // Attack timer
            if (IsAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0f)
                {
                    IsAttacking = false;
                }
            }
            
            // Dodge timer
            if (IsDodging)
            {
                _dodgeTimer -= deltaTime;
                if (_dodgeTimer <= 0f)
                {
                    IsDodging = false;
                }
            }
            
            // Spell cooldown timer
            if (_spellCooldownTimer > 0f)
            {
                _spellCooldownTimer -= deltaTime;
            }
            
            // Buff durations
            if (HasDamageBuff)
            {
                _damageBuffDuration -= deltaTime;
                if (_damageBuffDuration <= 0f)
                {
                    HasDamageBuff = false;
                    Console.WriteLine("Damage buff expired");
                }
            }
            
            if (HasSpeedBuff)
            {
                _speedBuffDuration -= deltaTime;
                if (_speedBuffDuration <= 0f)
                {
                    HasSpeedBuff = false;
                    Console.WriteLine("Speed buff expired");
                }
            }
            
            // Mana regeneration
            if (CurrentMana < MaxMana)
            {
                const float manaRegenRate = 5f; // Mana points per second
                CurrentMana = Math.Min(MaxMana, CurrentMana + (int)(manaRegenRate * deltaTime));
            }
        }
        
        /// <summary>
        /// Updates all active projectiles.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        private void UpdateProjectiles(float deltaTime, Generation.Dungeon dungeon)
        {
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _activeProjectiles[i];
                
                if (!projectile.IsActive)
                {
                    _activeProjectiles.RemoveAt(i);
                    continue;
                }
                
                projectile.Update(deltaTime, dungeon);
            }
        }

        /// <summary>
        /// Initiates a melee attack in the direction the player is facing.
        /// </summary>
        public void Attack()
        {
            if (!IsAttacking)
            {
                IsAttacking = true;
                _attackTimer = ATTACK_DURATION;
                
                // Calculate attack hitbox position based on player position and last movement direction
                Vector2 hitboxCenter = Position + (LastMovementDirection * (Sprite.Width + ATTACK_SIZE) / 2);
                
                // Create attack hitbox
                AttackHitbox = new Rectangle(
                    (int)(hitboxCenter.X - ATTACK_SIZE / 2),
                    (int)(hitboxCenter.Y - ATTACK_SIZE / 2),
                    ATTACK_SIZE,
                    ATTACK_SIZE);
            }
        }

        /// <summary>
        /// Deactivates the current attack.
        /// </summary>
        public void DeactivateAttack()
        {
            IsAttacking = false;
            _attackTimer = 0f;
        }
        
        /// <summary>
        /// Initiates a dodge action, granting temporary invincibility and a speed boost.
        /// </summary>
        public void Dodge()
        {
            if (!IsDodging)
            {
                IsDodging = true;
                _dodgeTimer = DODGE_DURATION;
            }
        }

        /// <summary>
        /// Adds Aetherium Essence to the player's collection.
        /// </summary>
        /// <param name="amount">Amount of essence to add.</param>
        public void AddAetheriumEssence(int amount)
        {
            AetheriumEssence += amount;
            Console.WriteLine($"Player now has {AetheriumEssence} Aetherium Essence");
        }

        /// <summary>
        /// Reduces the player's health by the specified amount.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        /// <returns>True if player is still alive after taking damage; false if player died.</returns>
        public bool TakeDamage(int damage)
        {
            if (IsInvincible)
            {
                Console.WriteLine("Player is invincible, no damage taken");
                return true;
            }
            
            CurrentHealth -= damage;
            Console.WriteLine($"Player took {damage} damage! Health: {CurrentHealth}/{MaxHealth}");
            
            // Prevent health from going below 0
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
            
            // Return whether the player is still alive
            return CurrentHealth > 0;
        }

        /// <summary>
        /// Activates the damage buff when interacting with a weaving altar.
        /// </summary>
        /// <param name="essenceCost">Amount of essence required to activate the buff.</param>
        /// <returns>True if the buff was activated; false if not enough essence.</returns>
        public bool ActivateDamageBuff(int essenceCost)
        {
            if (AetheriumEssence >= essenceCost)
            {
                AetheriumEssence -= essenceCost;
                HasDamageBuff = true;
                _damageBuffDuration = BUFF_DURATION;
                Console.WriteLine($"Damage buff activated for {BUFF_DURATION} seconds! Player now has {AetheriumEssence} Aetherium Essence");
                return true;
            }
            
            Console.WriteLine($"Not enough essence to activate buff. Have {AetheriumEssence}, need {essenceCost}");
            return false;
        }
        
        /// <summary>
        /// Activates the speed buff when interacting with a weaving altar.
        /// </summary>
        /// <param name="essenceCost">Amount of essence required to activate the buff.</param>
        /// <returns>True if the buff was activated; false if not enough essence.</returns>
        public bool ActivateSpeedBuff(int essenceCost)
        {
            if (AetheriumEssence >= essenceCost)
            {
                AetheriumEssence -= essenceCost;
                HasSpeedBuff = true;
                _speedBuffDuration = BUFF_DURATION;
                Console.WriteLine($"Speed buff activated for {BUFF_DURATION} seconds! Player now has {AetheriumEssence} Aetherium Essence");
                return true;
            }
            
            Console.WriteLine($"Not enough essence to activate buff. Have {AetheriumEssence}, need {essenceCost}");
            return false;
        }
        
        /// <summary>
        /// Gets the remaining duration of a specific buff.
        /// </summary>
        /// <param name="buffType">The type of buff to check.</param>
        /// <returns>The remaining duration in seconds, or 0 if the buff is not active.</returns>
        public float GetBuffDuration(BuffType buffType)
        {
            switch (buffType)
            {
                case BuffType.Damage:
                    return HasDamageBuff ? _damageBuffDuration : 0f;
                case BuffType.Speed:
                    return HasSpeedBuff ? _speedBuffDuration : 0f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Casts a spell projectile in the direction the player is facing.
        /// </summary>
        /// <returns>True if the spell was cast, false if on cooldown or insufficient mana.</returns>
        public bool CastSpell()
        {
            // Check if spell is on cooldown
            if (_spellCooldownTimer > 0f)
            {
                return false;
            }
            
            // Check if player has enough mana
            if (CurrentMana < SPELL_MANA_COST)
            {
                Console.WriteLine("Not enough mana to cast spell");
                return false;
            }
            
            // Check if projectile texture is available
            if (_projectileTexture == null)
            {
                Console.WriteLine("No projectile texture available");
                return false;
            }
            
            // Consume mana
            CurrentMana -= SPELL_MANA_COST;
            
            // Start cooldown
            _spellCooldownTimer = SPELL_COOLDOWN_DURATION;
            
            // Create projectile at player's position offset toward the facing direction
            Vector2 projectileStart = Position + (LastMovementDirection * Sprite.Width / 2);
            
            // Create the projectile with the player's damage
            Projectile projectile = new Projectile(
                projectileStart,
                LastMovementDirection,
                _projectileTexture,
                1000, // Ultra high damage to ensure one-hit destruction
                350f, // Projectile speed
                true); // Mark as player projectile
                
            // Add to active projectiles
            _activeProjectiles.Add(projectile);
            
            Console.WriteLine("Player cast a spell");
            return true;
        }
        
        /// <summary>
        /// Gets all active projectiles fired by the player.
        /// </summary>
        /// <returns>A list of active projectiles.</returns>
        public List<Projectile> GetActiveProjectiles()
        {
            return _activeProjectiles;
        }
        
        /// <summary>
        /// Gets the remaining spell cooldown time.
        /// </summary>
        /// <returns>The cooldown time in seconds.</returns>
        public float GetSpellCooldownRemaining()
        {
            return _spellCooldownTimer;
        }

        /// <summary>
        /// Draws the player to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="damageInvincibility">Optional damage invincibility timer for visual effects.</param>
        public void Draw(SpriteBatch spriteBatch, float damageInvincibility = 0f)
        {
            if (Sprite == null) return;
            
            // Determine the color to draw the player with based on status effects
            Color color = Color.White;
            
            // Flash red when taking damage
            if (damageInvincibility > 0)
            {
                // Oscillate between red and white during invincibility
                float flashRate = 10f; // Flash speed
                float flashValue = (float)Math.Sin(damageInvincibility * flashRate * Math.PI) * 0.5f + 0.5f;
                color = Color.Lerp(Color.Red, Color.White, flashValue);
            }
            // Bluish when dodging
            else if (IsDodging)
            {
                color = Color.LightBlue;
            }
            // Yellow when damage buff is active
            else if (HasDamageBuff && HasSpeedBuff)
            {
                // Orange when both buffs are active
                color = Color.Orange;
            }
            else if (HasDamageBuff)
            {
                color = Color.Yellow;
            }
            // Green when speed buff is active
            else if (HasSpeedBuff)
            {
                color = Color.LightGreen;
            }
            
            // Draw the player sprite
            spriteBatch.Draw(Sprite, Position, color);
            
            // Projectiles are now drawn by the main game loop
            // foreach (Projectile projectile in _activeProjectiles)
            // {
            //     projectile.Draw(spriteBatch);
            // }
        }

        /// <summary>
        /// Draws the attack hitbox for debugging purposes.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="debugTexture">A solid color texture for drawing the hitbox.</param>
        public void DrawAttackHitbox(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (IsAttacking)
            {
                spriteBatch.Draw(debugTexture, AttackHitbox, Color.Red * 0.5f);
            }
        }

        /// <summary>
        /// Adds a key to the player's inventory.
        /// </summary>
        public void AddKey()
        {
            KeyCount++;
            Console.WriteLine($"Player now has {KeyCount} keys");
        }
        
        /// <summary>
        /// Uses a key if one is available.
        /// </summary>
        /// <returns>True if a key was used; false if no keys are available.</returns>
        public bool UseKey()
        {
            if (KeyCount > 0)
            {
                KeyCount--;
                Console.WriteLine($"Used a key! {KeyCount} keys remaining");
                return true;
            }
            
            Console.WriteLine("No keys available!");
            return false;
        }

        /// <summary>
        /// Restores the player's health by the specified amount, up to the maximum health.
        /// </summary>
        /// <param name="amount">The amount of health to restore.</param>
        public void RestoreHealth(int amount)
        {
            CurrentHealth += amount;
            
            // Clamp health to maximum
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
            
            Console.WriteLine($"Restored {amount} health. Current health: {CurrentHealth}/{MaxHealth}");
        }
    }
    
    /// <summary>
    /// Types of buffs that can be applied to the player.
    /// </summary>
    public enum BuffType
    {
        Damage,
        Speed
    }
} 