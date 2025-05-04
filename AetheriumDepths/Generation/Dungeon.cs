using System.Collections.Generic;

namespace AetheriumDepths.Generation
{
    /// <summary>
    /// Represents a dungeon containing multiple rooms.
    /// </summary>
    public class Dungeon
    {
        /// <summary>
        /// The list of rooms in the dungeon.
        /// </summary>
        public List<Room> Rooms { get; private set; }

        /// <summary>
        /// Creates a new dungeon.
        /// </summary>
        public Dungeon()
        {
            Rooms = new List<Room>();
        }

        /// <summary>
        /// Gets the first room in the dungeon, considered the starting room.
        /// </summary>
        public Room StartingRoom => Rooms.Count > 0 ? Rooms[0] : null;
    }
} 