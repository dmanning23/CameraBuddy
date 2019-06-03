using Microsoft.Xna.Framework;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraBuddy.Tests
{
	public class TestPanner : Panner
	{
	}

	[TestFixture]
	public class PannerTests
	{
		TestPanner panner;

		[SetUp]
		public void Setup()
		{
			panner = new TestPanner();
		}

		[Test]
		public void Construction_UseManualOffset()
		{
			panner.UseManualOffset.ShouldBeFalse();
		}

		[Test]
		public void Construction_ManualOffset()
		{
			panner.ManualOffset.X.ShouldBe(0f);
			panner.ManualOffset.Y.ShouldBe(0f);
		}

		[Test]
		public void SetManuOffset()
		{
			panner.ManualOffset = new Vector2(2f, 3f);
			panner.UseManualOffset.ShouldBeTrue();
		}

		[Test]
		public void SetUseManuOffset()
		{
			panner.ManualOffset = new Vector2(2f, 3f);
			panner.UseManualOffset = false;
			panner.ManualOffset.X.ShouldBe(0f);
			panner.ManualOffset.Y.ShouldBe(0f);
		}
	}
}
