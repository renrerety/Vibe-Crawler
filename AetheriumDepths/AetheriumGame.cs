using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using AetheriumDepths.Core;

namespace AetheriumDepths
{
    /// <summary>
    /// The main game class for Aetherium Depths.
    /// </summary>
    public class AetheriumGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game state management
        private StateManager _stateManager;

        public AetheriumGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Initialize state manager with Gameplay as default
            _stateManager = new StateManager(StateManager.GameState.Gameplay);
            _stateManager.StateChanged += OnStateChanged;

            // Initialization logic
            Console.WriteLine("Initializing Aetherium Depths...");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Console.WriteLine("Loading content...");
        }

        protected override void Update(GameTime gameTime)
        {
            // Check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // Check for debug state changes
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.P))
            {
                // Toggle between Gameplay and Paused states
                if (_stateManager.CurrentState == StateManager.GameState.Gameplay)
                    _stateManager.ChangeState(StateManager.GameState.Paused);
                else if (_stateManager.CurrentState == StateManager.GameState.Paused)
                    _stateManager.ChangeState(StateManager.GameState.Gameplay);
            }
            else if (keyState.IsKeyDown(Keys.M))
            {
                // Switch to MainMenu state
                _stateManager.ChangeState(StateManager.GameState.MainMenu);
            }

            // Update state manager
            _stateManager.Update(gameTime);

            // Update based on current state
            switch (_stateManager.CurrentState)
            {
                case StateManager.GameState.MainMenu:
                    UpdateMainMenu(gameTime);
                    break;
                case StateManager.GameState.Gameplay:
                    UpdateGameplay(gameTime);
                    break;
                case StateManager.GameState.Paused:
                    UpdatePaused(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Start drawing
            _spriteBatch.Begin();

            // Draw based on current state
            switch (_stateManager.CurrentState)
            {
                case StateManager.GameState.MainMenu:
                    DrawMainMenu(gameTime);
                    break;
                case StateManager.GameState.Gameplay:
                    DrawGameplay(gameTime);
                    break;
                case StateManager.GameState.Paused:
                    DrawPaused(gameTime);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void OnStateChanged(object sender, StateManager.GameState newState)
        {
            Console.WriteLine($"Game state changed to: {newState}");
        }

        #region State-specific Update and Draw methods

        private void UpdateMainMenu(GameTime gameTime)
        {
            Console.WriteLine($"Updating MainMenu state: {gameTime.TotalGameTime}");
        }

        private void UpdateGameplay(GameTime gameTime)
        {
            Console.WriteLine($"Updating Gameplay state: {gameTime.TotalGameTime}");
        }

        private void UpdatePaused(GameTime gameTime)
        {
            Console.WriteLine($"Updating Paused state: {gameTime.TotalGameTime}");
        }

        private void DrawMainMenu(GameTime gameTime)
        {
            Console.WriteLine($"Drawing MainMenu state: {gameTime.TotalGameTime}");
        }

        private void DrawGameplay(GameTime gameTime)
        {
            Console.WriteLine($"Drawing Gameplay state: {gameTime.TotalGameTime}");
        }

        private void DrawPaused(GameTime gameTime)
        {
            Console.WriteLine($"Drawing Paused state: {gameTime.TotalGameTime}");
        }

        #endregion
    }
} 