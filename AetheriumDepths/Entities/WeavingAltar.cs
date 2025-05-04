using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents a weaving altar where the player can spend Aetherium Essence to gain buffs.
    /// </summary>
    public class WeavingAltar
    {
        /// <summary>
        /// Current position of the altar in the game world.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The texture used to render the altar.
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
        /// Creates a new weaving altar at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the altar.</param>
        /// <param name="sprite">The sprite texture for the altar.</param>
        public WeavingAltar(Vector2 position, Texture2D sprite)
        {
            Position = position;
            Sprite = sprite;
        }

        /// <summary>
        /// Draws the altar to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the altar
            spriteBatch.Draw(Sprite, Position, Color.White);
        }
    }
} 