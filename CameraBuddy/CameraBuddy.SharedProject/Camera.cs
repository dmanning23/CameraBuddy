using GameTimer;
using Microsoft.Xna.Framework;
using ResolutionBuddy;
using System;

namespace CameraBuddy
{
	public class Camera : ICamera
	{
		#region Properties

		/// <summary>
		/// flag used to reset the camera each frame
		/// </summary>
		protected bool CameraReset { get; set; }

		/// <summary>
		/// this is whether to shake the camera left or right
		/// </summary>
		protected bool ShakeLeft { get; set; }

		/// <summary>
		/// Clock used for camera movement and timing
		/// </summary>
		protected GameClock CameraClock { get; set; }

		/// <summary>
		/// This is the time the camera started shaking
		/// </summary>
		protected CountdownTimer ShakeTimer { get; set; }

		/// <summary>
		/// How hard to shake the camera.  1.0f for normal amount
		/// </summary>
		protected float ShakeAmount { get; set; }

		public Matrix TranslationMatrix { get; private set; }

		protected Scaler Scaler { get; set; }

		protected Panner Panner { get; set; }

		public float PanSpeed { get => Panner.PanSpeed; set => Panner.PanSpeed = value; }
		public float ShakePan { get => Panner.ShakePan; set => Panner.ShakePan = value; }
		public float LeftPadding { get => Panner.LeftPadding; set => Panner.LeftPadding = value; }
		public float RightPadding { get => Panner.RightPadding; set => Panner.RightPadding = value; }
		public float TopPadding { get => Panner.TopPadding; set => Panner.TopPadding = value; }
		public float BottomPadding { get => Panner.BottomPadding; set => Panner.BottomPadding = value; }
		public Rectangle WorldBoundary { get => Panner.WorldBoundary; set => Panner.WorldBoundary = value; }
		public bool IgnoreWorldBoundary { get => Panner.IgnoreWorldBoundary; set => Panner.IgnoreWorldBoundary = value; }
		public Vector2 Center { get => Panner.Center; }
		public float ScaleSpeed { get => Scaler.ScaleSpeed; set => Scaler.ScaleSpeed = value; }
		public float ShakeZoom { get => Scaler.ShakeZoom; set => Scaler.ShakeZoom = value; }
		public float Scale { get => Scaler.Scale; }
		public float? MinAutoScale { get => Scaler.MinAutoScale; set => Scaler.MinAutoScale = value; }
		public float? MaxAutoScale { get => Scaler.MaxAutoScale; set => Scaler.MaxAutoScale = value; }
		public float? MinManualScale { get => Scaler.MinManualScale; set => Scaler.MinManualScale = value; }
		public float? MaxManualScale { get => Scaler.MaxManualScale; set => Scaler.MaxManualScale = value; }
		public bool UseManualScale { get => Scaler.UseManualScale; set => Scaler.UseManualScale = value; }
		public float ManualScale { get => Scaler.ManualScale; set => Scaler.ManualScale = value; }

		public bool UseManualOffset { get => Panner.UseManualOffset; set => Panner.UseManualOffset = value; }
		public Vector2 ManualOffset { get => Panner.ManualOffset; set => Panner.ManualOffset = value; }

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="CameraBuddy.Camera"/> class.
		/// </summary>
		public Camera()
		{
			Scaler = new Scaler();
			Panner = new Panner();

			ShakeLeft = true;
			CameraReset = true;
			CameraClock = new GameClock();
			ShakeTimer = new CountdownTimer();

			ShakeAmount = 1.0f;

			TranslationMatrix = Matrix.Identity;
		}

		public void Update(GameClock clock)
		{
			CameraReset = true;
			CameraClock.Update(clock);
			ShakeTimer.Update(CameraClock);
		}

		public void AddPoint(Vector2 point)
		{
			Panner.AddPoint(point, CameraReset);

			//Set the camera reset to false after adding a point
			CameraReset = false;
		}

		public void BeginScene(bool forceToViewport)
		{
			MoveToViewport(forceToViewport);

			//setup the scale matrix
			var scaleMatrix = Matrix.CreateScale(Scaler.Scale);

			//Get the translation vectors
			var translationVect = new Vector2(
				(Resolution.TitleSafeArea.Width / 2f) - (Scaler.Scale * Panner.Center.X),
				(Resolution.TitleSafeArea.Height / 2f) - (Scaler.Scale * Panner.Center.Y));

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
			Panner.MoveToViewport(forceToViewport, CameraClock);

			//add the camera spin
			Panner.AddShake(ShakeAmount, ShakeLeft, ShakeTimer);

			//Constrain the camera to stay in teh game world... or dont.
			var newScale = Panner.Constrain();

			Scaler.UpdateScale(forceToViewport, newScale, CameraClock);

			//set teh camer position to be the center of the desired rectangle;
			Panner.UpdateCenter(Scaler.Scale);

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

				Scaler.AddShake(ShakeAmount, ShakeTimer);
			}
		}

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

		public void ForceToScreen()
		{
			BeginScene(true);
		}

		#endregion //Methods
	}
}