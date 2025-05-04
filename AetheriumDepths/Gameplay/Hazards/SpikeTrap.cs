using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AetheriumDepths.Gameplay.Hazards
{
    /// <summary>
    /// Represents a spike trap hazard in the game that activates on a timer.
    /// </summary>
    public class SpikeTrap
    {
        /// <summary>
        /// Current position of the spike trap in the game world.
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// The texture used to render the trap when spikes are visible (armed).
        /// </summary>
        public Texture2D SpriteArmed { get; set; }
        
        /// <summary>
        /// The texture used to render the trap when spikes are hidden (disarmed).
        /// </summary>
        public Texture2D SpriteDisarmed { get; set; }
        
        /// <summary>
        /// The bounding rectangle used for collision detection.
        /// </summary>
        public Rectangle Bounds => new Rectangle(
            (int)Position.X, 
            (int)Position.Y, 
            SpriteArmed?.Width ?? 0, 
            SpriteArmed?.Height ?? 0);
            
        /// <summary>
        /// Flag indicating if the trap is currently active (spikes are out).
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Timer for the current phase (activation countdown or active duration).
        /// </summary>
        private float _phaseTimer;
        
        /// <summary>
        /// Duration in seconds that the trap remains in the armed state.
        /// </summary>
        public float ActiveDuration { get; set; } = 1.0f;
        
        /// <summary>
        /// Duration in seconds that the trap remains in the disarmed state before activating again.
        /// </summary>
        public float CooldownDuration { get; set; } = 2.0f;
        
        /// <summary>
        /// The amount of damage the trap deals when active.
        /// </summary>
        public int Damage { get; set; } = 15;
        
        /// <summary>
        /// Dictionary mapping entities to their damage cooldown timers to prevent multiple hits in one activation.
        /// </summary>
        private Dictionary<object, float> _damageCooldowns = new Dictionary<object, float>();
        
        /// <summary>
        /// Duration in seconds that an entity is immune to damage from this trap after being hit.
        /// </summary>
        private const float DAMAGE_COOLDOWN = 0.5f;
        
        /// <summary>
        /// Creates a new spike trap at the specified position.
        /// </summary>
        /// <param name="position">The position of the trap in the game world.</param>
        /// <param name="spriteArmed">The sprite used when the trap is armed (spikes visible).</param>
        /// <param name="spriteDisarmed">The sprite used when the trap is disarmed (spikes hidden).</param>
        public SpikeTrap(Vector2 position, Texture2D spriteArmed, Texture2D spriteDisarmed)
        {
            Position = position;
            SpriteArmed = spriteArmed;
            SpriteDisarmed = spriteDisarmed;
            IsActive = false; // Start in the disarmed state
            _phaseTimer = CooldownDuration; // Start with full cooldown
        }
        
        /// <summary>
        /// Updates the spike trap's state based on timers.
        /// </summary>
        /// <param name="gameTime">Game time information for frame-independent updates.</param>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update damage cooldowns
            List<object> expiredCooldowns = new List<object>();
            foreach (var entity in _damageCooldowns.Keys)
            {
                _damageCooldowns[entity] -= deltaTime;
                if (_damageCooldowns[entity] <= 0)
                {
                    expiredCooldowns.Add(entity);
                }
            }
            
            // Remove expired cooldowns
            foreach (var entity in expiredCooldowns)
            {
                _damageCooldowns.Remove(entity);
            }
            
            // Update phase timer
            _phaseTimer -= deltaTime;
            
            // Check if timer has expired
            if (_phaseTimer <= 0)
            {
                // Toggle active state
                IsActive = !IsActive;
                
                // Reset timer based on new state
                _phaseTimer = IsActive ? ActiveDuration : CooldownDuration;
            }
        }
        
        /// <summary>
        /// Checks if an entity is on cooldown for damage from this trap.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>True if the entity is on cooldown; false otherwise.</returns>
        public bool IsEntityOnDamageCooldown(object entity)
        {
            return _damageCooldowns.ContainsKey(entity) && _damageCooldowns[entity] > 0;
        }
        
        /// <summary>
        /// Sets a damage cooldown for an entity after it takes damage from this trap.
        /// </summary>
        /// <param name="entity">The entity to set cooldown for.</param>
        public void SetEntityDamageCooldown(object entity)
        {
            _damageCooldowns[entity] = DAMAGE_COOLDOWN;
        }
        
        /// <summary>
        /// Draws the spike trap with the appropriate sprite based on its state.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used for rendering.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Select the appropriate sprite based on the trap's state
            Texture2D currentSprite = IsActive ? SpriteArmed : SpriteDisarmed;
            
            // Draw the trap
            if (currentSprite != null)
            {
                spriteBatch.Draw(
                    currentSprite,
                    Position,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    0f);
            }
        }
    }
} 