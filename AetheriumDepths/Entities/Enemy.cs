using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AetheriumDepths.Generation;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents an enemy in the game.
    /// </summary>
    public class Enemy
    {
        /// <summary>
        /// Current position of the enemy in the game world.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The texture used to render the enemy.
        /// </summary>
        public Texture2D Sprite { get; set; }

        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            Sprite?.Width ?? 0,
            Sprite?.Height ?? 0);

        /// <summary>
        /// Current health of the enemy.
        /// </summary>
        public int Health { get; protected set; }

        /// <summary>
        /// Flag indicating if this enemy is active in the game.
        /// Inactive enemies are scheduled for removal.
        /// </summary>
        public bool IsActive { get; protected set; } = true;
        
        /// <summary>
        /// The range at which the enemy detects and pursues the player.
        /// </summary>
        public virtual float DetectionRange { get; set; } = 200f;
        
        /// <summary>
        /// The range at which the enemy can attack the player.
        /// </summary>
        public virtual float AttackRange { get; set; } = 60f;
        
        /// <summary>
        /// The movement speed of the enemy in pixels per second.
        /// </summary>
        public virtual float MovementSpeed { get; set; } = 100f;
        
        /// <summary>
        /// The cooldown duration between attacks in seconds.
        /// </summary>
        public virtual float AttackCooldown => ATTACK_COOLDOWN;
        
        /// <summary>
        /// The duration of an attack in seconds.
        /// </summary>
        public virtual float AttackDuration => ATTACK_DURATION;
        
        /// <summary>
        /// The direction the enemy is currently facing.
        /// </summary>
        public Vector2 FacingDirection { get; protected set; } = new Vector2(0, 1); // Default facing down
        
        /// <summary>
        /// Flag indicating if an attack is currently active.
        /// </summary>
        public bool IsAttacking { get; protected set; }
        
        /// <summary>
        /// The hitbox for the current attack.
        /// </summary>
        public Rectangle AttackHitbox { get; protected set; }
        
        /// <summary>
        /// Timer for the duration of the attack.
        /// </summary>
        protected float _attackTimer;
        
        /// <summary>
        /// Duration of the attack in seconds.
        /// </summary>
        protected const float ATTACK_DURATION = 0.3f;
        
        /// <summary>
        /// Cooldown timer between attacks.
        /// </summary>
        protected float _attackCooldownTimer;
        
        /// <summary>
        /// Cooldown duration between attacks in seconds.
        /// </summary>
        protected const float ATTACK_COOLDOWN = 1.5f;
        
        /// <summary>
        /// The size of the attack hitbox.
        /// </summary>
        protected const int ATTACK_SIZE = 32;

        /// <summary>
        /// Creates a new enemy at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the enemy.</param>
        /// <param name="sprite">The sprite texture for the enemy.</param>
        /// <param name="health">The starting health of the enemy.</param>
        public Enemy(Vector2 position, Texture2D sprite, int health)
        {
            Position = position;
            Sprite = sprite;
            Health = health;
            
            // Initialize attack cooldown as ready to attack
            _attackCooldownTimer = 0f;
        }
        
        /// <summary>
        /// Updates the bounds of the enemy.
        /// </summary>
        protected virtual void UpdateBounds()
        {
            // Base implementation does nothing as Bounds is calculated on-demand
            // but derived classes can override this
        }

        /// <summary>
        /// Updates the enemy state and behavior.
        /// </summary>
        /// <param name="playerPosition">The current position of the player.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        public virtual void Update(Vector2 playerPosition, float deltaTime, Dungeon dungeon)
        {
            if (!IsActive) return;
            
            // Update attack timer if an attack is active
            if (IsAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0f)
                {
                    IsAttacking = false;
                }
            }
            
            // Update attack cooldown timer
            if (_attackCooldownTimer > 0f)
            {
                _attackCooldownTimer -= deltaTime;
            }
            
            // Calculate distance to player
            Vector2 directionToPlayer = playerPosition - Position;
            float distanceToPlayer = directionToPlayer.Length();
            
            // If player is within attack range and cooldown is ready, attack
            if (distanceToPlayer <= AttackRange && _attackCooldownTimer <= 0f && !IsAttacking)
            {
                Attack(directionToPlayer);
            }
            // Otherwise, if player is within detection range but outside attack range, move towards them
            else if (distanceToPlayer <= DetectionRange && distanceToPlayer > AttackRange && !IsAttacking)
            {
                // Normalize direction vector
                if (distanceToPlayer > 0)
                {
                    directionToPlayer = Vector2.Normalize(directionToPlayer);
                    FacingDirection = directionToPlayer;
                }
                
                // Calculate proposed new position
                Vector2 proposedPosition = Position + directionToPlayer * MovementSpeed * deltaTime;
                
                // Calculate bounds at the proposed position
                Rectangle proposedBounds = new Rectangle(
                    (int)proposedPosition.X,
                    (int)proposedPosition.Y,
                    Sprite.Width,
                    Sprite.Height);
                
                // Only move if the proposed movement is valid
                if (dungeon.IsMovementValid(proposedBounds))
                {
                    // Move towards player
                    Position = proposedPosition;
                }
                else
                {
                    // If the movement is invalid, try to slide along walls
                    
                    // Try moving only in X direction
                    Vector2 proposedXPosition = new Vector2(
                        Position.X + directionToPlayer.X * MovementSpeed * deltaTime,
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
                        Position.Y + directionToPlayer.Y * MovementSpeed * deltaTime);
                    
                    Rectangle proposedYBounds = new Rectangle(
                        (int)proposedYPosition.X,
                        (int)proposedYPosition.Y,
                        Sprite.Width,
                        Sprite.Height);
                    
                    if (dungeon.IsMovementValid(proposedYBounds))
                    {
                        Position = proposedYPosition;
                    }
                }
            }
        }
        
        /// <summary>
        /// Calculates the attack hitbox based on position and facing direction.
        /// </summary>
        /// <returns>A rectangle representing the attack hitbox.</returns>
        protected virtual Rectangle CalculateAttackHitbox()
        {
            // Calculate attack hitbox position based on enemy position and facing direction
            Vector2 hitboxCenter = Position + (FacingDirection * (Sprite.Width + ATTACK_SIZE) / 2);
            
            // Create and return attack hitbox
            return new Rectangle(
                (int)(hitboxCenter.X - ATTACK_SIZE / 2),
                (int)(hitboxCenter.Y - ATTACK_SIZE / 2),
                ATTACK_SIZE,
                ATTACK_SIZE);
        }
        
        /// <summary>
        /// Initiates an attack in the direction of the player.
        /// </summary>
        /// <param name="directionToPlayer">Direction vector pointing to the player.</param>
        protected virtual void Attack(Vector2 directionToPlayer)
        {
            // Start attack
            IsAttacking = true;
            _attackTimer = AttackDuration;
            _attackCooldownTimer = AttackCooldown;
            
            // Normalize direction if it's not already
            if (directionToPlayer.Length() > 0)
            {
                directionToPlayer = Vector2.Normalize(directionToPlayer);
                FacingDirection = directionToPlayer;
            }
            
            // Set the attack hitbox
            AttackHitbox = CalculateAttackHitbox();
            
            Console.WriteLine($"Enemy attacked in direction {FacingDirection}");
        }

        /// <summary>
        /// Reduces the enemy's health by the specified amount.
        /// </summary>
        /// <param name="damage">The amount of damage to deal.</param>
        public void TakeDamage(int damage)
        {
            if (!IsActive) return;
            
            Health -= damage;
            Console.WriteLine($"Enemy took {damage} damage. Health: {Health}");
            
            // Check if enemy is defeated
            if (Health <= 0)
            {
                IsActive = false;
                Console.WriteLine("Enemy defeated!");
            }
        }

        /// <summary>
        /// Reactivates the enemy with optionally specified health.
        /// </summary>
        /// <param name="health">The new health value. If -1, uses the current health.</param>
        public virtual void Reactivate(int health = -1)
        {
            if (health >= 0)
            {
                Health = health;
            }
            
            IsActive = true;
            IsAttacking = false;
            _attackTimer = 0f;
            _attackCooldownTimer = 0f;
            
            Console.WriteLine($"Enemy reactivated with {Health} health.");
        }

        /// <summary>
        /// Draws the enemy to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || Sprite == null) return;
            
            spriteBatch.Draw(Sprite, Position, Color.White);
        }
        
        /// <summary>
        /// Draws the attack hitbox for debugging.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void DrawAttackHitbox(SpriteBatch spriteBatch)
        {
            if (!IsAttacking || _debugTexture == null) return;
            
            spriteBatch.Draw(_debugTexture, AttackHitbox, Color.Red * 0.5f);
        }
        
        // Field for debug texture
        private static Texture2D _debugTexture;
        
        /// <summary>
        /// Sets the debug texture used for drawing attack hitboxes.
        /// </summary>
        /// <param name="debugTexture">The debug texture to use.</param>
        public static void SetDebugTexture(Texture2D debugTexture)
        {
            _debugTexture = debugTexture;
        }
    }
} 