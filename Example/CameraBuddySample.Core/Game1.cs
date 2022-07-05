using CameraBuddy;
using CollisionBuddy;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PrimitiveBuddy;
using ResolutionBuddy;

namespace CameraBuddySample
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		#region Members

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Circle _circle1;
		Circle _circle2;

		GameClock _clock;

		InputState _inputState;
		ControllerWrapper _controller;
		InputWrapper _inputWrapper;

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		ICamera _camera;

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			graphics.IsFullScreen = false;

			var resolution = new ResolutionComponent(this, graphics, new Point(1280, 720), new Point(1280, 720), false, true, true);

			_circle1 = new Circle();
			_circle2 = new Circle();

			_clock = new GameClock();
			_inputState = new InputState();
			_controller = new ControllerWrapper(0);
			_inputWrapper = new InputWrapper(_controller, _clock.GetCurrentTime);

			//set up the camera
			_camera = new Camera();
			_camera.TopPadding = 0.2f;
			_camera.WorldBoundary = new Rectangle(-2000, -1000, 4000, 2000);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			//init the blue circle so it will be on the left of the screen
			_circle1.Initialize(new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.X - 80,
											graphics.GraphicsDevice.Viewport.TitleSafeArea.Center.Y), 80.0f);

			//put the red circle on the right of the screen
			_circle2.Initialize(graphics.GraphicsDevice.Viewport.TitleSafeArea.Center, 80.0f);

			_clock.Start();

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//Before the game starts, add all the points to the camera
			AddToCamera();

			//Force the camera to the screen. This will start the camera in the correct spot instead of it doing a quick zoom to the circles.
			_camera.ForceToScreen();
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
				Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
#if !__IOS__
				this.Exit();
#endif
			}

			//update the timer
			_clock.Update(gameTime);

			//update the input
			_inputState.Update();
			_inputWrapper.Update(_inputState, false);

			//move the circle
			float movespeed = 1600.0f;

			//check veritcal movement
			if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Up))
			{
				_circle1.Translate(0.0f, -movespeed * _clock.TimeDelta);
			}
			else if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Down))
			{
				_circle1.Translate(0.0f, movespeed * _clock.TimeDelta);
			}

			//check horizontal movement
			if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Forward))
			{
				_circle1.Translate(movespeed * _clock.TimeDelta, 0.0f);
			}
			else if (_inputWrapper.Controller.CheckKeystrokeHeld(EKeystroke.Back))
			{
				_circle1.Translate(-movespeed * _clock.TimeDelta, 0.0f);
			}

			//add camera shake?
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.A))
			{
				_camera.AddCameraShake(0.5f);
			}
			else if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.B))
			{
				_camera.AddCameraShake(2.0f, 0.1f, 0.1f);
			}
			else if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.Y))
			{
				_camera.AddCameraShake(0f, 0.2f, 0.05f);
			}
			else if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.X))
			{
				_camera.StopCameraShake();
			}

			//update the camera
			_camera.Update(_clock);

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			AddToCamera();

			//update all the matrices of the camera before we start drawing
			_camera.BeginScene(false);

			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				null, null, null, null,
				_camera.TranslationMatrix);

			//draw the players circle in green
			var circlePrim = new Primitive(graphics.GraphicsDevice, spriteBatch);
			circlePrim.Circle(_circle1.Pos, _circle1.Radius, Color.Green);

			//draw the stationary circle in red
			circlePrim.Circle(_circle2.Pos, _circle2.Radius, Color.Red);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void AddToCamera()
		{
			//Add all our points to the camera
			AddCircleToCamera(_circle1);
			AddCircleToCamera(_circle2);
		}

		private void AddCircleToCamera(Circle circle)
		{
			_camera.AddPoint(circle.Pos + new Vector2(-circle.Radius, 0));
			_camera.AddPoint(circle.Pos + new Vector2(circle.Radius, 0f));
			_camera.AddPoint(circle.Pos + new Vector2(0, -circle.Radius));
			_camera.AddPoint(circle.Pos + new Vector2(0, circle.Radius));
		}

		#endregion //Members
	}
}

