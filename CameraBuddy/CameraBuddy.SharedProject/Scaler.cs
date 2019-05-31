using GameTimer;
using System;

namespace CameraBuddy
{
	/// <summary>
	/// This object is for managing the scale of the camera
	/// </summary>
	public class Scaler
	{
		#region Properties

		/// <summary>
		/// How fast to zoom in/out
		/// </summary>
		public float ScaleSpeed { get; set; }

		/// <summary>
		/// the amount to zoom during a camera shake
		/// this is a number between 0.0 and 1.0
		/// smaller = more shake
		/// </summary>
		public float ShakeZoom { get; set; }

		private float _scale;
		/// <summary>
		/// The amount to scale the rendering
		/// </summary>
		public float Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = UpdateWithMinMax(value);
			}
		}

		/// <summary>
		/// the previous value of Scale
		/// </summary>
		protected float PrevScale { get; set; }

		/// <summary>
		/// The Lowest possible scale (how far to zoom in)
		/// </summary>
		public float? MinScale { get; set; }

		/// <summary>
		/// The highest possible scale (how far to zoom out)
		/// </summary>
		public float? MaxScale { get; set; }

		public bool UseDesiredScale { get; set; }

		private float _desiredScale;
		/// <summary>
		/// Manually set the current scale of the camera
		/// </summary>
		public float DesiredScale
		{
			get
			{
				return _desiredScale;
			}
			set
			{
				_desiredScale = UpdateWithMinMax(value);
			}
		}

		#endregion //Properties

		#region Methods

		public Scaler()
		{
			ScaleSpeed = 20f;
			ShakeZoom = 0.02f;

			Scale = 1f;
			PrevScale = 1f;
			DesiredScale = 1f;
			UseDesiredScale = false;
		}

		private float UpdateWithMinMax(float value)
		{
			if (!MaxScale.HasValue && !MinScale.HasValue)
			{
				return value;
			}
			else if (MaxScale.HasValue && MinScale.HasValue)
			{
				return Math.Max(Math.Min(value, MaxScale.Value), MinScale.Value);
			}
			else if (MaxScale.HasValue)
			{
				return Math.Min(value, MaxScale.Value);
			}
			else if (MinScale.HasValue)
			{
				return Math.Max(value, MinScale.Value);
			}

			return value;
		}

		/// <summary>
		/// Set the scale, called from the camera
		/// </summary>
		/// <param name="forceToViewport"></param>
		/// <param name="newScale"></param>
		public void UpdateScale(bool forceToViewport, float newScale, GameClock clock)
		{
			if (forceToViewport)
			{
				Scale = newScale;
			}
			else if (UseDesiredScale)
			{
				Scale += (((DesiredScale - PrevScale) * ScaleSpeed) * clock.TimeDelta);
			}
			else
			{
				Scale += (((newScale - PrevScale) * ScaleSpeed) * clock.TimeDelta);
			}

			if (!UseDesiredScale || forceToViewport)
			{
				DesiredScale = Scale;
			}

			PrevScale = Scale;
		}

		public void AddShake(float shakeAmount, CountdownTimer shakeTimer)
		{
			shakeAmount = ShakeZoom * shakeAmount;

			//figure how much camera shake to add to the zoom
			var shakeZoom = ((Scale * shakeAmount) *
					(float)Math.Sin(
					((shakeTimer.CurrentTime * Math.PI) /
					shakeTimer.CountdownLength)));
			Scale *= 1.0f + shakeZoom;
		}

		#endregion //Methods
	}
}
