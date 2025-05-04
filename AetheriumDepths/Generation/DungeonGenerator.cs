using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AetheriumDepths.Generation
{
    /// <summary>
    /// Generates procedural dungeons.
    /// </summary>
    public class DungeonGenerator
    {
        // BSP generation constants
        private const int MIN_LEAF_SIZE = 800; // Increased to support much larger rooms
        private const int MIN_ROOM_SIZE = 720;  // Quadrupled from 180
        private const float ROOM_SIZE_FACTOR = 0.85f; // Maintained for consistency
        private const int CORRIDOR_WIDTH = 100; // Widened corridors to match larger rooms
        private const int MAX_ITERATIONS = 10;  // Increased to generate more rooms (10-15)
        
        // Random number generator
        private Random _random;
        
        /// <summary>
        /// Creates a new dungeon generator.
        /// </summary>
        public DungeonGenerator()
        {
            _random = new Random();
        }
        
        /// <summary>
        /// Generates a procedural dungeon using Binary Space Partitioning.
        /// </summary>
        /// <param name="viewportBounds">The bounds of the viewport to ensure rooms are visible.</param>
        /// <returns>A dungeon containing rooms and corridors.</returns>
        public Dungeon GenerateBSPDungeon(Rectangle viewportBounds)
        {
            Dungeon dungeon = new Dungeon();
            
            // Calculate dungeon area with increased margins for larger rooms
            int margin = 80; // Increased from 40
            
            // Create a dungeon area larger than the viewport to support more rooms
            int dungeonWidth = viewportBounds.Width * 3; // Triple width
            int dungeonHeight = viewportBounds.Height * 3; // Triple height
            
            Rectangle dungeonArea = new Rectangle(
                -dungeonWidth/2 + viewportBounds.Width/2, // Center the dungeon
                -dungeonHeight/2 + viewportBounds.Height/2, // Center the dungeon
                dungeonWidth, 
                dungeonHeight);
            
            // Create the root node of the BSP tree
            BSPNode rootNode = new BSPNode(dungeonArea);
            dungeon.RootNode = rootNode;
            
            // Recursively split the space
            SplitBSPNode(rootNode, 0);
            
            // Collect all the leaf nodes
            List<BSPNode> leafNodes = new List<BSPNode>();
            CollectLeafNodes(rootNode, leafNodes);
            dungeon.LeafNodes = leafNodes;
            
            // Create rooms in the leaf nodes
            foreach (BSPNode leaf in leafNodes)
            {
                CreateRoomInNode(leaf);
                if (leaf.Room != null)
                {
                    dungeon.Rooms.Add(leaf.Room);
                }
            }
            
            // Create corridors between rooms
            CreateCorridors(dungeon);
            
            // For consistency, make sure the first room in the list is the most suitable starting room
            // (e.g., it could be the largest room or one closest to the center)
            if (dungeon.Rooms.Count > 0)
            {
                // Find a room near the center to make it the starting room
                Room centerRoom = FindRoomNearestToCenter(dungeon.Rooms, dungeonArea.Center);
                
                // Move the center room to the beginning of the list
                if (centerRoom != null && dungeon.Rooms.Contains(centerRoom) && dungeon.Rooms[0] != centerRoom)
                {
                    dungeon.Rooms.Remove(centerRoom);
                    dungeon.Rooms.Insert(0, centerRoom);
                }
            }
            
            return dungeon;
        }
        
        /// <summary>
        /// Recursively splits a BSP node.
        /// </summary>
        /// <param name="node">The node to split.</param>
        /// <param name="iterations">Current recursion depth.</param>
        /// <returns>True if the node was split, false otherwise.</returns>
        private bool SplitBSPNode(BSPNode node, int iterations)
        {
            // Stop recursion if we've reached maximum depth
            if (iterations >= MAX_ITERATIONS)
            {
                return false;
            }
            
            // Don't split if the node is already too small
            if (node.Area.Width < MIN_LEAF_SIZE * 2 || node.Area.Height < MIN_LEAF_SIZE * 2)
            {
                return false;
            }
            
            // Decide whether to split horizontally or vertically based on aspect ratio
            bool splitHorizontally = node.Area.Width < node.Area.Height;
            
            // If the ratio is similar, choose randomly
            if (Math.Abs(node.Area.Width - node.Area.Height) < MIN_LEAF_SIZE)
            {
                splitHorizontally = _random.Next(2) == 0;
            }
            
            // Calculate the minimum and maximum valid split positions
            int min, max;
            if (splitHorizontally)
            {
                min = node.Area.Y + MIN_LEAF_SIZE;
                max = node.Area.Y + node.Area.Height - MIN_LEAF_SIZE;
            }
            else
            {
                min = node.Area.X + MIN_LEAF_SIZE;
                max = node.Area.X + node.Area.Width - MIN_LEAF_SIZE;
            }
            
            // If we can't split anymore, stop
            if (min >= max)
            {
                return false;
            }
            
            // Choose a random split position
            int splitPos = _random.Next(min, max);
            
            // Split the node
            if (!node.Split(splitHorizontally, splitPos - (splitHorizontally ? node.Area.Y : node.Area.X)))
            {
                return false;
            }
            
            // Recursively split the children
            SplitBSPNode(node.Left, iterations + 1);
            SplitBSPNode(node.Right, iterations + 1);
            
            return true;
        }
        
        /// <summary>
        /// Recursively collects all leaf nodes in the BSP tree.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <param name="leafNodes">The list to add leaf nodes to.</param>
        private void CollectLeafNodes(BSPNode node, List<BSPNode> leafNodes)
        {
            if (node.IsLeaf)
            {
                leafNodes.Add(node);
            }
            else
            {
                if (node.Left != null)
                {
                    CollectLeafNodes(node.Left, leafNodes);
                }
                if (node.Right != null)
                {
                    CollectLeafNodes(node.Right, leafNodes);
                }
            }
        }
        
        /// <summary>
        /// Creates a room within a BSP leaf node.
        /// </summary>
        /// <param name="node">The leaf node to create a room in.</param>
        private void CreateRoomInNode(BSPNode node)
        {
            // Calculate the maximum room size (as a percentage of the leaf size)
            int roomWidth = (int)(node.Area.Width * ROOM_SIZE_FACTOR);
            int roomHeight = (int)(node.Area.Height * ROOM_SIZE_FACTOR);
            
            // Ensure minimum room size
            roomWidth = Math.Max(roomWidth, MIN_ROOM_SIZE);
            roomHeight = Math.Max(roomHeight, MIN_ROOM_SIZE);
            
            // Ensure room doesn't exceed leaf size
            roomWidth = Math.Min(roomWidth, node.Area.Width - 1);
            roomHeight = Math.Min(roomHeight, node.Area.Height - 1);
            
            // Randomly position the room within the leaf
            int roomX = node.Area.X + _random.Next(0, node.Area.Width - roomWidth);
            int roomY = node.Area.Y + _random.Next(0, node.Area.Height - roomHeight);
            
            // Create the room
            Rectangle roomBounds = new Rectangle(roomX, roomY, roomWidth, roomHeight);
            Room room = new Room(roomBounds);
            
            // Assign the room to the node
            node.Room = room;
        }
        
        /// <summary>
        /// Creates corridors connecting the rooms in the dungeon.
        /// </summary>
        /// <param name="dungeon">The dungeon to create corridors in.</param>
        private void CreateCorridors(Dungeon dungeon)
        {
            // For each pair of sibling leaf nodes, connect their rooms
            ConnectRooms(dungeon.RootNode, dungeon);
        }
        
        /// <summary>
        /// Recursively connects rooms in the BSP tree.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <param name="dungeon">The dungeon to add corridors to.</param>
        private void ConnectRooms(BSPNode node, Dungeon dungeon)
        {
            // If this is a non-leaf node, recursively process children first
            if (!node.IsLeaf)
            {
                // Process left subtree
                if (node.Left != null)
                {
                    ConnectRooms(node.Left, dungeon);
                }
                
                // Process right subtree
                if (node.Right != null)
                {
                    ConnectRooms(node.Right, dungeon);
                }
                
                // Connect the rooms in the left and right children
                if (node.Left != null && node.Right != null)
                {
                    Room leftRoom = FindRoomInNode(node.Left);
                    Room rightRoom = FindRoomInNode(node.Right);
                    
                    if (leftRoom != null && rightRoom != null)
                    {
                        // Connect the centers of the rooms
                        Point start = new Point(
                            (int)leftRoom.Center.X,
                            (int)leftRoom.Center.Y);
                            
                        Point end = new Point(
                            (int)rightRoom.Center.X,
                            (int)rightRoom.Center.Y);
                            
                        dungeon.AddCorridor(start, end, CORRIDOR_WIDTH);
                    }
                }
            }
        }
        
        /// <summary>
        /// Finds a room in the given node or its children.
        /// </summary>
        /// <param name="node">The node to search.</param>
        /// <returns>A room, or null if no room was found.</returns>
        private Room FindRoomInNode(BSPNode node)
        {
            if (node.IsLeaf && node.Room != null)
            {
                return node.Room;
            }
            
            if (node.Left != null)
            {
                Room leftRoom = FindRoomInNode(node.Left);
                if (leftRoom != null)
                {
                    return leftRoom;
                }
            }
            
            if (node.Right != null)
            {
                Room rightRoom = FindRoomInNode(node.Right);
                if (rightRoom != null)
                {
                    return rightRoom;
                }
            }
            
            return null;
        }

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

        /// <summary>
        /// Finds the room nearest to the center of the dungeon.
        /// </summary>
        /// <param name="rooms">The list of rooms to search.</param>
        /// <param name="center">The center point of the dungeon.</param>
        /// <returns>The room nearest to the center.</returns>
        private Room FindRoomNearestToCenter(List<Room> rooms, Point center)
        {
            if (rooms.Count == 0) return null;
            
            Room nearestRoom = rooms[0];
            float nearestDistance = Vector2.Distance(
                new Vector2(center.X, center.Y),
                new Vector2(nearestRoom.Center.X, nearestRoom.Center.Y));
            
            foreach (Room room in rooms)
            {
                float distance = Vector2.Distance(
                    new Vector2(center.X, center.Y),
                    new Vector2(room.Center.X, room.Center.Y));
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestRoom = room;
                }
            }
            
            return nearestRoom;
        }
    }
} 