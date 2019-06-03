
namespace CameraBuddy
{
	public interface IScaler
	{
		/// <summary>
		/// How fast to zoom in/out
		/// </summary>
		float ScaleSpeed { get; set; }

		/// <summary>
		/// the amount to zoom during a camera shake
		/// this is a number between 0.0 and 1.0
		/// smaller = more shake
		/// </summary>
		float ShakeZoom { get; set; }

		/// <summary>
		/// The amount to scale the rendering
		/// </summary>
		float Scale { get; }

		/// <summary>
		/// The Lowest possible scale (how far to zoom in)
		/// </summary>
		float? MinAutoScale { get; set; }

		/// <summary>
		/// The highest possible scale (how far to zoom out)
		/// </summary>
		float? MaxAutoScale { get; set; }

		/// <summary>
		/// How far out the user can manually zoom the camera
		/// </summary>
		float? MinManualScale { get; set; }

		/// <summary>
		/// How far in the user can manually zoom the camera
		/// </summary>
		float? MaxManualScale { get; set; }

		/// <summary>
		/// Whether or not to use the manual scale value.
		/// </summary>
		bool UseManualScale { get; set; }

		/// <summary>
		/// Manually set the current scale of the camera
		/// </summary>
		float ManualScale { get; set; }
	}
}
