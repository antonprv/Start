using NUnit.Framework;
using NMath = Framework.FastMath.Numerics.FMath;

namespace FastMath_Tests.Numerics
{
	public class FromEulerTests
	{
		[TestCase( 0.5f, 0.3f, -0.2f )]     // in-range (old code was already fine here)
		[TestCase( 4f, 0.3f, -0.2f )]       // yaw > π - old code silently broke here
		[TestCase( -7f, 1f, 2f )]           // well beyond [-π, π]
		[TestCase( 20f, -20f, 20f )]        // many full turns accumulated
		public void GMath_FromEulerZYX_AnyFiniteAngle_ProducesUnitQuaternion( float yaw, float pitch, float roll )
		{
			var q = NMath.FromEulerZYX( yaw, pitch, roll );
			float len = MathF.Sqrt( q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W );

			Assert.That( float.IsNaN( len ), Is.False );
			// ~1% tolerance: this is an approximate library (polynomial sin/cos,
			// ~0.47% worst-case error), not an exact one - see Trigonometry.cs.
			Assert.That( len, Is.EqualTo( 1f ).Within( 0.01f ),
				$"yaw={yaw} pitch={pitch} roll={roll} len={len}" );
		}

		[Test]
		public void GMath_FromEulerZYX_OutOfRangeYaw_MatchesExactFormula()
		{
			// Compares against the library's OWN documented ZYX formula
			// evaluated with exact MathF.Sin/Cos, rather than a different
			// library's Euler convention (which can disagree on handedness/
			// axis order even when both implementations are correct). This
			// isolates "did the range reduction fix work" from "do two
			// different libraries agree on what ZYX means".
			float yaw = 4f, pitch = 0.3f, roll = -0.2f;
			float hy = yaw * 0.5f, hp = pitch * 0.5f, hr = roll * 0.5f;
			float sy = MathF.Sin( hy ), cy = MathF.Cos( hy );
			float sp = MathF.Sin( hp ), cp = MathF.Cos( hp );
			float sr = MathF.Sin( hr ), cr = MathF.Cos( hr );

			float ex = sr * cp * cy - cr * sp * sy;
			float ey = cr * sp * cy + sr * cp * sy;
			float ez = cr * cp * sy - sr * sp * cy;
			float ew = cr * cp * cy + sr * sp * sy;

			var q = NMath.FromEulerZYX( yaw, pitch, roll );

			Assert.That( q.X, Is.EqualTo( ex ).Within( 0.01f ) );
			Assert.That( q.Y, Is.EqualTo( ey ).Within( 0.01f ) );
			Assert.That( q.Z, Is.EqualTo( ez ).Within( 0.01f ) );
			Assert.That( q.W, Is.EqualTo( ew ).Within( 0.01f ) );
		}

		[Test]
		public void GMath_FromEulerXYZ_AnyFiniteAngle_ProducesUnitQuaternion()
		{
			var q = NMath.FromEulerXYZ( 5f, -6f, 8f );
			float len = MathF.Sqrt( q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W );
			Assert.That( len, Is.EqualTo( 1f ).Within( 0.01f ) );
		}

		[Test]
		public void GMath_FromEulerYXZ_AnyFiniteAngle_ProducesUnitQuaternion()
		{
			var q = NMath.FromEulerYXZ( 5f, -6f, 8f );
			float len = MathF.Sqrt( q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W );
			Assert.That( len, Is.EqualTo( 1f ).Within( 0.01f ) );
		}

		[Test]
		public void GMath_FromEulerZYX_Zero_IsIdentity()
		{
			var q = NMath.FromEulerZYX( 0f, 0f, 0f );
			Assert.That( NMath.IsIdentity( q ), Is.True );
		}
	}
}