using Microsoft.Xna.Framework;
using System;

namespace AetheriumDepths.Generation
{
    /// <summary>
    /// Represents a node in a Binary Space Partitioning (BSP) tree.
    /// Used for procedural dungeon generation.
    /// </summary>
    public class BSPNode
    {
        /// <summary>
        /// The rectangular area this node represents.
        /// </summary>
        public Rectangle Area { get; private set; }
        
        /// <summary>
        /// The left child of this node.
        /// </summary>
        public BSPNode Left { get; set; }
        
        /// <summary>
        /// The right child of this node.
        /// </summary>
        public BSPNode Right { get; set; }
        
        /// <summary>
        /// The parent of this node.
        /// </summary>
        public BSPNode Parent { get; set; }
        
        /// <summary>
        /// The room contained within this node (if it's a leaf node).
        /// </summary>
        public Room Room { get; set; }
        
        /// <summary>
        /// Whether this node is split horizontally (true) or vertically (false).
        /// </summary>
        public bool IsSplitHorizontally { get; private set; }
        
        /// <summary>
        /// The split position where this node was divided.
        /// </summary>
        public int SplitPosition { get; private set; }
        
        /// <summary>
        /// Whether this node is a leaf (has no children).
        /// </summary>
        public bool IsLeaf => Left == null && Right == null;
        
        /// <summary>
        /// Creates a new BSP node with the specified area.
        /// </summary>
        /// <param name="area">The rectangular area this node represents.</param>
        public BSPNode(Rectangle area)
        {
            Area = area;
        }
        
        /// <summary>
        /// Splits this node either horizontally or vertically.
        /// </summary>
        /// <param name="splitHorizontally">Whether to split horizontally (true) or vertically (false).</param>
        /// <param name="splitPosition">The position along the axis to split at.</param>
        /// <returns>True if the split was successful, false otherwise.</returns>
        public bool Split(bool splitHorizontally, int splitPosition)
        {
            // Don't split if we're already split
            if (!IsLeaf)
            {
                return false;
            }
            
            // Store the split information
            IsSplitHorizontally = splitHorizontally;
            SplitPosition = splitPosition;
            
            if (splitHorizontally)
            {
                // Split horizontally (create top and bottom areas)
                Rectangle topArea = new Rectangle(Area.X, Area.Y, Area.Width, splitPosition);
                Rectangle bottomArea = new Rectangle(Area.X, Area.Y + splitPosition, Area.Width, Area.Height - splitPosition);
                
                // Create child nodes
                Left = new BSPNode(topArea) { Parent = this };
                Right = new BSPNode(bottomArea) { Parent = this };
            }
            else
            {
                // Split vertically (create left and right areas)
                Rectangle leftArea = new Rectangle(Area.X, Area.Y, splitPosition, Area.Height);
                Rectangle rightArea = new Rectangle(Area.X + splitPosition, Area.Y, Area.Width - splitPosition, Area.Height);
                
                // Create child nodes
                Left = new BSPNode(leftArea) { Parent = this };
                Right = new BSPNode(rightArea) { Parent = this };
            }
            
            return true;
        }
    }
} 