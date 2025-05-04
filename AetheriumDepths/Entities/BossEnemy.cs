using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents a boss enemy in the game, which has more health, is larger, and does more damage.
    /// </summary>
    public class BossEnemy : Enemy
    {
        /// <summary>
        /// The multiplier for boss damage compared to regular enemies.
        /// </summary>
        public const float DamageMultiplier = 2.0f;
        
        /// <summary>
        /// The multiplier for boss size compared to regular enemies.
        /// </summary>
        public const float SizeMultiplier = 1.5f;
        
        /// <summary>
        /// The movement speed of the boss.
        /// </summary>
        public override float MovementSpeed => 90f; // Slightly slower than normal enemies
        
        /// <summary>
        /// The detection range for the boss.
        /// </summary>
        public override float DetectionRange => 400f; // Larger detection range
        
        /// <summary>
        /// The attack range for the boss.
        /// </summary>
        public override float AttackRange => 100f; // Larger attack range
        
        /// <summary>
        /// The cooldown between boss attacks.
        /// </summary>
        public override float AttackCooldown => 2.0f; // Longer attack cooldown for balance
        
        /// <summary>
        /// The duration of boss attacks.
        /// </summary>
        public override float AttackDuration => 0.8f; // Longer attack duration
        
        /// <summary>
        /// The alpha/transparency value for rendering.
        /// </summary>
        public float Alpha => 1.0f;
        
        /// <summary>
        /// The rotation angle for rendering.
        /// </summary>
        public float Rotation => 0.0f;
        
        /// <summary>
        /// Creates a new boss enemy at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the boss.</param>
        /// <param name="sprite">The sprite texture for the boss.</param>
        /// <param name="health">The initial health of the boss.</param>
        public BossEnemy(Vector2 position, Texture2D sprite, int health) 
            : base(position, sprite, health)
        {
            // Adjust bounds to reflect larger size
            UpdateBounds();
        }
        
        /// <summary>
        /// Updates the bounds of the boss to reflect its larger size.
        /// </summary>
        protected override void UpdateBounds()
        {
            if (Sprite != null)
            {
                // Create larger bounds for the boss
                int width = (int)(Sprite.Width * SizeMultiplier);
                int height = (int)(Sprite.Height * SizeMultiplier);
                
                // Note: Since we can't directly override the Bounds property (it's read-only),
                // we adjust the way we render the boss instead
            }
        }
        
        /// <summary>
        /// Draws the boss enemy to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive && Sprite != null)
            {
                // Draw with a reddish tint to indicate boss status
                Color tint = new Color(1.0f, 0.5f, 0.5f) * Alpha;
                
                // Draw larger than normal enemies
                spriteBatch.Draw(
                    Sprite,
                    Position,
                    null,
                    tint,
                    Rotation,
                    new Vector2(Sprite.Width / 2, Sprite.Height / 2),
                    SizeMultiplier,
                    SpriteEffects.None,
                    0);
                
                // Draw attack hitbox for debugging if active
                DrawAttackHitbox(spriteBatch);
            }
        }
        
        /// <summary>
        /// Calculates the attack hitbox for the boss enemy.
        /// </summary>
        /// <returns>A rectangle representing the attack hitbox.</returns>
        protected override Rectangle CalculateAttackHitbox()
        {
            // Create a larger attack hitbox than normal enemies
            int hitboxSize = (int)(50 * SizeMultiplier);
            
            // Calculate hitbox position based on facing direction
            Vector2 directionVector;
            
            // Convert FacingDirection vector to a cardinal direction for simpler calculations
            if (Math.Abs(FacingDirection.X) > Math.Abs(FacingDirection.Y))
            {
                // Facing left or right
                directionVector = new Vector2(Math.Sign(FacingDirection.X), 0);
            }
            else
            {
                // Facing up or down
                directionVector = new Vector2(0, Math.Sign(FacingDirection.Y));
            }
            
            // Calculate hitbox position
            if (directionVector.Y < 0)
            {
                // Facing up
                return new Rectangle(
                    (int)(Position.X - hitboxSize / 2),
                    (int)(Position.Y - hitboxSize - Bounds.Height / 2),
                    hitboxSize,
                    hitboxSize);
            }
            else if (directionVector.Y > 0)
            {
                // Facing down
                return new Rectangle(
                    (int)(Position.X - hitboxSize / 2),
                    (int)(Position.Y + Bounds.Height / 2),
                    hitboxSize,
                    hitboxSize);
            }
            else if (directionVector.X < 0)
            {
                // Facing left
                return new Rectangle(
                    (int)(Position.X - hitboxSize - Bounds.Width / 2),
                    (int)(Position.Y - hitboxSize / 2),
                    hitboxSize,
                    hitboxSize);
            }
            else if (directionVector.X > 0)
            {
                // Facing right
                return new Rectangle(
                    (int)(Position.X + Bounds.Width / 2),
                    (int)(Position.Y - hitboxSize / 2),
                    hitboxSize,
                    hitboxSize);
            }
            
            // Fallback (should not reach here)
            return new Rectangle(0, 0, 0, 0);
        }
    }
} 