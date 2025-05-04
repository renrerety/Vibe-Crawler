using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    }
} 