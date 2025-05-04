using Microsoft.Xna.Framework;

namespace AetheriumDepths.Core
{
    /// <summary>
    /// Utility class providing collision detection methods.
    /// </summary>
    public static class CollisionUtility
    {
        /// <summary>
        /// Checks if two rectangles (Axis-Aligned Bounding Boxes) intersect.
        /// </summary>
        /// <param name="rect1">The first rectangle.</param>
        /// <param name="rect2">The second rectangle.</param>
        /// <returns>True if the rectangles intersect; false otherwise.</returns>
        public static bool CheckAABBCollision(Rectangle rect1, Rectangle rect2)
        {
            // Use Rectangle's built-in Intersects method for AABB collision
            return rect1.Intersects(rect2);
        }
    }
} 