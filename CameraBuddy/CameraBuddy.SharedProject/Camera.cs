using GameTimer;
using Microsoft.Xna.Framework;
using ResolutionBuddy;
using System;

namespace CameraBuddy
{
	public class Camera
	{
		#region Fields

		/// <summary>
		/// the amount to zoom during a camera shake
		/// this is a number between 0.0 and 1.0
		/// smaller = more shake
		/// </summary>
		private const float CAMERA_SHAKE_ZOOM = 0.02f;

		/// <summary>
		/// the amount to multiply the rotation of during the shake
		/// </summary>
		private const float CAMERA_SHAKE_ROTATE = 25.0f;

		/// <summary>
		/// how fast the camera can pan
		/// </summary>
		private const float CAMERA_SPEED = 10.0f;

		private const float SCALE_SPEED = 20.0f;

		#endregion //Fields

		#region Properties

		/// <summary>
		/// flag used to reset the camera each frame
		/// </summary>
		private bool CameraReset { get; set; }

		/// <summary>
		/// this is whether to shake the camera left or right
		/// </summary>
		protected bool ShakeLeft { get; set; }

		/// <summary>
		/// Clock used for camera movement and timing
		/// </summary>
		private GameClock CameraClock { get; set; }

		/// <summary>
		/// This is the time the camera started shaking
		/// </summary>
		protected CountdownTimer ShakeTimer { get; set; }

		/// <summary>
		/// How hard to shake the camera.  1.0f for normal amount
		/// </summary>
		protected float ShakeAmount { get; set; }

		/// <summary>
		/// teh position to use as the camera center
		/// </summary>
		private Vector2 _origin;
		public Vector2 Origin
		{
			get
			{
				return _origin;
			}
			set
			{
				_origin = value;
			}
		}

		/// <summary>
		/// The amount to scale the rendering
		/// </summary>
		public float Scale { get; set; }

		/// <summary>
		/// the previous value of Scale
		/// </summary>
		private float PrevScale { get; set; }

		/// <summary>
		/// The boundary we don't want the camera to leave!
		/// </summary>
		/// <value>The world.</value>
		public Rectangle WorldBoundary { get; set; }

		/// <summary>
		/// This is a flag that can be used if your game doesn't have a world boundary
		/// </summary>
		/// <value><c>true</c> if ignore world boundary; otherwise, <c>false</c>.</value>
		public bool IgnoreWorldBoundary { get; set; }

		/// <summary>
		/// translation matrix for the camera
		/// </summary>
		public Matrix TranslationMatrix { get; private set; }

		//The previous coordinate system
		private float PrevLeft { get; set; }
		private float PrevRight { get; set; }
		private float PrevTop { get; set; }
		private float PrevBottom { get; set; }

		//the target coordinate system, will fit every object onto the screen.
		public float Left { get; private set; }
		public float Right { get; private set; }
		public float Top { get; private set; }
		public float Bottom { get; private set; }

		private float ShakeZoom
		{
			get
			{
				return CAMERA_SHAKE_ZOOM * ShakeAmount;
			}
		}

		private float ShakeRotate
		{
			get
			{
				return CAMERA_SHAKE_ROTATE * ShakeAmount;
			}
		}

		/// <summary>
		/// A perecentage between 0.0-1.0 of how much padding to add to the screen 
		/// </summary>
		public float LeftPadding { get; set; }
		public float RightPadding { get; set; }
		public float TopPadding { get; set; }
		public float BottomPadding { get; set; }

