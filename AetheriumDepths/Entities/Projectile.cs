using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AetheriumDepths.Generation;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents a projectile that can be fired by the player or enemies.
    /// </summary>
    public class Projectile
    {
        /// <summary>
        /// Current position of the projectile.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// Direction and speed vector of the projectile.
        /// </summary>
        private Vector2 _velocity;
        
        /// <summary>
        /// The texture used to render the projectile.
        /// </summary>
        public Texture2D Sprite { get; private set; }
        
        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - (Sprite?.Width ?? 0) / 2, 
            (int)Position.Y - (Sprite?.Height ?? 0) / 2, 
            Sprite?.Width ?? 0, 
            Sprite?.Height ?? 0);
            
        /// <summary>
        /// Damage dealt by this projectile when it hits a target.
        /// </summary>
        public int Damage { get; private set; }
        
        /// <summary>
        /// Flag indicating if this projectile is active.
        /// Inactive projectiles are scheduled for removal.
        /// </summary>
        public bool IsActive { get; private set; } = true;
        
        /// <summary>
        /// Tracks the entity type that fired this projectile.
        /// Used to prevent friendly fire.
        /// </summary>
        public bool IsPlayerProjectile { get; private set; }
        
        /// <summary>
        /// Alias for IsPlayerProjectile for compatibility.
        /// </summary>
        public bool IsPlayerOwned => IsPlayerProjectile;
        
        /// <summary>
        /// Current rotation of the projectile in radians.
        /// </summary>
        private float _rotation = 0f;
        
        /// <summary>
        /// Speed at which the projectile rotates.
        /// </summary>
        private float _rotationSpeed = 0f;
        
        /// <summary>
        /// Maximum lifetime of the projectile in seconds.
        /// </summary>
        private float _maxLifetime = 5.0f;
        
        /// <summary>
        /// Current lifetime of the projectile in seconds.
        /// </summary>
        private float _currentLifetime = 0f;
        
        /// <summary>
        /// Creates a new projectile.
        /// </summary>
        /// <param name="position">The starting position of the projectile.</param>
        /// <param name="direction">The normalized direction vector.</param>
        /// <param name="sprite">The sprite texture for the projectile.</param>
        /// <param name="damage">The damage the projectile deals when it hits.</param>
        /// <param name="speed">The speed of the projectile in pixels per second.</param>
        /// <param name="isPlayerProjectile">Whether this projectile was fired by the player.</param>
        public Projectile(Vector2 position, Vector2 direction, Texture2D sprite, int damage, float speed, bool isPlayerProjectile = false)
        {
            Position = position;
            Sprite = sprite;
            Damage = damage;
            IsPlayerProjectile = isPlayerProjectile;
            
            // Calculate velocity vector based on direction and speed
            _velocity = direction * speed;
            
            // Set random rotation speed for visual effect
            _rotationSpeed = (float)(System.Math.Sign(direction.X) * 10.0f);
            
            // Calculate initial rotation based on direction
            if (direction != Vector2.Zero)
            {
                _rotation = (float)System.Math.Atan2(direction.Y, direction.X);
            }
        }
        
        /// <summary>
        /// Updates the projectile's position and checks for out-of-bounds.
        /// </summary>
        /// <param name="gameTime">Game time information for frame-rate independent movement.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        public void Update(GameTime gameTime, Dungeon dungeon)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Update(deltaTime, dungeon);
        }
        
        /// <summary>
        /// Updates the projectile's position and checks for out-of-bounds.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="dungeon">The current dungeon for collision detection.</param>
        public void Update(float deltaTime, Dungeon dungeon)
        {
            if (!IsActive) return;
            
            // Update position based on velocity
            Vector2 newPosition = Position + _velocity * deltaTime;
            
            // Update rotation
            _rotation += _rotationSpeed * deltaTime;
            
            // Update lifetime
            _currentLifetime += deltaTime;
            if (_currentLifetime >= _maxLifetime)
            {
                Deactivate();
                return;
            }
            
            // Check for dungeon collision
            Rectangle proposedBounds = new Rectangle(
                (int)newPosition.X - Sprite.Width / 2,
                (int)newPosition.Y - Sprite.Height / 2,
                Sprite.Width,
                Sprite.Height);
                
            // Deactivate if hitting a wall
            if (!dungeon.IsMovementValid(proposedBounds))
            {
                Deactivate();
                return;
            }
            
            // Update position if all checks pass
            Position = newPosition;
        }
        
        /// <summary>
        /// Deactivates the projectile.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
        
        /// <summary>
        /// Draws the projectile to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || Sprite == null) return;
            
            // Draw projectile with rotation and proper origin
            spriteBatch.Draw(
                Sprite,
                Position,
                null,
                IsPlayerProjectile ? Color.Cyan : Color.OrangeRed,
                _rotation,
                new Vector2(Sprite.Width / 2, Sprite.Height / 2),
                1.0f,
                SpriteEffects.None,
                0f);
        }
    }
} 