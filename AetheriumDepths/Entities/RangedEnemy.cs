using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using AetheriumDepths.Generation;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents a ranged enemy that attacks from a distance by firing projectiles.
    /// </summary>
    public class RangedEnemy : Enemy
    {
        /// <summary>
        /// The ideal distance the ranged enemy tries to maintain from the player.
        /// </summary>
        public float OptimalRange { get; set; } = 150f;
        
        /// <summary>
        /// Threshold distance to determine if the enemy is too close to the player.
        /// </summary>
        public float RetreatingThreshold { get; set; } = 120f;
        
        /// <summary>
        /// List of active projectiles fired by this enemy.
        /// </summary>
        private List<Projectile> _activeProjectiles = new List<Projectile>();
        
        /// <summary>
        /// The texture used for projectiles fired by this enemy.
        /// </summary>
        private Texture2D _projectileTexture;
        
        /// <summary>
        /// Creates a new ranged enemy at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the enemy.</param>
        /// <param name="sprite">The sprite texture for the enemy.</param>
        /// <param name="projectileTexture">The texture for projectiles fired by this enemy.</param>
        /// <param name="health">The starting health of the enemy.</param>
        public RangedEnemy(Vector2 position, Texture2D sprite, Texture2D projectileTexture, int health) 
            : base(position, sprite, health)
        {
            _projectileTexture = projectileTexture;
            
            // Ranged enemies have a larger detection range
            DetectionRange = 300f;
            
            // Set attack range to match the detection range since we use projectiles
            AttackRange = 250f;
            
            // Ranged enemies are slower than basic melee enemies
            MovementSpeed = 80f;
        }
        
        /// <summary>
        /// Updates the enemy state and behavior.
        /// </summary>
        /// <param name="playerPosition">The current position of the player.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        public override void Update(Vector2 playerPosition, float deltaTime, Dungeon dungeon)
        {
            if (!IsActive) return;
            
            // Calculate distance to player
            Vector2 directionToPlayer = playerPosition - Position;
            float distanceToPlayer = directionToPlayer.Length();
            
            // Keep facing the player regardless of movement
            if (distanceToPlayer > 0)
            {
                FacingDirection = Vector2.Normalize(directionToPlayer);
            }
            
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
            
            // Update projectiles
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                _activeProjectiles[i].Update(deltaTime, dungeon);
                
                // Remove inactive projectiles
                if (!_activeProjectiles[i].IsActive)
                {
                    _activeProjectiles.RemoveAt(i);
                }
            }
            
            // If player is within attack range and cooldown is ready, fire a projectile
            if (distanceToPlayer <= AttackRange && _attackCooldownTimer <= 0f)
            {
                FireProjectile(playerPosition);
            }
            
            // If player is detected but outside optimal range, move towards them
            if (distanceToPlayer <= DetectionRange && distanceToPlayer > OptimalRange)
            {
                MoveTowards(playerPosition, deltaTime, dungeon);
            }
            // If player is too close, move away
            else if (distanceToPlayer < RetreatingThreshold)
            {
                MoveAway(playerPosition, deltaTime, dungeon);
            }
            // Otherwise, if within optimal range, don't move (just keep firing)
        }
        
        /// <summary>
        /// Fires a projectile at the target position.
        /// </summary>
        /// <param name="targetPosition">The position to aim at.</param>
        private void FireProjectile(Vector2 targetPosition)
        {
            if (_projectileTexture == null) return;
            
            // Start attack animation/state
            IsAttacking = true;
            _attackTimer = ATTACK_DURATION;
            _attackCooldownTimer = ATTACK_COOLDOWN * 1.5f; // Ranged enemies fire slower
            
            // Calculate direction to target
            Vector2 direction = targetPosition - Position;
            if (direction != Vector2.Zero)
            {
                direction = Vector2.Normalize(direction);
            }
            
            // Create projectile at enemy's position offset toward the target
            Vector2 projectileStart = Position + (direction * Sprite.Width / 2);
            Projectile projectile = new Projectile(
                projectileStart, 
                direction, 
                _projectileTexture, 
                1, // Projectile damage
                300f); // Projectile speed
                
            // Add to active projectiles
            _activeProjectiles.Add(projectile);
            
            Console.WriteLine("Ranged enemy fired a projectile");
        }
        
        /// <summary>
        /// Move towards the target position.
        /// </summary>
        /// <param name="targetPosition">The position to move towards.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        private void MoveTowards(Vector2 targetPosition, float deltaTime, Dungeon dungeon)
        {
            // Calculate direction to target
            Vector2 direction = targetPosition - Position;
            if (direction.Length() > 0)
            {
                direction = Vector2.Normalize(direction);
            }
            
            // Calculate proposed new position
            Vector2 proposedPosition = Position + direction * MovementSpeed * deltaTime;
            
            // Use the same wall sliding logic as in base Enemy
            TryMove(proposedPosition, direction, deltaTime, dungeon);
        }
        
        /// <summary>
        /// Move away from the target position.
        /// </summary>
        /// <param name="targetPosition">The position to move away from.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        private void MoveAway(Vector2 targetPosition, float deltaTime, Dungeon dungeon)
        {
            // Calculate direction away from target
            Vector2 direction = Position - targetPosition;
            if (direction.Length() > 0)
            {
                direction = Vector2.Normalize(direction);
            }
            
            // Calculate proposed new position
            Vector2 proposedPosition = Position + direction * MovementSpeed * deltaTime;
            
            // Use the same wall sliding logic
            TryMove(proposedPosition, direction, deltaTime, dungeon);
        }
        
        /// <summary>
        /// Try to move to a proposed position, with wall sliding if blocked.
        /// </summary>
        /// <param name="proposedPosition">The proposed new position.</param>
        /// <param name="direction">The movement direction.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        private void TryMove(Vector2 proposedPosition, Vector2 direction, float deltaTime, Dungeon dungeon)
        {
            // Calculate bounds at the proposed position
            Rectangle proposedBounds = new Rectangle(
                (int)proposedPosition.X,
                (int)proposedPosition.Y,
                Sprite.Width,
                Sprite.Height);
            
            // Only move if the proposed movement is valid
            if (dungeon.IsMovementValid(proposedBounds))
            {
                Position = proposedPosition;
            }
            else
            {
                // Try moving only in X direction
                Vector2 proposedXPosition = new Vector2(
                    Position.X + direction.X * MovementSpeed * deltaTime,
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
                    Position.Y + direction.Y * MovementSpeed * deltaTime);
                
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
        
        /// <summary>
        /// Draws the enemy and its projectiles.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            // Draw active projectiles
            foreach (Projectile projectile in _activeProjectiles)
            {
                projectile.Draw(spriteBatch);
            }
        }
        
        /// <summary>
        /// Gets all active projectiles fired by this enemy.
        /// </summary>
        /// <returns>A list of active projectiles.</returns>
        public List<Projectile> GetActiveProjectiles()
        {
            return _activeProjectiles;
        }
    }
} 