		public float? MinScale { get; set; }
		public float? MaxScale { get; set; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="CameraBuddy.Camera"/> class.
		/// </summary>
		public Camera()
		{
			Scale = 1.0f;
			PrevScale = 1.0f;
			Origin = Vector2.Zero;

			PrevLeft = 0.0f;
			PrevRight = 0.0f;
			PrevTop = 0.0f;
			PrevBottom = 0.0f;
			Left = 0.0f;
			Right = 0.0f;
			Top = 0.0f;
			Bottom = 0.0f;
			ShakeLeft = true;
			CameraReset = true;
			CameraClock = new GameClock();
			ShakeTimer = new CountdownTimer();
			WorldBoundary = new Rectangle(0, 0, 0, 0);
			IgnoreWorldBoundary = false;
			ShakeAmount = 1.0f;

			TranslationMatrix = Matrix.Identity;
		}

		/// <summary>
		/// Called to reset the camera view each frame
		/// Make sure to call this before any pionts are added to the scene
		/// </summary>
		public void Update(GameClock clock)
		{
			CameraReset = true;
			CameraClock.Update(clock);
			ShakeTimer.Update(CameraClock);
		}

		/// <summary>
		/// Add a point to the scene.
		/// This gets called multiple times during the update loop to add all the points we want in frame.
		/// Later, it will calculate a scale and translation matrix to fit them all on screen.
		/// </summary>
		/// <param name="point">the point that we want to be seen in the camera</param>
		public void AddPoint(Vector2 point)
		{
			if (CameraReset)
			{
				//first point this frame, reset the viewport
				Left = point.X;
				Right = point.X;
				Bottom = point.Y;
				Top = point.Y;

				CameraReset = false;
			}
			else
			{
				//check the left & right
				if (point.X < Left)
				{
					Left = point.X;
				}
				else if (point.X > Right)
				{
					Right = point.X;
				}

				//check the top & bottom
				if (point.Y < Top)
				{
					Top = point.Y;
				}
				else if (point.Y > Bottom)
				{
					Bottom = point.Y;
				}
			}
		}

		/// <summary>
		/// Call this before spritebatch.begin is called to set the matrixes up
		/// After this is called, get the TranslationMatrix and pass it into spritebatch.begin
		/// </summary>
		/// <param name="forceToViewport">Can pass "true" to force it to snap to the required matrix, pass "false" to slowly transition</param>
		public void BeginScene(bool forceToViewport)
		{
			MoveToViewport(forceToViewport);

			//setup the scale matrix
			var scaleMatrix = Matrix.CreateScale(Scale);

			//Get the translation vectors
			var translationVect = new Vector2(
				(Resolution.TitleSafeArea.Width / 2f) - (Scale * Origin.X),
				(Resolution.TitleSafeArea.Height / 2f) - (Scale * Origin.Y));

			//setup the translation matrix
			TranslationMatrix = Matrix.CreateTranslation(translationVect.X, translationVect.Y, 0f);
			TranslationMatrix = Matrix.Multiply(scaleMatrix, TranslationMatrix);
		}

		/// <summary>
		/// Moves to viewport.
		/// Can pass "true" to force it to snap to the required matrix, pass "false" to slowly transition
		/// </summary>
		/// <param name="forceToViewport">If set to <c>true</c> b force to viewport.</param>
		private void MoveToViewport(bool forceToViewport)
		{
			//Add the padding
			var vert = Bottom - Top;
			Top -= vert * TopPadding;
			Bottom += vert * BottomPadding;
			var horiz = Right - Left;
			Left -= horiz * LeftPadding;
			Right += horiz * RightPadding;

			//set the camera position 
			if (forceToViewport)
			{
				PrevLeft = Left;
				PrevRight = Right;
				PrevTop = Top;
				PrevBottom = Bottom;
			}
			else
			{
				//move the left
				var leftDiff = (((Left - PrevLeft) * CAMERA_SPEED) * CameraClock.TimeDelta);
				PrevLeft += leftDiff;
				Left = PrevLeft;

				//move the Right
				var rightDiff = (((Right - PrevRight) * CAMERA_SPEED) * CameraClock.TimeDelta);
				PrevRight += rightDiff;
				Right = PrevRight;

				//move the Top
				var topDiff = (((Top - PrevTop) * CAMERA_SPEED) * CameraClock.TimeDelta);
				PrevTop += topDiff;
				Top = PrevTop;

				//move the Bottom
				var bottomDiff = (((Bottom - PrevBottom) * CAMERA_SPEED) * CameraClock.TimeDelta);
				PrevBottom += bottomDiff;
				Bottom = PrevBottom;
			}

			//add the camera spin
			if (ShakeTimer.RemainingTime > 0.0f)
			{
				//figure out the proper rotation for the camera shake
				var shakeX = (ShakeRotate *
					(float)Math.Sin(
					((ShakeTimer.CurrentTime * (2.0f * Math.PI)) /
					ShakeTimer.CountdownLength)));

				var shakeY = (ShakeRotate *
					(float)Math.Cos(
					((ShakeTimer.CurrentTime * (2.0f * Math.PI)) /
					ShakeTimer.CountdownLength)));

				shakeX = shakeX * (ShakeLeft ? -1.0f : 1.0f);

				Left += shakeX;
				Right += shakeX;

				Top += shakeY;
				Bottom += shakeY;
			}

			//Constrain the camera to stay in teh game world... or dont.
			if (!IgnoreWorldBoundary)
			{
				//hold all points inside the game world!
				if (Top < WorldBoundary.Top)
				{
					Top = WorldBoundary.Top;
				}

				if (Bottom > WorldBoundary.Bottom)
				{
					Bottom = WorldBoundary.Bottom;
				}

				if (Left < WorldBoundary.Left)
				{
					Left = WorldBoundary.Left;
				}

				if (Right > WorldBoundary.Right)
				{
					Right = WorldBoundary.Right;
				}
			}
			else
			{
				//Let the camera roam around wherever it wants
				WorldBoundary = new Rectangle(
					(int)Left,
					(int)Top,
					(int)(Right - Left),
					(int)(Bottom - Top));
			}

			//check if we need to zoom to fit either horizontal or vertical

			//get the current aspect ratio
			var screenAspectRatio = (float)Resolution.ScreenArea.Width / (float)Resolution.ScreenArea.Height;

			//get the current target aspect ratio
			var width = Right - Left;
			var height = Bottom - Top;
			var aspectRatio = width / height;

			/*
			Here's the formula:

			X   A+i
			- = ---
			Y   B+j

			where x/y is the screen aspect ratio and A+i/B+j is the current target.
			*/

			//If the current target is less than gl aspect ratio, B is too big (too tall)
			var newScale = 1.0f;
			if (aspectRatio < screenAspectRatio)
			{
				//j = 0.0f, figure for i
				//increase the A (width) to fit the aspect ratio
				var totalAdjustment = (screenAspectRatio * height) - width;

				//don't let it go past the walls
				var adjustedRight = (Right + (totalAdjustment / 2.0f));
				var adjustedLeft = (Left - (totalAdjustment / 2.0f));

				var offRightWall = adjustedRight > WorldBoundary.Right;
				var offLeftWall = adjustedLeft < WorldBoundary.Left;

				if (IgnoreWorldBoundary || (!offRightWall && !offLeftWall))
				{
					//those new limits are fine!
					Right = adjustedRight;
					Left = adjustedLeft;
				}
				else if (offRightWall && offLeftWall)
				{
					//both limits are screwed up
					Right = WorldBoundary.Right;
					Left = WorldBoundary.Left;
				}
				else
				{
					//have to adjust to keep the camera on the board

					if (offRightWall && !offLeftWall)
					{
						//put the right at the wall
						var fRightAdjustment = adjustedRight - WorldBoundary.Right;
						Right = WorldBoundary.Right;
						Left -= ((totalAdjustment / 2.0f) + fRightAdjustment);
					}
					else if (!offRightWall && offLeftWall)
					{
						//put the left at the wall
						var fLeftAdjustment = WorldBoundary.Left - adjustedLeft;
						Left = WorldBoundary.Left;
						Right += (totalAdjustment / 2.0f) + fLeftAdjustment;
					}

					//ok double check those limits are still good after being adjusted
					if (Right > WorldBoundary.Right)
					{
						Right = WorldBoundary.Right;
					}
					if (Left < WorldBoundary.Left)
					{
						Left = WorldBoundary.Left;
					}
				}

				newScale = Resolution.ScreenArea.Width / (Right - Left);
			}
			//If the current target is greater than gl aspect ratio, A is too big (too wide)
			else if (aspectRatio > screenAspectRatio)
			{
				//i = 0.0f, figure for j
				//increase the B (height) to fit the aspect ratio
				var totalAdjustment = ((width * (float)Resolution.ScreenArea.Height) / (float)Resolution.ScreenArea.Width) - height;

				//if that moves below the bottom, add it all to the top
				var adjustedBottom = (Bottom + (totalAdjustment / 2.0f)); //this is where the bottom will be
				var adjustedCeiling = (Top - (totalAdjustment / 2.0f)); //this is where ceiling will be

				var offBottomWall = adjustedBottom > WorldBoundary.Bottom;
				var offCeilingWall = adjustedCeiling < WorldBoundary.Top;

				if (IgnoreWorldBoundary || (!offBottomWall && !offCeilingWall))
				{
					//those new limits are fine!
					Top = adjustedCeiling;
					Bottom = adjustedBottom;
				}
				else if (offBottomWall && offCeilingWall)
				{
					//both limits are screwed up
					Bottom = WorldBoundary.Bottom;
					Top = WorldBoundary.Top;
				}
				else
				{
					//have to adjust to keep the camera on the board

					if (offBottomWall && !offCeilingWall)
					{
						//put the bottom at the floor
						var fBottomAdjustment = adjustedBottom - WorldBoundary.Bottom;
						Bottom = WorldBoundary.Bottom;
						Top -= ((totalAdjustment / 2.0f) + fBottomAdjustment);
					}
					else if (!offBottomWall && offCeilingWall)
					{
						//put the top at the ceiling
						var fTopAdjustment = WorldBoundary.Top - adjustedCeiling;
						Top = WorldBoundary.Top;
						Bottom += (totalAdjustment / 2.0f) + fTopAdjustment;
					}

					//ok double check those limits are still good after being adjusted
					if (Bottom > WorldBoundary.Bottom)
					{
						Bottom = WorldBoundary.Bottom;
					}
					if (Top < WorldBoundary.Top)
					{
						Top = WorldBoundary.Top;
					}
				}

				newScale = Resolution.ScreenArea.Height / (Bottom - Top);
			}

			UpdateScale(forceToViewport, newScale);

			//set teh camer position to be the center of the desired rectangle;
			_origin.X = ((Left + Right) / 2.0f) - (Resolution.TitleSafeArea.Left / Scale);
			_origin.Y = ((Top + Bottom) / 2.0f) - (Resolution.TitleSafeArea.Top / Scale);

			if (ShakeTimer.RemainingTime > 0.0f)
			{
				/*
				okay the formula for camera shake rotation:

				sin(

				current		=		x 
				-------			--------
				shakedellta			2pi
				
				)

				multiply by a number 0 > 1 to increase amplitude of the camera rotate
				*/

				//figure how much camera shake to add to the zoom
				var shakeZoom = ((Scale * ShakeZoom) *
					(float)Math.Sin(
					((ShakeTimer.CurrentTime * Math.PI) /
					ShakeTimer.CountdownLength)));
				Scale *= 1.0f + shakeZoom;
			}
		}

		/// <summary>
		/// set the camera scale
		/// </summary>
		/// <param name="forceToViewport"></param>
		/// <param name="newScale"></param>
		private void UpdateScale(bool forceToViewport, float newScale)
		{
			if (forceToViewport)
			{
				Scale = newScale;
			}
			else
			{
				Scale += (((newScale - PrevScale) * SCALE_SPEED) * CameraClock.TimeDelta);
			}

			if (MaxScale.HasValue)
			{
				Scale = Math.Min(Scale, MaxScale.Value);
			}

			if (MinScale.HasValue)
			{
				Scale = Math.Max(Scale, MinScale.Value);
			}

			PrevScale = Scale;
		}

		/// <summary>
		/// add some camera shaking!
		/// </summary>
		/// <param name="timeDelta">how long to shake the camera</param>
		/// <param name="amount">how hard to shake the camera</param>
		public void AddCameraShake(float timeDelta, float amount = 1.0f)
		{
			ShakeLeft = !ShakeLeft;

			if (ShakeTimer.HasTimeRemaining)
			{
				ShakeAmount = Math.Max(ShakeAmount, amount);

				if (ShakeTimer.RemainingTime < timeDelta)
				{
					ShakeTimer.Start(timeDelta);
				}
			}
			else
			{
				ShakeAmount = amount;
				ShakeTimer.Start(timeDelta);
			}
		}

		/// <summary>
		/// Forces to screen.  Same as doing BeginScene(true)
		/// </summary>
		public void ForceToScreen()
		{
			BeginScene(true);
		}

		#endregion //Methods
	}
}