using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

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

        /// <summary>
        /// Gets all walkable bounds (rooms and corridors) in the dungeon.
        /// </summary>
        /// <returns>A list of rectangles representing all walkable areas in the dungeon.</returns>
        public List<Rectangle> GetAllWalkableBounds()
        {
            // Create a new list to hold all walkable bounds
            List<Rectangle> walkableBounds = new List<Rectangle>();
            
            // Add all room bounds
            foreach (Room room in Rooms)
            {
                walkableBounds.Add(room.Bounds);
            }
            
            // Add all corridor bounds
            walkableBounds.AddRange(Corridors);
            
            return walkableBounds;
        }
        
        /// <summary>
        /// Checks if a position is within any walkable bounds in the dungeon.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is within walkable bounds; false otherwise.</returns>
        public bool IsPositionValid(Vector2 position)
        {
            // Get all walkable bounds
            List<Rectangle> walkableBounds = GetAllWalkableBounds();
            
            // Create a point from the position
            Point point = new Point((int)position.X, (int)position.Y);
            
            // Check if the point is within any of the walkable bounds
            foreach (Rectangle bounds in walkableBounds)
            {
                if (bounds.Contains(point))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if an entity's movement to a new position is valid based on its bounds.
        /// </summary>
        /// <param name="proposedBounds">The entity's bounds at the proposed position.</param>
        /// <returns>True if the movement is valid; false otherwise.</returns>
        public bool IsMovementValid(Rectangle proposedBounds)
        {
            // Get all walkable bounds
            List<Rectangle> walkableBounds = GetAllWalkableBounds();
            
            // Check each corner of the proposed bounds
            Point topLeft = new Point(proposedBounds.Left, proposedBounds.Top);
            Point topRight = new Point(proposedBounds.Right, proposedBounds.Top);
            Point bottomLeft = new Point(proposedBounds.Left, proposedBounds.Bottom);
            Point bottomRight = new Point(proposedBounds.Right, proposedBounds.Bottom);
            
            // Check if each corner is within ANY walkable area
            bool topLeftValid = false;
            bool topRightValid = false;
            bool bottomLeftValid = false;
            bool bottomRightValid = false;
            
            foreach (Rectangle bounds in walkableBounds)
            {
                if (bounds.Contains(topLeft))
                    topLeftValid = true;
                
                if (bounds.Contains(topRight))
                    topRightValid = true;
                
                if (bounds.Contains(bottomLeft))
                    bottomLeftValid = true;
                
                if (bounds.Contains(bottomRight))
                    bottomRightValid = true;
                
                // If all corners are valid, we can return early
                if (topLeftValid && topRightValid && bottomLeftValid && bottomRightValid)
                    return true;
            }
            
            // Movement is valid only if all corners are within some walkable area
            return topLeftValid && topRightValid && bottomLeftValid && bottomRightValid;
        }
    }
} 