using GameTimer;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraBuddy.Tests
{
	[TestFixture]
	public class ScalerTests
	{
		Scaler scaler;

		[SetUp]
		public void Setup()
		{
			scaler = new Scaler();
		}

		[Test]
		public void Construction_Scale()
		{
			scaler.Scale.ShouldBe(1f);
		}

		[Test]
		public void Construction_DesiredScale()
		{
			scaler.DesiredScale.ShouldBe(1f);
		}

		[Test]
		public void Construction_UseDesiredScale()
		{
			scaler.UseDesiredScale.ShouldBe(false);
		}

		[Test]
		public void Construction_MinScale()
		{
			scaler.MinScale.HasValue.ShouldBeFalse();
		}

		[Test]
		public void Construction_MaxScale()
		{
			scaler.MaxScale.HasValue.ShouldBeFalse();
		}

		[TestCase(1f, 2f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 0f, 1f)]
		[TestCase(1f, 2f, 3f, 2f)]
		public void SetDesiredScale(float minScale, float maxScale, float desiredScale, float expectedValue)
		{
			scaler.MinScale = minScale;
			scaler.MaxScale = maxScale;
			scaler.DesiredScale = desiredScale;

			scaler.DesiredScale.ShouldBe(expectedValue);
		}

		[TestCase(1f, 2f, 1.5f, 1.5f)]
		[TestCase(1f, 2f, 0f, 1f)]
		[TestCase(1f, 2f, 3f, 2f)]
		public void ForceToViewport(float minScale, float maxScale, float newScale, float expectedValue)
		{
			scaler.MinScale = minScale;
			scaler.MaxScale = maxScale;

			scaler.UpdateScale(true, newScale, new GameClock());

			scaler.Scale.ShouldBe(expectedValue);
		}
	}
}
