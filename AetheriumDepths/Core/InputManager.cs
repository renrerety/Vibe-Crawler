using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace AetheriumDepths.Core
{
    /// <summary>
    /// Manages input from various devices (keyboard, mouse, gamepad, touch)
    /// and provides an abstraction layer for game actions.
    /// </summary>
    public class InputManager
    {
        // Store current and previous keyboard states to detect just-pressed keys
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        // Store current and previous gamepad states to detect just-pressed buttons
        private GamePadState _currentGamePadState;
        private GamePadState _previousGamePadState;

        /// <summary>
        /// Enum representing the possible game actions
        /// </summary>
        public enum GameAction
        {
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            Attack,
            Dodge,
            Interact,
            UseSpell
        }

        /// <summary>
        /// Creates a new input manager
        /// </summary>
        public InputManager()
        {
            // Initialize keyboard states
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;

            // Initialize gamepad states
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _previousGamePadState = _currentGamePadState;
        }

        /// <summary>
        /// Updates the input states. Should be called once per frame.
        /// </summary>
        public void Update()
        {
            // Update keyboard states
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            // Update gamepad states
            _previousGamePadState = _currentGamePadState;
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Checks if a game action is currently being pressed.
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <returns>True if the action is being pressed</returns>
        public bool IsActionPressed(GameAction action)
        {
            return action switch
            {
                GameAction.MoveUp => _currentKeyboardState.IsKeyDown(Keys.W) || 
                                     _currentKeyboardState.IsKeyDown(Keys.Up) ||
                                     _currentGamePadState.DPad.Up == ButtonState.Pressed ||
                                     _currentGamePadState.ThumbSticks.Left.Y > 0.5f,

                GameAction.MoveDown => _currentKeyboardState.IsKeyDown(Keys.S) || 
                                       _currentKeyboardState.IsKeyDown(Keys.Down) ||
                                       _currentGamePadState.DPad.Down == ButtonState.Pressed ||
                                       _currentGamePadState.ThumbSticks.Left.Y < -0.5f,

                GameAction.MoveLeft => _currentKeyboardState.IsKeyDown(Keys.A) || 
                                       _currentKeyboardState.IsKeyDown(Keys.Left) ||
                                       _currentGamePadState.DPad.Left == ButtonState.Pressed ||
                                       _currentGamePadState.ThumbSticks.Left.X < -0.5f,

                GameAction.MoveRight => _currentKeyboardState.IsKeyDown(Keys.D) || 
                                        _currentKeyboardState.IsKeyDown(Keys.Right) ||
                                        _currentGamePadState.DPad.Right == ButtonState.Pressed ||
                                        _currentGamePadState.ThumbSticks.Left.X > 0.5f,

                GameAction.Attack => _currentKeyboardState.IsKeyDown(Keys.Space) ||
                                     _currentGamePadState.Buttons.X == ButtonState.Pressed,

                GameAction.Dodge => _currentKeyboardState.IsKeyDown(Keys.LeftShift) ||
                                    _currentGamePadState.Buttons.A == ButtonState.Pressed,

                GameAction.Interact => _currentKeyboardState.IsKeyDown(Keys.E) ||
                                       _currentGamePadState.Buttons.B == ButtonState.Pressed,

                GameAction.UseSpell => _currentKeyboardState.IsKeyDown(Keys.Q) ||
                                       _currentGamePadState.IsButtonDown(Buttons.RightShoulder),

                _ => false
            };
        }

        /// <summary>
        /// Checks if a game action was just pressed (i.e., pressed this frame but not last frame).
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <returns>True if the action was just pressed</returns>
        public bool IsActionJustPressed(GameAction action)
        {
            return action switch
            {
                GameAction.MoveUp => (IsKeyJustPressed(Keys.W) || IsKeyJustPressed(Keys.Up) ||
                                     IsButtonJustPressed(Buttons.DPadUp) ||
                                     (_currentGamePadState.ThumbSticks.Left.Y > 0.5f && _previousGamePadState.ThumbSticks.Left.Y <= 0.5f)),

                GameAction.MoveDown => (IsKeyJustPressed(Keys.S) || IsKeyJustPressed(Keys.Down) ||
                                       IsButtonJustPressed(Buttons.DPadDown) ||
                                       (_currentGamePadState.ThumbSticks.Left.Y < -0.5f && _previousGamePadState.ThumbSticks.Left.Y >= -0.5f)),

                GameAction.MoveLeft => (IsKeyJustPressed(Keys.A) || IsKeyJustPressed(Keys.Left) ||
                                       IsButtonJustPressed(Buttons.DPadLeft) ||
                                       (_currentGamePadState.ThumbSticks.Left.X < -0.5f && _previousGamePadState.ThumbSticks.Left.X >= -0.5f)),

                GameAction.MoveRight => (IsKeyJustPressed(Keys.D) || IsKeyJustPressed(Keys.Right) ||
                                        IsButtonJustPressed(Buttons.DPadRight) ||
                                        (_currentGamePadState.ThumbSticks.Left.X > 0.5f && _previousGamePadState.ThumbSticks.Left.X <= 0.5f)),

                GameAction.Attack => IsKeyJustPressed(Keys.Space) || IsButtonJustPressed(Buttons.X),

                GameAction.Dodge => IsKeyJustPressed(Keys.LeftShift) || IsButtonJustPressed(Buttons.A),

                GameAction.Interact => IsKeyJustPressed(Keys.E) || IsButtonJustPressed(Buttons.B),

                GameAction.UseSpell => IsKeyJustPressed(Keys.Q) || IsButtonJustPressed(Buttons.RightShoulder),

                _ => false
            };
        }

        /// <summary>
        /// Gets a vector representing the player's movement direction.
        /// </summary>
        /// <returns>A normalized vector representing movement direction.</returns>
        public Vector2 GetMovementVector()
        {
            Vector2 movementVector = Vector2.Zero;

            // Add keyboard/DPad input
            if (IsActionPressed(GameAction.MoveUp))
                movementVector.Y -= 1;
            if (IsActionPressed(GameAction.MoveDown))
                movementVector.Y += 1;
            if (IsActionPressed(GameAction.MoveLeft))
                movementVector.X -= 1;
            if (IsActionPressed(GameAction.MoveRight))
                movementVector.X += 1;

            // Add thumbstick input
            Vector2 thumbStick = _currentGamePadState.ThumbSticks.Left;
            thumbStick.Y *= -1; // Invert Y axis to match screen coordinates

            if (Math.Abs(thumbStick.X) > 0.25f || Math.Abs(thumbStick.Y) > 0.25f)
            {
                movementVector += thumbStick;
            }

            // Normalize the vector if it has a magnitude
            if (movementVector != Vector2.Zero)
            {
                movementVector.Normalize();
            }

            return movementVector;
        }

        // Private helper methods
        private bool IsKeyJustPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a specific key was just pressed (i.e., pressed this frame but not last frame).
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key was just pressed</returns>
        public bool CheckKeyJustPressed(Keys key)
        {
            return IsKeyJustPressed(key);
        }

        private bool IsButtonJustPressed(Buttons button)
        {
            return _currentGamePadState.IsButtonDown(button) && _previousGamePadState.IsButtonUp(button);
        }
    }
} 