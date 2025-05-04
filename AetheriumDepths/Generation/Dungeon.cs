using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
        /// The list of corridors connecting rooms in the dungeon.
        /// Each corridor is represented as a Rectangle.
        /// </summary>
        public List<Rectangle> Corridors { get; private set; }
        
        /// <summary>
        /// The root BSP node used to generate this dungeon.
        /// </summary>
        public BSPNode RootNode { get; set; }
        
        /// <summary>
        /// The leaf nodes of the BSP tree.
        /// </summary>
        public List<BSPNode> LeafNodes { get; set; }

        /// <summary>
        /// Creates a new dungeon.
        /// </summary>
        public Dungeon()
        {
            Rooms = new List<Room>();
            Corridors = new List<Rectangle>();
            LeafNodes = new List<BSPNode>();
        }

        /// <summary>
        /// Gets the first room in the dungeon, considered the starting room.
        /// </summary>
        public Room StartingRoom => Rooms.Count > 0 ? Rooms[0] : null;
        
        /// <summary>
        /// Adds a corridor connecting two points in the dungeon.
        /// </summary>
        /// <param name="start">The starting point.</param>
        /// <param name="end">The ending point.</param>
        /// <param name="width">The width of the corridor.</param>
        public void AddCorridor(Point start, Point end, int width)
        {
            // Create an L-shaped corridor between the two points
            
            // First segment (horizontal)
            Rectangle horizontalSegment = new Rectangle(
                start.X < end.X ? start.X : end.X,
                start.Y - width / 2,
                System.Math.Abs(end.X - start.X),
                width);
                
            // Second segment (vertical)
            Rectangle verticalSegment = new Rectangle(
                end.X - width / 2,
                start.Y < end.Y ? start.Y : end.Y,
                width,
                System.Math.Abs(end.Y - start.Y));
                
            // Add the corridor segments
            Corridors.Add(horizontalSegment);
            Corridors.Add(verticalSegment);
        }
    }
} 