using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AetheriumDepths.Core
{
    /// <summary>
    /// Represents a 2D camera in the game world.
    /// </summary>
    public class Camera2D
    {
        /// <summary>
        /// The position of the camera in world space.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The current zoom level of the camera.
        /// </summary>
        public float Zoom { get; set; } = 1.0f;

        /// <summary>
        /// The current rotation of the camera in radians.
        /// </summary>
        public float Rotation { get; set; } = 0.0f;

        /// <summary>
        /// The viewport dimensions used by the camera.
        /// </summary>
        public Viewport Viewport { get; private set; }

        /// <summary>
        /// Creates a new 2D camera.
        /// </summary>
        /// <param name="viewport">The viewport to use for the camera.</param>
        public Camera2D(Viewport viewport)
        {
            Viewport = viewport;
            Position = Vector2.Zero;
        }

        /// <summary>
        /// Gets the view transform matrix for the camera.
        /// </summary>
        /// <returns>A transformation matrix that positions, rotates, and scales based on camera properties.</returns>
        public Matrix GetTransformMatrix()
        {
            // Create the transformation matrix
            return 
                // Translate to the negative of the camera position
                Matrix.CreateTranslation(new Vector3(-Position, 0)) *
                // Rotate around the origin
                Matrix.CreateRotationZ(Rotation) *
                // Scale by the zoom factor
                Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                // Translate to position the view at the center of the viewport
                Matrix.CreateTranslation(new Vector3(Viewport.Width * 0.5f, Viewport.Height * 0.5f, 0));
        }

        /// <summary>
        /// Moves the camera to a target position with smooth lerping.
        /// </summary>
        /// <param name="targetPosition">The position to move towards.</param>
        /// <param name="lerpFactor">The speed factor for the lerp (0-1).</param>
        public void MoveToTarget(Vector2 targetPosition, float lerpFactor)
        {
            Position = Vector2.Lerp(Position, targetPosition, lerpFactor);
        }

        /// <summary>
        /// Updates the viewport used by the camera.
        /// </summary>
        /// <param name="viewport">The new viewport to use.</param>
        public void UpdateViewport(Viewport viewport)
        {
            Viewport = viewport;
        }
    }
} 