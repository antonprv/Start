namespace FastMath_Tests
{
	internal static class SquareRootTestsHelpers
	{
		// GMath.FastInvSqrt's default algorithm is InvSqrtAlgorithm.QuakeBitHack
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
		public const float NORMALIZE_TOLERANCE = 0.003f; // 0.3% - ~1.7x headroom over QuakeBitHack's 0.175%
	}
}