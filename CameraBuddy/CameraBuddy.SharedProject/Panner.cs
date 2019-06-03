using GameTimer;
using Microsoft.Xna.Framework;
using ResolutionBuddy;
using System;

namespace CameraBuddy
{
	public class Panner
	{
		#region Properties

		public float PanSpeed { get; set; }

		
		public float ShakePan { get; set; }

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

		public float LeftPadding { get; set; }
		public float RightPadding { get; set; }
		public float TopPadding { get; set; }
		public float BottomPadding { get; set; }

		public Rectangle WorldBoundary { get; set; }

		public bool IgnoreWorldBoundary { get; set; }

		private Vector2 _center;
		public Vector2 Center
		{
			get => _center;
			set => _center = value;
		}

		private bool _useManualOffset;
		public bool UseManualOffset
		{
			get => _useManualOffset;
			set
			{
				_useManualOffset = value;
				if (!UseManualOffset)
				{
					_manualOffset = Vector2.Zero;
				}
			}
		}

		private Vector2 _manualOffset;
		public Vector2 ManualOffset
		{
			get
			{
				return _manualOffset;
			}
			set
			{
				_useManualOffset = true;
				_manualOffset = value;
			}
		}

		#endregion //Properties

		#region Methods

		public Panner()
		{
			PanSpeed = 10f;
			ShakePan = 25f;

			PrevLeft = 0.0f;
			PrevRight = 0.0f;
			PrevTop = 0.0f;
			PrevBottom = 0.0f;
			Left = 0.0f;
			Right = 0.0f;
			Top = 0.0f;
			Bottom = 0.0f;

			WorldBoundary = new Rectangle(0, 0, 0, 0);
			IgnoreWorldBoundary = false;

			Center = Vector2.Zero;
		}

		/// <summary>
		/// Add a point to the scene.
		/// This gets called multiple times during the update loop to add all the points we want in frame.
		/// Later, it will calculate a scale and translation matrix to fit them all on screen.
		/// </summary>
		/// <param name="point">the point that we want to be seen in the camera</param>
		public void AddPoint(Vector2 point, bool cameraReset)
		{
			if (cameraReset)
			{
				//first point this frame, reset the viewport
				Left = point.X;
				Right = point.X;
				Bottom = point.Y;
				Top = point.Y;
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

		public void MoveToViewport(bool forceToViewport, GameClock clock)
		{
			//Add the offset 
			Top += ManualOffset.Y;
			Bottom += ManualOffset.Y;
			Left += ManualOffset.X;
			Right += ManualOffset.X;

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
				var leftDiff = (((Left - PrevLeft) * PanSpeed) * clock.TimeDelta);
				PrevLeft += leftDiff;
				Left = PrevLeft;

				//move the Right
				var rightDiff = (((Right - PrevRight) * PanSpeed) * clock.TimeDelta);
				PrevRight += rightDiff;
				Right = PrevRight;

				//move the Top
				var topDiff = (((Top - PrevTop) * PanSpeed) * clock.TimeDelta);
				PrevTop += topDiff;
				Top = PrevTop;

				//move the Bottom
				var bottomDiff = (((Bottom - PrevBottom) * PanSpeed) * clock.TimeDelta);
				PrevBottom += bottomDiff;
				Bottom = PrevBottom;
			}
		}

		public void AddShake(float shakeAmount, bool shakeLeft, CountdownTimer shakeTimer)
		{
			shakeAmount = ShakePan * shakeAmount;

			//add the camera spin
			if (shakeTimer.HasTimeRemaining)
			{
				//figure out the proper rotation for the camera shake
				var shakeX = (shakeAmount *
					(float)Math.Sin(
					((shakeTimer.CurrentTime * (2.0f * Math.PI)) /
					shakeTimer.CountdownLength)));

				var shakeY = (shakeAmount *
					(float)Math.Cos(
					((shakeTimer.CurrentTime * (2.0f * Math.PI)) /
					shakeTimer.CountdownLength)));

				shakeX = shakeX * (shakeLeft ? -1.0f : 1.0f);

				Left += shakeX;
				Right += shakeX;

				Top += shakeY;
				Bottom += shakeY;
			}
		}

		public float Constrain()
		{
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

			return newScale;
		}

		public void UpdateCenter(float scale)
		{
			//set teh camer position to be the center of the desired rectangle;
			_center.X = ((Left + Right) / 2.0f) - (Resolution.TitleSafeArea.Left / scale);
			_center.Y = ((Top + Bottom) / 2.0f) - (Resolution.TitleSafeArea.Top / scale);
		}

		#endregion //Methods
	}
}
