using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AetheriumDepths.Gameplay
{
    /// <summary>
    /// Type of loot item.
    /// </summary>
    public enum LootType
    {
        /// <summary>
        /// Health potion that restores player health when collected.
        /// </summary>
        HealthPotion,
        
        /// <summary>
        /// Key that can be used to unlock doors.
        /// </summary>
        Key
    }
    
    /// <summary>
    /// Represents a collectible loot item in the game.
    /// </summary>
    public class LootItem
    {
        /// <summary>
        /// Current position of the loot item in the game world.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The texture used to render the loot item.
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
        /// The type of loot item.
        /// </summary>
        public LootType Type { get; private set; }
        
        /// <summary>
        /// Flag indicating if this loot item is active in the game.
        /// Inactive items are scheduled for removal.
        /// </summary>
        public bool IsActive { get; private set; } = true;
        
        /// <summary>
        /// The amount of health restored by a health potion.
        /// </summary>
        public static readonly int HealthPotionAmount = 20;
        
        /// <summary>
        /// Creates a new loot item at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the loot item.</param>
        /// <param name="sprite">The sprite texture for the loot item.</param>
        /// <param name="type">The type of loot item.</param>
        public LootItem(Vector2 position, Texture2D sprite, LootType type)
        {
            Position = position;
            Sprite = sprite;
            Type = type;
        }
        
        /// <summary>
        /// Marks the loot item as collected and inactive.
        /// </summary>
        public void Collect()
        {
            if (IsActive)
            {
                IsActive = false;
            }
        }
        
        /// <summary>
        /// Draws the loot item to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || Sprite == null) return;
            
            // Render the loot item with a slight hover effect based on time
            float hoverOffset = (float)System.Math.Sin(System.Environment.TickCount / 300.0f) * 3.0f;
            Vector2 renderPosition = new Vector2(Position.X, Position.Y + hoverOffset);
            
            // Add a slight rotation for visual appeal
            float rotation = (float)System.Math.Sin(System.Environment.TickCount / 500.0f) * 0.1f;
            
            // Draw with hover effect and slight rotation
            spriteBatch.Draw(
                Sprite, 
                renderPosition, 
                null, 
                Color.White, 
                rotation, 
                new Vector2(Sprite.Width / 2, Sprite.Height / 2), 
                1.0f, 
                SpriteEffects.None, 
                0);
        }
    }
} 