using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AetheriumDepths.Gameplay.Interactables
{
    /// <summary>
    /// Represents a treasure chest that can be opened to contain loot.
    /// </summary>
    public class TreasureChest
    {
        /// <summary>
        /// Current position of the chest in the game world.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The texture used to render the chest when closed.
        /// </summary>
        public Texture2D SpriteClosed { get; set; }
        
        /// <summary>
        /// The texture used to render the chest when open.
        /// </summary>
        public Texture2D SpriteOpen { get; set; }
        
        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            SpriteClosed?.Width ?? 0, 
            SpriteClosed?.Height ?? 0);
            
        /// <summary>
        /// Flag indicating if the chest has been opened.
        /// </summary>
        public bool IsOpen { get; private set; }
        
        /// <summary>
        /// Flag to prevent multiple loot drops when opened.
        /// </summary>
        private bool _hasDroppedLoot = false;
        
        /// <summary>
        /// Creates a new treasure chest at the specified position.
        /// </summary>
        /// <param name="position">The position of the chest in the game world.</param>
        /// <param name="spriteClosed">The sprite texture for the closed chest.</param>
        /// <param name="spriteOpen">The sprite texture for the open chest.</param>
        public TreasureChest(Vector2 position, Texture2D spriteClosed, Texture2D spriteOpen)
        {
            Position = position;
            SpriteClosed = spriteClosed;
            SpriteOpen = spriteOpen;
            IsOpen = false;
        }
        
        /// <summary>
        /// Opens the chest if it's currently closed.
        /// </summary>
        /// <returns>True if the chest was opened by this call; false if it was already open.</returns>
        public bool Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                Console.WriteLine("Treasure chest opened!");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Returns whether the chest should drop loot when opened.
        /// </summary>
        /// <returns>True if the chest should drop loot and hasn't already; false otherwise.</returns>
        public bool ShouldDropLoot()
        {
            if (IsOpen && !_hasDroppedLoot)
            {
                _hasDroppedLoot = true;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Draws the chest with the appropriate sprite based on its state.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used for rendering.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Select the appropriate sprite based on the chest's state
            Texture2D currentSprite = IsOpen ? SpriteOpen : SpriteClosed;
            
            // Draw the chest
            if (currentSprite != null)
            {
                spriteBatch.Draw(
                    currentSprite,
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