using GameTimer;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraBuddy.Tests
{
	public class TestScaler : Scaler
	{
		public float TestPrevScale
		{
			get => PrevScale;
			set => PrevScale = value;
		}

		public float TestScale
		{
			set => Scale = value;
		}
	}

	[TestFixture]
	public class ScalerTests
	{
		TestScaler scaler;

		[SetUp]
		public void Setup()
		{
			scaler = new TestScaler();
		}

		[Test]
		public void Construction_Scale()
		{
			scaler.Scale.ShouldBe(1f);
		}

		[Test]
		public void Construction_PrevScale()
		{
			scaler.TestPrevScale.ShouldBe(1f);
		}


		[Test]
		public void Construction_DesiredScale()
		{
			scaler.ManualScale.ShouldBe(1f);
		}

		[Test]
		public void Construction_UseDesiredScale()
		{
			scaler.UseManualScale.ShouldBe(false);
		}

		[Test]
		public void Construction_MinScale()
		{
			scaler.MinAutoScale.HasValue.ShouldBeFalse();
		}

		[Test]
		public void Construction_MaxScale()
		{
			scaler.MaxAutoScale.HasValue.ShouldBeFalse();
		}

		public void SetDesiredScale_FlipsFlag()
		{
			scaler.ManualScale = 2f;
			scaler.UseManualScale.ShouldBeTrue();
		}

		public void SetFlag_SetsDesiredScale()
		{
			scaler.ManualScale = 2f;
			scaler.TestScale = 4f;

			scaler.UseManualScale = false;
			scaler.ManualScale.ShouldBe(4f);
		}

		[TestCase(1f, 2f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 0f, 1f)]
		[TestCase(1f, 2f, 3f, 2f)]
		public void SetDesiredScale(float minScale, float maxScale, float desiredScale, float expectedValue)
		{
			scaler.MinManualScale = minScale;
			scaler.MaxManualScale = maxScale;
			scaler.ManualScale = desiredScale;

			scaler.ManualScale.ShouldBe(expectedValue);
		}

		[TestCase(1f, 2f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 0f, 1f)]
		[TestCase(1f, 2f, 3f, 2f)]
		[TestCase(null, 2f, 3f, 2f)]
		[TestCase(1f, null, 3f, 3f)]
		[TestCase(null, 2f, 0f, 0f)]
		[TestCase(1f, null, 0f, 1f)]
		[TestCase(null, null, 3f, 3f)]
		[TestCase(null, null, 0f, 0f)]
		public void SetScale(float? minScale, float? maxScale, float scale, float expectedValue)
		{
			scaler.MinAutoScale = minScale;
			scaler.MaxAutoScale = maxScale;
			scaler.TestScale = scale;

			scaler.Scale.ShouldBe(expectedValue);
		}

		[TestCase(1f, 2f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 0f, 1f)]
		[TestCase(1f, 2f, 3f, 2f)]
		[TestCase(null, 2f, 3f, 2f)]
		[TestCase(1f, null, 3f, 3f)]
		[TestCase(null, 2f, 0f, 0f)]
		[TestCase(1f, null, 0f, 1f)]
		[TestCase(null, null, 3f, 3f)]
		[TestCase(null, null, 0f, 0f)]
		public void ForceToViewport(float? minScale, float? maxScale, float newScale, float expectedValue)
		{
			scaler.MinAutoScale = minScale;
			scaler.MaxAutoScale = maxScale;

			scaler.UpdateScale(true, newScale, new GameClock());

			scaler.Scale.ShouldBe(expectedValue);
		}

		[Test]
		public void UpdatePrevScale()
		{
			scaler.ScaleSpeed = 1f;
			var clock = new GameClock()
			{
				TimeDelta = 1f,
			};

			scaler.UpdateScale(true, 2f, clock);
			scaler.TestPrevScale.ShouldBe(2f);
			scaler.UpdateScale(true, 3f, clock);
			scaler.TestPrevScale.ShouldBe(3f);
		}

		[Test]
		public void UpdateScale()
		{
			scaler.ScaleSpeed = 1f;
			var clock = new GameClock()
			{
				TimeDelta = 1f,
			};

			scaler.UpdateScale(false, 3f, clock);
			scaler.Scale.ShouldBe(3f);
			scaler.UpdateScale(false, 5f, clock);
			scaler.Scale.ShouldBe(5f);
		}

		[Test]
		public void UpdateScale_Slower()
		{
			scaler.ScaleSpeed = 0.5f;
			var clock = new GameClock()
			{
				TimeDelta = 1f,
			};

			scaler.UpdateScale(false, 3f, clock);
			scaler.Scale.ShouldBe(2f);
			scaler.UpdateScale(false, 5f, clock);
			scaler.Scale.ShouldBe(3.5f);
		}

		[TestCase(1f, 2f, 3f, 4f, 2.5f, 2.5f)]
		[TestCase(1f, 2f, 3f, 4f, 1.5f, 2f)]
		[TestCase(1f, 2f, 3f, 4f, 3.5f, 3f)]
		public void Update_DifferentMinMax(float? minManual, float? minAuto, float? maxAuto, float? maxManual, float scale, float expectedValue)
		{
			scaler.ScaleSpeed = 1f;
			var clock = new GameClock()
			{
				TimeDelta = 1f,
			};
			scaler.MinManualScale = minManual;
			scaler.MinAutoScale = minAuto;
			scaler.MaxAutoScale = maxAuto;
			scaler.MaxManualScale = maxManual;

			scaler.UpdateScale(false, scale, clock);
			scaler.Scale.ShouldBe(expectedValue);
		}

		[TestCase(1f, 2f, 3f, 4f, 2.5f, 2.5f, 2.5f)]
		[TestCase(1f, 2f, 3f, 4f, 1.5f, 2.5f, 2.5f)]
		[TestCase(1f, 2f, 3f, 4f, 3.5f, 2.5f, 2.5f)]
		[TestCase(1f, 2f, 3f, 4f, 2.5f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 3f, 4f, 1.5f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 3f, 4f, 3.5f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 3f, 4f, 2.5f, 3.5f, 3.5f)]
		[TestCase(1f, 2f, 3f, 4f, 1.5f, 3.5f, 3.5f)]
		[TestCase(1f, 2f, 3f, 4f, 3.5f, 3.5f, 3.5f)]
		public void Update_DifferentMinMax_Desired(float? minManual, float? minAuto, float? maxAuto, float? maxManual, float scale, float desrired, float expectedValue)
		{
			scaler.ScaleSpeed = 1f;
			var clock = new GameClock()
			{
				TimeDelta = 1f,
			};
			scaler.MinManualScale = minManual;
			scaler.MinAutoScale = minAuto;
			scaler.MaxAutoScale = maxAuto;
			scaler.MaxManualScale = maxManual;
			scaler.ManualScale = desrired;

			scaler.UpdateScale(false, scale, clock);
			scaler.Scale.ShouldBe(expectedValue);
		}
	}
}
