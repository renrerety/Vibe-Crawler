using Microsoft.Xna.Framework;
using System;

namespace AetheriumDepths.Core
{
    /// <summary>
    /// Manages the different game states and transitions between them.
    /// </summary>
    public class StateManager
    {
        // Current active state
        private GameState _currentState;

        // Event that fires when state changes
        public event EventHandler<GameState> StateChanged;

        /// <summary>
        /// Available game states
        /// </summary>
        public enum GameState
        {
            MainMenu,
            Gameplay,
            Paused,
            GameOver
        }

        /// <summary>
        /// Gets the current game state
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// Creates a new state manager starting with the specified state
        /// </summary>
        /// <param name="initialState">The initial game state</param>
        public StateManager(GameState initialState = GameState.Gameplay)
        {
            _currentState = initialState;
            Console.WriteLine($"StateManager initialized with state: {_currentState}");
        }

        /// <summary>
        /// Changes the current game state
        /// </summary>
        /// <param name="newState">The new state to change to</param>
        public void ChangeState(GameState newState)
        {
            if (_currentState != newState)
            {
                Console.WriteLine($"State changing from {_currentState} to {newState}");
                _currentState = newState;
                
                // Notify subscribers of state change
                StateChanged?.Invoke(this, _currentState);
            }
        }

        /// <summary>
        /// Updates the current state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            // In a more complex implementation, the state manager might handle
            // additional logic here such as transition effects between states
            Console.WriteLine($"StateManager updating state: {_currentState}");
        }
    }
} 