using Microsoft.Xna.Framework;
using System;

namespace AetheriumDepths.Generation
{
    /// <summary>
    /// Defines the different types of rooms in the dungeon.
    /// </summary>
    public enum RoomType
    {
        /// <summary>
        /// Normal room with standard encounters.
        /// </summary>
        Normal,
        
        /// <summary>
        /// Starting room where the player begins.
        /// </summary>
        Start,
        
        /// <summary>
        /// Room containing treasure chests.
        /// </summary>
        Treasure,
        
        /// <summary>
        /// Room containing the Aetherium Weaving altar.
        /// </summary>
        Altar,
        
        /// <summary>
        /// Room containing the boss enemy.
        /// </summary>
        Boss
    }
    
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
        /// The type of room.
        /// </summary>
        public RoomType Type { get; set; } = RoomType.Normal;

        /// <summary>
        /// Creates a new room with the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds of the room.</param>
        public Room(Rectangle bounds)
        {
            Bounds = bounds;
        }
        
        /// <summary>
        /// Creates a new room with the specified bounds and type.
        /// </summary>
        /// <param name="bounds">The bounds of the room.</param>
        /// <param name="type">The type of room.</param>
        public Room(Rectangle bounds, RoomType type)
        {
            Bounds = bounds;
            Type = type;
        }

        /// <summary>
        /// Gets the center position of the room.
        /// </summary>
        public Vector2 Center => new Vector2(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);
    }
} 