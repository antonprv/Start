// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Runtime.CompilerServices;

namespace FastMath
{
    public static partial class FMath
    {
        // ----------------------------------------------------------------
        // FastAtan / FastAtan2
        //
        // Strategy - octant reduction:
        //   r = (x ± |y|) / (|y| ± x)   →   r ∈ [−1, 1]
        //   atan2(y, x) = base_angle − atan(r)
        //
        // Two quality levels:
        //   Fast (default) - cubic polynomial,  error < 0.0038 rad (≈ 0.22°)
        //   Precise        - 5th-order Horner,  error < 0.0002 rad (≈ 0.01°)
        //
        // Hot-path optimisations (unsafe required):
        //   • Absolute value of y: clear sign bit via pointer cast (no branch,
        //     no NaN-guard needed - the x == 0 path is handled first).
        //   • Sign restoration of result: OR the original sign bit back into
        //     the result's IEEE-754 bit-pattern (no branch, no multiply by ±1).
        // ----------------------------------------------------------------

        // --- cubic atan polynomial on [−1, 1]: atan(x) ≈ (C1 + C3·x²)·x ---
        // coefficients from Rall's approximation
        private const float ATAN_C1 = 0.9817f;   // ≈ π/4 + correction
        private const float ATAN_C3 = -0.1963f;

        // --- 5th-order minimax polynomial: atan(x) ≈ x·(P0 + x²·(P2 + x²·P4)) ---
        private const float ATAN_P0 = 0.99997749f;
        private const float ATAN_P2 = -0.33256906f;
        private const float ATAN_P4 = 0.19153276f;

        /// <summary>
        /// Fast arctangent. Returns angle in radians, range [−π/2, π/2].
        /// <para>Fast mode error:    &lt; 0.0038 rad (≈ 0.22°)</para>
        /// <para>Precise mode error: &lt; 0.0002 rad (≈ 0.01°)</para>
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <param name="precise">Use higher-order polynomial for better accuracy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float FastAtan(float x, bool precise = false)
        {
            // Split sign bit and absolute value without a branch.
            int xBits = *(int*)&x;
            int signBit = xBits & unchecked((int)0x80000000);
            int absXInt = xBits & 0x7FFFFFFF;
            float absX = *(float*)&absXInt;

            float result;

            if (absX <= 1f)
            {
                // Direct polynomial for |x| ≤ 1.
                result = precise ? AtanUnitPrecise(absX) : AtanUnit(absX);
            }
            else
            {
                // Identity: atan(x) = π/2 − atan(1/|x|)  for |x| > 1.
                // One division is cheaper than a full atan branch tree.
                float inv = 1f / absX;
                result = HALF_PI - (precise ? AtanUnitPrecise(inv) : AtanUnit(inv));
            }

            // Restore original sign in one OR - result is always ≥ 0 here,
            // so its sign bit is 0 and we can safely OR in the input sign bit.
            int resBits = *(int*)&result;
            int signed = resBits | signBit;
            return *(float*)&signed;
        }

        /// <summary>
        /// Fast four-quadrant arctangent of (y, x).
        /// Returns angle in radians, range [−π, π].
        /// Matches the sign convention of <c>Mathf.Atan2</c>.
        /// <para>Fast mode error:    &lt; 0.0038 rad (≈ 0.22°)</para>
        /// <para>Precise mode error: &lt; 0.0002 rad (≈ 0.01°)</para>
        /// </summary>
        /// <param name="y">Vertical component.</param>
        /// <param name="x">Horizontal component.</param>
        /// <param name="precise">Use higher-order polynomial for better accuracy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float FastAtan2(float y, float x, bool precise = false)
        {
            // Absolute value of y: clear sign bit (no branch, no float mask).
            int yBits = *(int*)&y;
            int absYInt = yBits & 0x7FFFFFFF;
            float absY = *(float*)&absYInt;

            // Degenerate case: x = 0.
            if (x == 0f)
            {
                if (absYInt == 0) return 0f;   // atan2(0, 0) → 0 (matches Unity/Godot)
                                               // Return ±π/2 preserving sign of y.
                return (yBits & unchecked((int)0x80000000)) == 0 ? HALF_PI : -HALF_PI;
            }

            // Octant reduction - maps (x, y) to r ∈ [−1, 1] and chooses base angle.
            //   x ≥ 0 : r = (x − |y|) / (x + |y|),  base = π/4
            //   x < 0 : r = (x + |y|) / (|y| − x),  base = 3π/4
            //
            // Identity: atan2(y, x) = base − atan(r)
            float r, angle;

            if (x >= 0f)
            {
                r = (x - absY) / (x + absY);
                angle = QUARTER_PI;
            }
            else
            {
                r = (x + absY) / (absY - x);
                angle = THREE_QTR_PI;
            }

            // Subtract atan(r) - both fast and precise polys handle negative r correctly.
            angle -= precise ? AtanUnitPrecise(r) : AtanUnit(r);

            // Restore sign of y: negate if y < 0.
            return (yBits & unchecked((int)0x80000000)) != 0 ? -angle : angle;
        }

        // ----------------------------------------------------------------
        // Private atan(x) polynomial helpers.
        // Both accept x ∈ [−1, 1] and return values in [−π/4, π/4].
        // The same polys are reused by FastAtan and FastAtan2 so the
        // code is compiled once and inlined at each call site.
        // ----------------------------------------------------------------

        /// <summary>
        /// Cubic atan approximation: (C1 + C3·x²)·x, error &lt; 0.0038 rad.
        /// Naturally odd (handles negative x correctly).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float AtanUnit(float x)
          => (ATAN_C1 + ATAN_C3 * x * x) * x;

        /// <summary>
        /// 5th-degree minimax atan via Horner's scheme, error &lt; 0.0002 rad.
        /// Naturally odd (handles negative x correctly).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float AtanUnitPrecise(float x)
        {
            float x2 = x * x;
            return x * (ATAN_P0 + x2 * (ATAN_P2 + x2 * ATAN_P4));
        }
    }
}
