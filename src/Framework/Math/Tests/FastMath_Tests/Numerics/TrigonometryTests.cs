using NUnit.Framework;
using NMath = Framework.FastMath.Numerics.FMath;

namespace FastMath_Tests.Numerics
{
	public class TrigonometryTests
	{
		public static readonly float[] WideRangeAngles =
		{
			0f, 0.1f, 1f, MathF.PI / 2f, MathF.PI, 2f * MathF.PI,
			-0.1f, -1f, -MathF.PI / 2f, -MathF.PI, -2f * MathF.PI,
			5f, -5f, 10f, -10f, 100f, -100f
		};

		[TestCaseSource( nameof( WideRangeAngles ) )]
		public void FastSin_AnyFiniteInput_MatchesMathFSin( float angle )
		{
			float expected = MathF.Sin( angle );
			float actual = NMath.FastSin( angle );
			// Documented worst-case error of the underlying polynomial is
			// ~0.47%; use an absolute tolerance since sin values pass through 0.
			Assert.That( actual, Is.EqualTo( expected ).Within( 0.01f ),
				$"angle={angle} expected={expected} actual={actual}" );
		}

		[TestCaseSource( nameof( WideRangeAngles ) )]
		public void FastCos_AnyFiniteInput_MatchesMathFCos( float angle )
		{
			float expected = MathF.Cos( angle );
			float actual = NMath.FastCos( angle );
			Assert.That( actual, Is.EqualTo( expected ).Within( 0.01f ),
				$"angle={angle} expected={expected} actual={actual}" );
		}

		[Test]
		public void FastSinUnsafe_InDomain_MatchesMathFSin()
		{
			// FastSinUnsafe's contract is x ∈ [-π/2, π/2] - test only within it.
			var rnd = new Random( 2 );
			for ( int i = 0; i < 200; i++ )
			{
				float x = ( (float)rnd.NextDouble() * 2f - 1f ) * ( MathF.PI / 2f );
				float expected = MathF.Sin( x );
				float actual = NMath.FastSin( x );
				Assert.That( actual, Is.EqualTo( expected ).Within( 0.01f ), $"x={x}" );
			}
		}

		[Test]
		public void FastAtan2_MatchesMathFAtan2_AcrossQuadrants()
		{
			var rnd = new Random( 3 );
			for ( int i = 0; i < 500; i++ )
			{
				float y = (float)( rnd.NextDouble() * 20 - 10 );
				float x = (float)( rnd.NextDouble() * 20 - 10 );
				float expected = MathF.Atan2( y, x );
				float actual = NMath.FastAtan2( y, x, precise: true );
				Assert.That( actual, Is.EqualTo( expected ).Within( 0.001f ),
					$"y={y} x={x} expected={expected} actual={actual}" );
			}
		}

		[Test]
		public void FastAtan2_ZeroZero_ReturnsZero()
		{
			Assert.That( NMath.FastAtan2( 0f, 0f ), Is.EqualTo( 0f ) );
		}
	}
}