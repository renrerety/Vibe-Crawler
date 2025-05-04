using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AetheriumDepths.Gameplay.Interactables
{
    /// <summary>
    /// Represents a destructible crate that can be damaged and potentially drop items.
    /// </summary>
    public class DestructibleCrate
    {
        /// <summary>
        /// Current position of the crate in the game world.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The texture used to render the crate.
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
        /// Current health of the crate.
        /// </summary>
        public int CurrentHealth { get; private set; }
        
        /// <summary>
        /// Maximum health of the crate.
        /// </summary>
        public int MaxHealth { get; private set; }
        
        /// <summary>
        /// Flag indicating if the crate has been destroyed.
        /// </summary>
        public bool IsDestroyed { get; private set; }
        
        /// <summary>
        /// Flag to prevent multiple loot drops when destroyed.
        /// </summary>
        private bool _hasDroppedLoot = false;
        
        /// <summary>
        /// Creates a new destructible crate at the specified position.
        /// </summary>
        /// <param name="position">The position of the crate in the game world.</param>
        /// <param name="sprite">The sprite texture for the crate.</param>
        /// <param name="health">The crate's health (default: 1).</param>
        public DestructibleCrate(Vector2 position, Texture2D sprite, int health = 1)
        {
            Position = position;
            Sprite = sprite;
            MaxHealth = health;
            CurrentHealth = health;
            IsDestroyed = false;
        }
        
        /// <summary>
        /// Reduces the crate's health by the specified amount.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        /// <returns>True if the crate was destroyed by this damage; false otherwise.</returns>
        public bool TakeDamage(int damage)
        {
            if (IsDestroyed)
            {
                Console.WriteLine("Crate is already destroyed!");
                return false;
            }
            
            // Take damage
            CurrentHealth -= damage;
            Console.WriteLine($"Crate took {damage} damage! Health before: {CurrentHealth + damage}, after: {CurrentHealth}/{MaxHealth}");
            
            // Check if the crate has been destroyed
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                IsDestroyed = true;
                Console.WriteLine("Crate is now destroyed!");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Returns whether the crate should drop loot when destroyed.
        /// </summary>
        /// <returns>True if the crate should drop loot and hasn't already; false otherwise.</returns>
        public bool ShouldDropLoot()
        {
            if (IsDestroyed && !_hasDroppedLoot)
            {
                _hasDroppedLoot = true;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Draws the crate if it's not destroyed.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used for rendering.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsDestroyed && Sprite != null)
            {
                spriteBatch.Draw(
                    Sprite,
                    Position,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
} 