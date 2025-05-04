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
        public int Health { get; private set; }

        /// <summary>
        /// Flag indicating if this enemy is active in the game.
        /// Inactive enemies are scheduled for removal.
        /// </summary>
        public bool IsActive { get; private set; } = true;
        
        /// <summary>
        /// The range at which the enemy detects and pursues the player.
        /// </summary>
        public float DetectionRange { get; set; } = 200f;
        
        /// <summary>
        /// The range at which the enemy can attack the player.
        /// </summary>
        public float AttackRange { get; set; } = 60f;
        
        /// <summary>
        /// The movement speed of the enemy in pixels per second.
        /// </summary>
        public float MovementSpeed { get; set; } = 100f;
        
        /// <summary>
        /// The direction the enemy is currently facing.
        /// </summary>
        public Vector2 FacingDirection { get; private set; } = new Vector2(0, 1); // Default facing down
        
        /// <summary>
        /// Flag indicating if an attack is currently active.
        /// </summary>
        public bool IsAttacking { get; private set; }
        
        /// <summary>
        /// The hitbox for the current attack.
        /// </summary>
        public Rectangle AttackHitbox { get; private set; }
        
        /// <summary>
        /// Timer for the duration of the attack.
        /// </summary>
        private float _attackTimer;
        
        /// <summary>
        /// Duration of the attack in seconds.
        /// </summary>
        private const float ATTACK_DURATION = 0.3f;
        
        /// <summary>
        /// Cooldown timer between attacks.
        /// </summary>
        private float _attackCooldownTimer;
        
        /// <summary>
        /// Cooldown duration between attacks in seconds.
        /// </summary>
        private const float ATTACK_COOLDOWN = 1.5f;
        
        /// <summary>
        /// The size of the attack hitbox.
        /// </summary>
        private const int ATTACK_SIZE = 32;

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
        /// Updates the enemy state and behavior.
        /// </summary>
        /// <param name="playerPosition">The current position of the player.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        public void Update(Vector2 playerPosition, float deltaTime, Dungeon dungeon)
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
        /// Initiates an attack in the direction of the player.
        /// </summary>
        /// <param name="directionToPlayer">Direction vector pointing to the player.</param>
        private void Attack(Vector2 directionToPlayer)
        {
            // Start attack
            IsAttacking = true;
            _attackTimer = ATTACK_DURATION;
            _attackCooldownTimer = ATTACK_COOLDOWN;
            
            // Normalize direction if it's not already
            if (directionToPlayer.Length() > 0)
            {
                directionToPlayer = Vector2.Normalize(directionToPlayer);
                FacingDirection = directionToPlayer;
            }
            
            // Calculate attack hitbox position based on enemy position and facing direction
            Vector2 hitboxCenter = Position + (FacingDirection * (Sprite.Width + ATTACK_SIZE) / 2);
            
            // Create attack hitbox
            AttackHitbox = new Rectangle(
                (int)(hitboxCenter.X - ATTACK_SIZE / 2),
                (int)(hitboxCenter.Y - ATTACK_SIZE / 2),
                ATTACK_SIZE,
                ATTACK_SIZE);
            
            Console.WriteLine($"Enemy attacked in direction {FacingDirection}");
        }

        /// <summary>
        /// Reduces the enemy's health by the specified amount.
        /// </summary>
        /// <param name="damage">The amount of damage to deal.</param>
        public void TakeDamage(int damage)
        {
            Health -= damage;
            
            // Check if enemy is defeated
            if (Health <= 0)
            {
                IsActive = false;
            }
        }
        
        /// <summary>
        /// Reactivates the enemy with full health.
        /// </summary>
        /// <param name="health">Optional new health value. If not specified, the initial health is restored.</param>
        public void Reactivate(int health = -1)
        {
            // If health parameter is provided, use it; otherwise keep current health
            if (health > 0)
            {
                Health = health;
            }
            
            // Reset attack state
            IsAttacking = false;
            _attackTimer = 0f;
            _attackCooldownTimer = 0f;
            
            // Set active state to true
            IsActive = true;
        }

        /// <summary>
        /// Draws the enemy to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                spriteBatch.Draw(Sprite, Position, Color.White);
            }
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
} 