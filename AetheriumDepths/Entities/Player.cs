using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AetheriumDepths.Entities
{
    /// <summary>
    /// Represents the player character in the game.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Current position of the player in the game world.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The texture used to render the player.
        /// </summary>
        public Texture2D Sprite { get; set; }

        /// <summary>
        /// The last non-zero movement direction of the player.
        /// Used for attack direction.
        /// </summary>
        public Vector2 LastMovementDirection { get; private set; } = new Vector2(0, 1); // Default to facing down

        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            Sprite?.Width ?? 0, 
            Sprite?.Height ?? 0);

        /// <summary>
        /// Flag indicating if an attack is currently active.
        /// </summary>
        public bool IsAttacking { get; private set; }

        /// <summary>
        /// Timer for the duration of the attack.
        /// </summary>
        private float _attackTimer;

        /// <summary>
        /// Duration of the attack in seconds.
        /// </summary>
        private const float ATTACK_DURATION = 0.2f;

        /// <summary>
        /// The hitbox for the current attack.
        /// </summary>
        public Rectangle AttackHitbox { get; private set; }

        /// <summary>
        /// The size of the attack hitbox.
        /// </summary>
        private const int ATTACK_SIZE = 32;
        
        /// <summary>
        /// Flag indicating if a dodge is currently active.
        /// </summary>
        public bool IsDodging { get; private set; }
        
        /// <summary>
        /// Flag indicating if the player is currently invincible.
        /// </summary>
        public bool IsInvincible => IsDodging; // Currently only invincible during dodges
        
        /// <summary>
        /// Timer for the duration of the dodge.
        /// </summary>
        private float _dodgeTimer;
        
        /// <summary>
        /// Duration of the dodge in seconds.
        /// </summary>
        private const float DODGE_DURATION = 0.3f;
        
        /// <summary>
        /// Speed multiplier during dodge.
        /// </summary>
        private const float DODGE_SPEED_MULTIPLIER = 2.5f;

        /// <summary>
        /// Creates a new player at the specified position.
        /// </summary>
        /// <param name="position">The initial position of the player.</param>
        /// <param name="sprite">The sprite texture for the player.</param>
        public Player(Vector2 position, Texture2D sprite)
        {
            Position = position;
            Sprite = sprite;
        }

        /// <summary>
        /// Updates the player's position based on the movement vector.
        /// </summary>
        /// <param name="movementVector">The direction to move in.</param>
        /// <param name="speed">The speed at which to move.</param>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        /// <param name="viewportBounds">The bounds of the visible game screen.</param>
        public void Move(Vector2 movementVector, float speed, float deltaTime, Rectangle viewportBounds)
        {
            // Apply movement
            float effectiveSpeed = speed;
            
            // Apply dodge speed boost if dodging
            if (IsDodging)
            {
                effectiveSpeed *= DODGE_SPEED_MULTIPLIER;
            }
            
            // If dodging with no movement vector, use last movement direction
            Vector2 effectiveMovementVector = movementVector;
            if (IsDodging && movementVector == Vector2.Zero && LastMovementDirection != Vector2.Zero)
            {
                effectiveMovementVector = LastMovementDirection;
            }
            
            Vector2 newPosition = Position + (effectiveMovementVector * effectiveSpeed * deltaTime);

            // If movement direction is non-zero, update LastMovementDirection
            if (movementVector != Vector2.Zero)
            {
                LastMovementDirection = Vector2.Normalize(movementVector);
            }

            // Clamp to viewport bounds (accounting for sprite size)
            newPosition.X = MathHelper.Clamp(newPosition.X, 0, viewportBounds.Width - (Sprite?.Width ?? 0));
            newPosition.Y = MathHelper.Clamp(newPosition.Y, 0, viewportBounds.Height - (Sprite?.Height ?? 0));

            // Update position
            Position = newPosition;
        }

        /// <summary>
        /// Updates the player's state, including attack and dodge timers.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        public void Update(float deltaTime)
        {
            // Update attack timer if an attack is active
            if (IsAttacking)
            {
                _attackTimer -= deltaTime;
                if (_attackTimer <= 0f)
                {
                    IsAttacking = false;
                }
            }
            
            // Update dodge timer if a dodge is active
            if (IsDodging)
            {
                _dodgeTimer -= deltaTime;
                if (_dodgeTimer <= 0f)
                {
                    IsDodging = false;
                }
            }
        }

        /// <summary>
        /// Initiates a melee attack in the direction the player is facing.
        /// </summary>
        public void Attack()
        {
            if (!IsAttacking)
            {
                IsAttacking = true;
                _attackTimer = ATTACK_DURATION;
                
                // Calculate attack hitbox position based on player position and last movement direction
                Vector2 hitboxCenter = Position + (LastMovementDirection * (Sprite.Width + ATTACK_SIZE) / 2);
                
                // Create attack hitbox
                AttackHitbox = new Rectangle(
                    (int)(hitboxCenter.X - ATTACK_SIZE / 2),
                    (int)(hitboxCenter.Y - ATTACK_SIZE / 2),
                    ATTACK_SIZE,
                    ATTACK_SIZE);
            }
        }

        /// <summary>
        /// Deactivates the current attack.
        /// </summary>
        public void DeactivateAttack()
        {
            IsAttacking = false;
            _attackTimer = 0f;
        }
        
        /// <summary>
        /// Initiates a dodge action, granting temporary invincibility and a speed boost.
        /// </summary>
        public void Dodge()
        {
            if (!IsDodging)
            {
                IsDodging = true;
                _dodgeTimer = DODGE_DURATION;
            }
        }

        /// <summary>
        /// Draws the player to the screen.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the player with a semi-transparent effect when invincible
            Color playerColor = IsInvincible ? new Color(255, 255, 255, 150) : Color.White;
            spriteBatch.Draw(Sprite, Position, playerColor);
        }

        /// <summary>
        /// Draws the attack hitbox for debugging purposes.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to use for drawing.</param>
        /// <param name="debugTexture">A solid color texture for drawing the hitbox.</param>
        public void DrawAttackHitbox(SpriteBatch spriteBatch, Texture2D debugTexture)
        {
            if (IsAttacking)
            {
                spriteBatch.Draw(debugTexture, AttackHitbox, Color.Red * 0.5f);
            }
        }
    }
} 