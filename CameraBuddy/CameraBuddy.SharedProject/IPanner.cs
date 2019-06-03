using Microsoft.Xna.Framework;

namespace CameraBuddy
{
	public interface IPanner
	{
		/// <summary>
		/// how fast the camera can pan
		/// </summary>
		float PanSpeed { get; set; }

		/// <summary>
		/// the amount to multiply the rotation of during the shake
		/// </summary>
		float ShakePan { get; set; }

		/// <summary>
		/// A perecentage between 0.0-1.0 of how much padding to add to the screen 
		/// </summary>
		float LeftPadding { get; set; }

		/// <summary>
		/// A perecentage between 0.0-1.0 of how much padding to add to the screen 
		/// </summary>
		float RightPadding { get; set; }

		/// <summary>
		/// A perecentage between 0.0-1.0 of how much padding to add to the screen 
		/// </summary>
		float TopPadding { get; set; }

		/// <summary>
		/// A perecentage between 0.0-1.0 of how much padding to add to the screen 
		/// </summary>
		float BottomPadding { get; set; }

		/// <summary>
		/// The boundary we don't want the camera to leave!
		/// </summary>
		/// <value>The world.</value>
		Rectangle WorldBoundary { get; set; }

		/// <summary>
		/// This is a flag that can be used if your game doesn't have a world boundary
		/// </summary>
		/// <value><c>true</c> if ignore world boundary; otherwise, <c>false</c>.</value>
		bool IgnoreWorldBoundary { get; set; }

		/// <summary>
		/// teh position to use as the camera center
		/// </summary>
		Vector2 Center { get; }

		bool UseManualOffset { get; set; }

		Vector2 ManualOffset { get; set; }
	}
}
