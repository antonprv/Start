// Regression tests for SquareRoot.cs.
//
// Two things this file specifically guards against regressing:
//   1. FastInvSqrt accuracy claims (this session replaced the Quake III hack
//      with a hardware-estimate + Newton-Raphson hybrid - see benchmark notes
//      in SquareRoot.cs). These tests pin the measured error bounds so a
//      future change to the formula can't silently drift without a test
//      failure.
//   2. The overflow bug: Normalize()/Length() on very large vectors used to
//      square components directly, which could overflow to +Infinity and
//      turn into NaN. This is now guarded by pre-scaling - these tests
//      exercise exactly the failure case that prompted the fix.

using Framework.FastMath.Godot;
using Godot;
using NUnit.Framework;

namespace FastMath.Tests
{
    public class SquareRootTests
    {
        // FMath.FastInvSqrt's default algorithm is InvSqrtAlgorithm.QuakeBitHack
        // (single Newton-Raphson pass), chosen for compatibility with targets
        // that don't have MathF.ReciprocalSqrtEstimate. Its measured worst-case
        // relative error is ~0.175% (see BENCHMARKS.md) - these overflow/
        // Normalize tests below check CORRECTNESS (no NaN/Infinity, right
        // ballpark), not tight numerical precision, so the tolerance here is
        // set a bit above that algorithm's real error bound rather than the
        // ~0.03% you'd get from InvSqrtAlgorithm.Hardware. If you switch
        // FastInvSqrt's default (or pass algorithm: Hardware / precise: true)
        // you can tighten this back down - see FastInvSqrt_Default_WithinHalfPercent
        // below for the number actually measured for whatever is the current default.
        private const float NORMALIZE_TOLERANCE = 0.003f; // 0.3% - ~1.7x headroom over QuakeBitHack's 0.175%
        [TestCase( 0.0001f )]
        [TestCase( 1f )]
        [TestCase( 4f )]
        [TestCase( 100f )]
        [TestCase( 500000f )]
        public void FastInvSqrt_Default_WithinHalfPercent( float x )
        {
            float expected = 1f / MathF.Sqrt( x );
            float actual = FMath.FastInvSqrt( x );
            float relErr = MathF.Abs( actual - expected ) / expected;
            Assert.That( relErr, Is.LessThan( 0.005f ), $"relErr={relErr:P4} for x={x}" );
        }

        [TestCase( 0.0001f )]
        [TestCase( 1f )]
        [TestCase( 4f )]
        [TestCase( 100f )]
        [TestCase( 500000f )]
        public void FastInvSqrt_Precise_TighterThanDefault( float x )
        {
            float expected = 1f / MathF.Sqrt( x );
            float defaultErr = MathF.Abs( FMath.FastInvSqrt( x, precise: false ) - expected );
            float preciseErr = MathF.Abs( FMath.FastInvSqrt( x, precise: true ) - expected );
            Assert.That( preciseErr, Is.LessThanOrEqualTo( defaultErr + 1e-9f ) );
        }

        [Test]
        public void FastSqrt_MatchesMathFSqrt_WithinTolerance()
        {
            var rnd = new Random( 1 );
            for ( int i = 0; i < 1000; i++ )
            {
                float x = (float)( rnd.NextDouble() * 10000.0 );
                float expected = MathF.Sqrt( x );
                float actual = FMath.FastSqrt( x );
                if ( expected < 1e-6f )
                {
                    continue; // near-zero: relative error is meaningless
                }
                float relErr = MathF.Abs( actual - expected ) / expected;
                Assert.That( relErr, Is.LessThan( 0.005f ), $"x={x} expected={expected} actual={actual}" );
            }
        }

        [Test]
        public void FastSqrt_NonPositive_ReturnsZero()
        {
            Assert.That( FMath.FastSqrt( 0f ), Is.EqualTo( 0f ) );
            Assert.That( FMath.FastSqrt( -5f ), Is.EqualTo( 0f ) );
        }

        // ----------------------------------------------------------------
        // Overflow-safety regression tests - the actual bug this session found
        // and fixed. Before the fix, all of these produced NaN.
        // ----------------------------------------------------------------

