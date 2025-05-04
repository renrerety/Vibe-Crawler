using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AetheriumDepths.Gameplay.Interactables
{
    /// <summary>
    /// Represents a door that can be locked and requires a key to unlock.
    /// </summary>
    public class Door
    {
        /// <summary>
        /// Current position of the door in the game world.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The texture used to render the door when locked.
        /// </summary>
        public Texture2D SpriteLocked { get; set; }
        
        /// <summary>
        /// The texture used to render the door when unlocked.
        /// </summary>
        public Texture2D SpriteUnlocked { get; set; }
        
        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            SpriteLocked?.Width ?? 0, 
            SpriteLocked?.Height ?? 0);
            
        /// <summary>
        /// Flag indicating if the door is currently locked.
        /// </summary>
        public bool IsLocked { get; private set; }
        
        /// <summary>
        /// Creates a new door at the specified position.
        /// </summary>
        /// <param name="position">The position of the door in the game world.</param>
        /// <param name="spriteLocked">The sprite texture for the locked door.</param>
        /// <param name="spriteUnlocked">The sprite texture for the unlocked door.</param>
        /// <param name="isLocked">Whether the door starts locked (default: true).</param>
        public Door(Vector2 position, Texture2D spriteLocked, Texture2D spriteUnlocked, bool isLocked = true)
        {
            Position = position;
            SpriteLocked = spriteLocked;
            SpriteUnlocked = spriteUnlocked;
            IsLocked = isLocked;
        }
        
        /// <summary>
        /// Unlocks the door if it's currently locked.
        /// </summary>
        /// <returns>True if the door was unlocked by this call; false if it was already unlocked.</returns>
        public bool Unlock()
        {
            if (IsLocked)
            {
                IsLocked = false;
                Console.WriteLine("Door unlocked!");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Draws the door with the appropriate sprite based on its state.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used for rendering.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Select the appropriate sprite based on the door's state
            Texture2D currentSprite = IsLocked ? SpriteLocked : SpriteUnlocked;
            
            // Draw the door
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