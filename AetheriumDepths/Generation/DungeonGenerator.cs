using Microsoft.Xna.Framework;
using System;

namespace AetheriumDepths.Generation
{
    /// <summary>
    /// Generates procedural dungeons.
    /// </summary>
    public class DungeonGenerator
    {
        /// <summary>
        /// Generates a simple dungeon with hardcoded room positions.
        /// This is a placeholder for future procedural generation.
        /// </summary>
        /// <param name="viewportBounds">The bounds of the viewport to ensure rooms are visible.</param>
        /// <returns>A dungeon containing rooms.</returns>
        public Dungeon GenerateSimpleDungeon(Rectangle viewportBounds)
        {
            Dungeon dungeon = new Dungeon();
            
            // Add a few hardcoded rooms
            // Room 1 (Starting Room) - centered in the upper left portion
            dungeon.Rooms.Add(new Room(new Rectangle(100, 100, 200, 150)));
            
            // Room 2 - to the right of Room 1
            dungeon.Rooms.Add(new Room(new Rectangle(400, 100, 150, 200)));
            
            // Room 3 - below and between Rooms 1 and 2
            dungeon.Rooms.Add(new Room(new Rectangle(250, 350, 180, 180)));
            
            // Room 4 - small room connected to Room 3
            dungeon.Rooms.Add(new Room(new Rectangle(450, 400, 100, 100)));
            
            // Validate that rooms are within viewport bounds
            for (int i = 0; i < dungeon.Rooms.Count; i++)
            {
                Rectangle roomBounds = dungeon.Rooms[i].Bounds;
                
                // Ensure room is within viewport (with a small margin)
                int margin = 20;
                roomBounds.X = Math.Max(margin, Math.Min(viewportBounds.Width - roomBounds.Width - margin, roomBounds.X));
                roomBounds.Y = Math.Max(margin, Math.Min(viewportBounds.Height - roomBounds.Height - margin, roomBounds.Y));
                
                // Update room with adjusted bounds if needed
                dungeon.Rooms[i] = new Room(roomBounds);
            }
            
            return dungeon;
        }
    }
} 