using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using AetheriumDepths.Entities;

namespace AetheriumDepths.Gameplay
{
    /// <summary>
    /// Manages combat interactions between entities, such as damage application, 
    /// health checks, and death events.
    /// </summary>
    public class CombatManager
    {
        // Events
        /// <summary>
        /// Event triggered when an enemy is killed.
        /// Provides the position of the killed enemy.
        /// </summary>
        public event Action<Vector2> EnemyKilled;

        // Combat constants
        private readonly int _enemyTouchDamage;
        private readonly int _baseAttackDamage;
        private readonly int _damageBuffMultiplier;
        private readonly int _aetheriumEssenceReward;

        // Damage invincibility
        private float _playerInvincibilityTimer = 0f;
        private readonly float _playerInvincibilityDuration;

        // References
        private Player _player;
        private List<Enemy> _enemies;

        /// <summary>
        /// Creates a new CombatManager to handle combat interactions.
        /// </summary>
        /// <param name="player">Reference to the player entity.</param>
        /// <param name="enemies">Reference to the list of active enemies.</param>
        /// <param name="enemyTouchDamage">Damage dealt when enemy touches player.</param>
        /// <param name="baseAttackDamage">Base player attack damage.</param>
        /// <param name="damageBuffMultiplier">Damage multiplier when buff is active.</param>
        /// <param name="aetheriumEssenceReward">Essence rewarded when enemy is defeated.</param>
        /// <param name="invincibilityDuration">Duration of player invincibility after taking damage.</param>
        public CombatManager(
            Player player, 
            List<Enemy> enemies, 
            int enemyTouchDamage = 10, 
            int baseAttackDamage = 1, 
            int damageBuffMultiplier = 2, 
            int aetheriumEssenceReward = 1,
            float invincibilityDuration = 1.0f)
        {
            _player = player;
            _enemies = enemies;
            _enemyTouchDamage = enemyTouchDamage;
            _baseAttackDamage = baseAttackDamage;
            _damageBuffMultiplier = damageBuffMultiplier;
            _aetheriumEssenceReward = aetheriumEssenceReward;
            _playerInvincibilityDuration = invincibilityDuration;
        }

        /// <summary>
        /// Updates the internal timers for combat.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        public void Update(float deltaTime)
        {
            // Update invincibility timer
            if (_playerInvincibilityTimer > 0)
            {
                _playerInvincibilityTimer -= deltaTime;
                if (_playerInvincibilityTimer < 0)
                {
                    _playerInvincibilityTimer = 0;
                }
            }
        }

        /// <summary>
        /// Checks if the player is currently invincible after taking damage.
        /// </summary>
        public bool IsPlayerInvincibleFromDamage => _playerInvincibilityTimer > 0;

        /// <summary>
        /// Gets the remaining invincibility time for the player after taking damage.
        /// </summary>
        public float PlayerDamageInvincibilityTimer => _playerInvincibilityTimer;

        /// <summary>
        /// Apply damage from one entity to another.
        /// </summary>
        /// <param name="attacker">The attacking entity (can be null for environment damage).</param>
        /// <param name="target">The entity receiving damage.</param>
        /// <param name="damageAmount">The amount of damage to apply.</param>
        /// <returns>True if the target was damaged, false if invincible or invalid target.</returns>
        public bool ApplyDamage(object attacker, object target, int damageAmount)
        {
            // Handle player taking damage
            if (target is Player player)
            {
                // Check if player is invincible (from dodge or damage cooldown)
                if (player.IsInvincible || IsPlayerInvincibleFromDamage)
                {
                    return false;
                }

                // Apply damage to player
                bool playerAlive = player.TakeDamage(damageAmount);
                
                // Start invincibility timer
                _playerInvincibilityTimer = _playerInvincibilityDuration;
                
                return true;
            }
            
            // Handle enemy taking damage
            if (target is Enemy enemy)
            {
                // Calculate damage, accounting for player damage buff
                int effectiveDamage = damageAmount;
                if (attacker is Player attackingPlayer && attackingPlayer.HasDamageBuff)
                {
                    effectiveDamage *= _damageBuffMultiplier;
                }
                
                // Apply damage to enemy
                enemy.TakeDamage(effectiveDamage);
                
                // Check if enemy was killed
                if (!enemy.IsActive)
                {
                    HandleEnemyDeath(enemy);
                }
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Apply damage to the player from enemy touch or attack.
        /// </summary>
        /// <returns>True if the player was damaged, false if invincible.</returns>
        public bool ApplyPlayerDamage()
        {
            return ApplyDamage(null, _player, _enemyTouchDamage);
        }

        /// <summary>
        /// Apply damage from player attack to an enemy.
        /// </summary>
        /// <param name="enemy">The enemy to damage.</param>
        /// <returns>True if damage was applied.</returns>
        public bool ApplyPlayerAttackDamage(Enemy enemy)
        {
            return ApplyDamage(_player, enemy, _baseAttackDamage);
        }

        /// <summary>
        /// Handle the death of an enemy.
        /// </summary>
        /// <param name="enemy">The enemy that was killed.</param>
        private void HandleEnemyDeath(Enemy enemy)
        {
            // Reward player with essence
            _player.AddAetheriumEssence(_aetheriumEssenceReward);
            
            // Trigger the EnemyKilled event with the enemy's position
            EnemyKilled?.Invoke(enemy.Position);
            
            Console.WriteLine($"Enemy defeated! Player received {_aetheriumEssenceReward} Aetherium Essence.");
        }
    }
} 