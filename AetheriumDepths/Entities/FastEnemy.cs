using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents a fast-moving enemy with low health.
    /// </summary>
    public class FastEnemy : Enemy
    {
        /// <summary>
        /// Creates a new fast enemy at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the enemy.</param>
        /// <param name="sprite">The sprite texture for the enemy.</param>
        /// <param name="health">The starting health of the enemy.</param>
        public FastEnemy(Vector2 position, Texture2D sprite, int health) 
            : base(position, sprite, health)
        {
            // Fast enemies move significantly faster than base enemies
            MovementSpeed = 180f;
            
            // Fast enemies have a smaller detection range - they rely on speed when they see the player
            DetectionRange = 250f;
            
            // Attack range is slightly lower
            AttackRange = 45f;
        }

        /// <summary>
        /// Draws the fast enemy with a distinctive color tint.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || Sprite == null) return;
            
            // Draw with a greenish tint to distinguish from regular enemies
            spriteBatch.Draw(
                Sprite,
                Position,
                null,
                new Color(150, 255, 150), // Light green tint
                0f,
                Vector2.Zero,
                1.0f,
                SpriteEffects.None,
                0f);
            
            // If debugging, draw attack hitbox
            if (IsAttacking)
            {
                // This is handled by the main game class via DrawAttackHitbox
            }
        }
    }
} 