using GameTimer;
using Microsoft.Xna.Framework;
using Moq;
using NUnit.Framework;
using ResolutionBuddy;
using Shouldly;

namespace CameraBuddy.Tests
{
	public class TestCamera : Camera
	{
		public bool GetShakeLeft
		{
			get
			{
				return ShakeLeft;
			}
			set
			{
				ShakeLeft = value;
			}
		}

		public CountdownTimer GetShakeTimer
		{
			get
			{
				return ShakeTimer;
			}
		}

		public float GetShakeAmount
		{
			get
			{
				return ShakeAmount;
			}
		}
	}

	[TestFixture]
	public class CameraTests
	{
		GameClock _clock;
		TestCamera _camera;

		[SetUp]
		public void Setup()
		{
			var resolution = new Mock<IResolution>();
			resolution.Setup(x => x.ScreenArea).Returns(new Rectangle(0, 0, 1280, 720));
			Resolution.Init(resolution.Object);

			_clock = new GameClock();
			_camera = new TestCamera()
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

		[Test]
		public void ShakeChangeDirection()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.GetShakeLeft.ShouldBeTrue();
		}

		[Test]
		public void DefaultShakeTime()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.GetShakeTimer.RemainingTime.ShouldBe(1f);
		}

		[Test]
		public void DefaultShakeAmount()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.GetShakeAmount.ShouldBe(2f);
		}

		[Test]
		public void LessShakeTime()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.AddCameraShake(0.5f, 2f);
			_camera.GetShakeTimer.RemainingTime.ShouldBe(1f);
		}

		[Test]
		public void MoreShakeTime()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.AddCameraShake(3f, 2f);
			_camera.GetShakeTimer.RemainingTime.ShouldBe(3f);
		}

		[Test]
		public void LessShakeAmount()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.AddCameraShake(1f, 0.5f);
			_camera.GetShakeAmount.ShouldBe(2f);
		}

		[Test]
		public void MoreShakeAmount()
		{
			_camera.GetShakeLeft = false;
			_camera.AddCameraShake(1f, 2f);
			_camera.AddCameraShake(1f, 3f);
			_camera.GetShakeAmount.ShouldBe(3f);
		}
	}
}
