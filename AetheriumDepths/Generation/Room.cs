using Microsoft.Xna.Framework;
using System;

namespace AetheriumDepths.Generation
{
    /// <summary>
    /// Represents a room in the dungeon.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// The bounds of the room.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// Creates a new room with the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds of the room.</param>
        public Room(Rectangle bounds)
        {
            Bounds = bounds;
        }

        /// <summary>
        /// Gets the center position of the room.
        /// </summary>
        public Vector2 Center => new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
    }
} 