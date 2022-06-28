using GameTimer;
using System;

namespace CameraBuddy
{
    public class Shaker : IShaker
    {
        #region Properties

		private float ShakeDelta { get; set; } 

        /// <summary>
		/// this is whether to shake the camera left or right
		/// </summary>
		public bool ShakeLeft { get; set; }

		/// <summary>
		/// This times the entire camera shake from start to finish
		/// </summary>
		protected CountdownTimer WholeTimer { get; set; }

		/// <summary>
		/// This times an individual shake
		/// </summary>
		public CountdownTimer ShakeTimer { get; protected set; }

		/// <summary>
		/// How hard to shake the camera.  1.0f for normal amount
		/// </summary>
		public float ShakeAmount { get; protected set; }

		private bool EndlessShake { get; set; }

		protected bool IsShaking => EndlessShake || (!WholeTimer.Paused && WholeTimer.HasTimeRemaining);

		#endregion //Properties

		#region Methods

		public Shaker()
		{
			ShakeLeft = true;
			WholeTimer = new CountdownTimer();
			ShakeTimer = new CountdownTimer();
			ShakeAmount = 1f;
		}

		public void Update(GameClock clock)
        {
			WholeTimer.Update(clock);
			ShakeTimer.Update(clock);

			if (IsShaking && !ShakeTimer.Paused && !ShakeTimer.HasTimeRemaining)
            {
				ShakeLeft = !ShakeLeft;
				ShakeTimer.Start(ShakeDelta);
            }
		}

        public void AddShake(float length, float delta, float amount)
        {
			ShakeDelta = delta;

			//shake the opposite direction
			ShakeLeft = !ShakeLeft;

			//start timing the shake
			if (WholeTimer.HasTimeRemaining)
			{
				ShakeAmount = Math.Max(ShakeAmount, amount);

				if (WholeTimer.RemainingTime < length)
				{
					WholeTimerStart(length);
				}
			}
			else
			{
				ShakeAmount = amount;
				WholeTimerStart(length);
			}

			//start timing the delta
			ShakeTimer.Start(delta);
		}

		public void SetShake(float length, float delta, float amount)
		{
			ShakeDelta = delta;

			//shake the opposite direction
			ShakeLeft = !ShakeLeft;

			//start timing the shake
			ShakeAmount = amount;
			WholeTimerStart(length);

			//start timing the delta
			ShakeTimer.Start(delta);
		}

		private void WholeTimerStart(float length)
        {
			if (length > 0)
			{
				EndlessShake = false;
				WholeTimer.Start(length);
			}
			else
            {
				EndlessShake = true;
			}
        }

		public void StopShake()
		{
			ShakeTimer.Stop();
			WholeTimer.Stop();
			EndlessShake = false;
		}

        #endregion //Methods
    }
}