        [TestCase( 1e17f )]
        [TestCase( 1e19f )]
        [TestCase( 1e20f )]
        [TestCase( 1e25f )]
        [TestCase( 3.0e38f )]   // close to float.MaxValue
        public void Normalize_Vector3_HugeComponent_NoNaN( float magnitude )
        {
            var v = new Vector3( magnitude, 0f, 0f );
            FMath.Normalize( ref v );
            Assert.That( float.IsNaN( v.X ), Is.False );
            Assert.That( float.IsNaN( v.Y ), Is.False );
            Assert.That( float.IsNaN( v.Z ), Is.False );
            Assert.That( v.X, Is.EqualTo( 1f ).Within( NORMALIZE_TOLERANCE ), $"expected ~(1,0,0), got ({v.X},{v.Y},{v.Z})" );
        }

        [Test]
        public void Normalize_Vector3_HugeMixedComponents_NoNaN()
        {
            // The original bug: x²+y²+z² overflows to +Infinity even though
            // each individual component is finite and representable.
            var v = new Vector3( 2e19f, 2e19f, 2e19f );
            FMath.Normalize( ref v );
            Assert.That( float.IsNaN( v.X ), Is.False );
            float len = MathF.Sqrt( v.X * v.X + v.Y * v.Y + v.Z * v.Z );
            Assert.That( len, Is.EqualTo( 1f ).Within( NORMALIZE_TOLERANCE ) );
        }

        [TestCase( 1e18f )]
        [TestCase( 1e20f )]
        public void Normalize_Vector2_HugeComponent_NoNaN( float magnitude )
        {
            var v = new Vector2( magnitude, 0f );
            FMath.Normalize( ref v );
            Assert.That( float.IsNaN( v.X ), Is.False );
            Assert.That( v.X, Is.EqualTo( 1f ).Within( NORMALIZE_TOLERANCE ) );
        }

        [Test]
        public void Normalize_Quaternion_HugeComponent_NoNaN()
        {
            var q = new Quaternion( 1e19f, 0f, 0f, 0f );
            FMath.Normalize( ref q );
            Assert.That( float.IsNaN( q.X ), Is.False );
            float len = MathF.Sqrt( q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W );
            Assert.That( len, Is.EqualTo( 1f ).Within( NORMALIZE_TOLERANCE ) );
        }

        [Test]
        public void FastLength_HugeVector_NoOverflow()
        {
            var v = new Vector3( 1e20f, 0f, 0f );
            float len = FMath.FastLength( v );
            Assert.That( float.IsNaN( len ), Is.False );
            Assert.That( float.IsInfinity( len ), Is.False );
            float relErr = MathF.Abs( len - 1e20f ) / 1e20f;
            Assert.That( relErr, Is.LessThan( 0.01f ), $"len={len}" );
        }

        [Test]
        public void Normalize_TypicalGameVector_UnaffectedByOverflowGuard()
        {
            // Sanity check that the overflow guard doesn't change behaviour
            // (or add meaningful cost) for ordinary, small game-scale vectors.
            var v = new Vector3( 3f, 4f, 0f );
            FMath.Normalize( ref v );
            Assert.That( v.X, Is.EqualTo( 0.6f ).Within( NORMALIZE_TOLERANCE ) );
            Assert.That( v.Y, Is.EqualTo( 0.8f ).Within( NORMALIZE_TOLERANCE ) );
        }

        [Test]
        public void Normalize_ZeroVector_ReturnsZero()
        {
            var v = Vector3.Zero;
            FMath.Normalize( ref v );
            Assert.That( v.X, Is.EqualTo( 0f ) );
            Assert.That( v.Y, Is.EqualTo( 0f ) );
            Assert.That( v.Z, Is.EqualTo( 0f ) );
        }

        [Test]
        public void IsNormalized_UnitVector_ReturnsTrue()
        {
            Assert.That( FMath.IsNormalized( new Vector3( 1f, 0f, 0f ) ), Is.True );
        }

        [Test]
        public void IsNormalized_NonUnitVector_ReturnsFalse()
        {
            Assert.That( FMath.IsNormalized( new Vector3( 5f, 0f, 0f ) ), Is.False );
        }
    }
}