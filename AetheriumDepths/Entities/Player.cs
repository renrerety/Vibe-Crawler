using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
        /// Creates a new player at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the player.</param>
        /// <param name="sprite">The sprite texture for the player.</param>
        public Player(Vector2 position, Texture2D sprite)
        {
            Position = position;
            Sprite = sprite;
            CurrentHealth = MaxHealth; // Initialize current health to max health
            Console.WriteLine($"Player created with {CurrentHealth}/{MaxHealth} health");
        }

        /// <summary>
        /// Updates the player's position based on the movement vector.
        /// </summary>
        /// <param name="movementVector">The direction to move in.</param>
        /// <param name="speed">The speed at which to move.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="viewportBounds">The bounds of the visible game screen.</param>
        public void Move(Vector2 movementVector, float speed, float deltaTime, Rectangle viewportBounds)
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
            
            Vector2 newPosition = Position + (effectiveMovementVector * effectiveSpeed * deltaTime);

            // If movement direction is non-zero, update LastMovementDirection
            if (movementVector != Vector2.Zero)
            {
                LastMovementDirection = Vector2.Normalize(movementVector);
            }

            // Clamp to viewport bounds (accounting for sprite size)
            newPosition.X = MathHelper.Clamp(newPosition.X, 0, viewportBounds.Width - (Sprite?.Width ?? 0));
            newPosition.Y = MathHelper.Clamp(newPosition.Y, 0, viewportBounds.Height - (Sprite?.Height ?? 0));

            // Update position
            Position = newPosition;
        }

        /// <summary>
        /// Updates the player's state, including attack, dodge, and buff timers.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        public void Update(float deltaTime)
        {
            // Update attack timer if an attack is active
            if (IsAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0f)
                {
                    IsAttacking = false;
                }
            }
            
            // Update dodge timer if a dodge is active
            if (IsDodging)
            {
                _dodgeTimer -= deltaTime;
                if (_dodgeTimer <= 0f)
                {
                    IsDodging = false;
                }
            }
            
            // Update damage buff duration if active
            if (HasDamageBuff)
            {
                _damageBuffDuration -= deltaTime;
                if (_damageBuffDuration <= 0f)
                {
                    HasDamageBuff = false;
                    Console.WriteLine("Damage buff has expired");
                }
            }
            
            // Update speed buff duration if active
            if (HasSpeedBuff)
            {
                _speedBuffDuration -= deltaTime;
                if (_speedBuffDuration <= 0f)
                {
                    HasSpeedBuff = false;
                    Console.WriteLine("Speed buff has expired");
                }
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
        /// Draws the player to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="damageInvincibility">Optional timer for damage invincibility.</param>
        public void Draw(SpriteBatch spriteBatch, float damageInvincibility = 0f)
        {
            // Apply appropriate color based on player state
            Color playerColor;
            
            if (IsInvincible)
            {
                // Semi-transparent when invincible (dodge)
                playerColor = new Color(255, 255, 255, 150);
            }
            else if (damageInvincibility > 0f)
            {
                // Flash red when in damage invincibility state
                // Use a pulsing effect based on the timer
                float pulseRate = 10f; // Flashing speed
                float alpha = 0.7f + (float)Math.Sin(damageInvincibility * pulseRate) * 0.3f;
                playerColor = new Color(255, 100, 100, (int)(255 * alpha));
            }
            else if (HasDamageBuff && HasSpeedBuff)
            {
                // Orange tint when both buffs are active
                playerColor = new Color(255, 165, 0);
            }
            else if (HasDamageBuff)
            {
                // Yellow tint when damage buff is active
                playerColor = Color.Yellow;
            }
            else if (HasSpeedBuff)
            {
                // Green tint when speed buff is active
                playerColor = Color.LightGreen;
            }
            else
            {
                // Normal color
                playerColor = Color.White;
            }
            
            spriteBatch.Draw(Sprite, Position, playerColor);
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