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
            (int)(Position.X - (SpriteLocked?.Width ?? 0) / 2), 
            (int)(Position.Y - (SpriteLocked?.Height ?? 0) / 2), 
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
        /// Locks the door if it's currently unlocked.
        /// </summary>
        /// <returns>True if the door was locked by this call; false if it was already locked.</returns>
        public bool Lock()
        {
            if (!IsLocked)
            {
                IsLocked = true;
                Console.WriteLine("Door locked!");
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
                float scale = 1.2f; // Make the door slightly larger for visibility
                Color tint = IsLocked ? Color.Red : Color.Green; // Red for locked, green for unlocked
                
                spriteBatch.Draw(
                    currentSprite,
                    Position,
                    null,
                    tint,
                    0f,
                    new Vector2(currentSprite.Width / 2, currentSprite.Height / 2), // Center the door at its position
                    scale,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
} 