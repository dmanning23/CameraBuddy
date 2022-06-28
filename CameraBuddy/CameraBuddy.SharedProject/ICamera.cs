using GameTimer;
using Microsoft.Xna.Framework;

namespace CameraBuddy
{
	public interface ICamera : IPanner, IScaler
	{
		/// <summary>
		/// translation matrix for the camera
		/// </summary>
		Matrix TranslationMatrix { get; }

		/// <summary>
		/// Called to reset the camera view each frame
		/// Make sure to call this before any pionts are added to the scene
		/// </summary>
		void Update(GameClock clock);

		/// <summary>
		/// Add a point to the scene.
		/// This gets called multiple times during the update loop to add all the points we want in frame.
		/// Later, it will calculate a scale and translation matrix to fit them all on screen.
		/// </summary>
		/// <param name="point">the point that we want to be seen in the camera</param>
		void AddPoint(Vector2 point);

		/// <summary>
		/// Call this before spritebatch.begin is called to set the matrixes up
		/// After this is called, get the TranslationMatrix and pass it into spritebatch.begin
		/// </summary>
		/// <param name="forceToViewport">Can pass "true" to force it to snap to the required matrix, pass "false" to slowly transition</param>
		void BeginScene(bool forceToViewport);

		/// <summary>
		/// add some camera shaking!
		/// </summary>
		/// <param name="timeDelta">how long to shake the camera</param>
		/// <param name="amount">how hard to shake the camera</param>

		void AddCameraShake(float timeDelta, float amount = 1.0f);

		void AddCameraShake(float length, float delta, float amount);

		void StopCameraShake();

		/// <summary>
		/// Forces to screen.  Same as doing BeginScene(true)
		/// </summary>

		void ForceToScreen();
	}
}
