using GameTimer;
using System;

namespace CameraBuddy
{
	/// <summary>
	/// This object is for managing the scale of the camera
	/// </summary>
	public class Scaler : IScaler
	{
		#region Properties

		public float ScaleSpeed { get; set; }

		public float ShakeZoom { get; set; }

		private float _scale;
		public float Scale
		{
			get
			{
				return _scale;
			}
			protected set
			{
				_scale = UpdateWithMinMax(value, MinAutoScale, MaxAutoScale);
			}
		}

		/// <summary>
		/// the previous value of Scale
		/// </summary>
		protected float PrevScale { get; set; }

		public float? MinAutoScale { get; set; }

		public float? MaxAutoScale { get; set; }

		public float? MinManualScale { get; set; }

		public float? MaxManualScale { get; set; }

		private bool _useManualScale;
		public bool UseManualScale
		{
			get => _useManualScale;
			set
			{
				_useManualScale = value;
				if (!UseManualScale)
				{
					_manualScale = Scale;
				}
			}
		}

		private float _manualScale;
		public float ManualScale
		{
			get
			{
				return _manualScale;
			}
			set
			{
				_useManualScale = true;
				_manualScale = UpdateWithMinMax(value, MinManualScale, MaxManualScale);
				PrevScale = _manualScale;
				_scale = _manualScale;
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
			UseManualScale = false;
		}

		protected float UpdateWithMinMax(float value, float? min, float? max)
		{
			if (!max.HasValue && !min.HasValue)
			{
				return value;
			}
			else if (max.HasValue && min.HasValue)
			{
				return Math.Max(Math.Min(value, max.Value), min.Value);
			}
			else if (max.HasValue)
			{
				return Math.Min(value, max.Value);
			}
			else if (min.HasValue)
			{
				return Math.Max(value, min.Value);
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
			else if (UseManualScale)
			{
				_scale += (((ManualScale - PrevScale) * ScaleSpeed) * clock.TimeDelta);
			}
			else
			{
				Scale += (((newScale - PrevScale) * ScaleSpeed) * clock.TimeDelta);
			}

			//I don't remember why this code is here?
			//if (!UseManualScale || forceToViewport)
			//{
			//	_manualScale = Scale;
			//}

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
