using GameTimer;
using Microsoft.Xna.Framework;
using Moq;
using NUnit.Framework;
using ResolutionBuddy;

namespace CameraBuddy.Tests
{
	[TestFixture]
    public class CameraTests
    {
		GameClock _clock;
		Camera _camera;

		[SetUp]
		public void Setup()
		{
			var resolution = new Mock<IResolution>();
			resolution.Setup(x => x.ScreenArea).Returns(new Rectangle(0, 0, 1280, 720));
			Resolution.Init(resolution.Object);

			_clock = new GameClock();
			_camera = new Camera()
			{
				IgnoreWorldBoundary = true
			};
		}

		[Test]
		public void Camera_construction()
		{
			_camera.Update(_clock);
			_camera.AddPoint(Vector2.Zero);
			_camera.AddPoint(new Vector2(1280, 720));

			_camera.BeginScene(true);
		}
	}
}
