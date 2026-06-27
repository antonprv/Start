// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using System.Runtime.CompilerServices;

namespace Framework.FastMath
{
    /// <summary>
    /// Fast square-root, inverse-square-root, length, normalization.
    ///
    /// Hack catalogue:
    ///   FastInvSqrt   - Quake III IEEE-754 bit trick + Newton-Raphson.
    ///   FastSqrt      - x * FastInvSqrt(x), avoids hardware fsqrt.
    ///   FastNormalize - single FastInvSqrt call for all components.
    ///   Normalize(ref Vector3/Vector2) - in-place, zero copies, zero allocs.
    ///   Normalized(in Vector3/Vector2) - returns new value, original untouched.
    ///   IsNormalized  - squared-length vs 1 ± epsilon; avoids sqrt entirely.
    ///   FastLength    - sinSq * invSin trick: sqrt from one inv-sqrt call.
    ///   DistanceSquared - cheapest possible distance check (zero sqrt).
    ///   FastDistance2D  - 2-D fast path (one fewer mul than 3-D).
    ///   FastLengthSq  - computes length² for 2-D and 3-D without any root.
    /// </summary>
    public static partial class FMath
    {
        // ----------------------------------------------------------------
        // FastInvSqrt - Quake III bit-hack  (the heart of the library)
        // ----------------------------------------------------------------

        /// <summary>
        /// Legendary Fast Inverse Square Root from Quake III Arena.
        /// Computes 1/√x faster than hardware √x + division.
        ///
        /// Error after one Newton-Raphson:   ≈ 0.175%
        /// Error after two  Newton-Raphsons: ≈ 0.000015%   (precise mode)
        ///
        /// Original comment: "What the fuck?"
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static unsafe float FastInvSqrt( float x, bool precise = false )
        {
            const int MAGIC = 0x5f3759df;
            float xhalf = 0.5f * x;
            int i = *(int*)&x;
            i = MAGIC - ( i >> 1 );
            x = *(float*)&i;
            x = x * ( 1.5f - xhalf * x * x );           // Newton-Raphson #1
            if ( precise )
                x = x * ( 1.5f - xhalf * x * x );       // Newton-Raphson #2
            return x;
        }

        // ----------------------------------------------------------------
        // FastSqrt
        // ----------------------------------------------------------------

        /// <summary>
        /// Fast square root: x · FastInvSqrt(x).
        /// One Newton-Raphson pass; error ≈ 0.175%.
        /// Returns 0 for x ≤ 0 (guards NaN propagation).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastSqrt( float x )
        {
            if ( x <= 0f ) return 0f;
            return x * FastInvSqrt( x );
        }

        // ----------------------------------------------------------------
        // Normalize (ref scalar components) - lowest-level overload
        // ----------------------------------------------------------------

        /// <summary>
        /// Fast 3-D normalization via a single FastInvSqrt call.
        /// Writes results back through refs - zero copies, zero allocation.
        /// Sets all components to 0 when length² &lt; epsilon.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void FastNormalize( ref float x, ref float y, ref float z,
                                         float epsilon = SMALL_NUMBER )
        {
            float sq = x * x + y * y + z * z;
            if ( sq < epsilon ) { x = y = z = 0f; return; }
            float inv = FastInvSqrt( sq );
            x *= inv; y *= inv; z *= inv;
        }

        // ----------------------------------------------------------------
        // Vector3 - Normalize / Normalized / IsNormalized
        // ----------------------------------------------------------------

        /// <summary>
        /// Normalizes <paramref name="v"/> in-place using FastInvSqrt.
        /// Mutates the original struct through the ref - zero allocation.
        /// Sets v to Vector3.Zero when its length is below epsilon.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Normalize( ref Vector3 v, float epsilon = SMALL_NUMBER )
        {
            float sq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if ( sq < epsilon ) { v = Vector3.Zero; return; }
            float inv = FastInvSqrt( sq );   // ← Quake III bit hack
            v.X *= inv; v.Y *= inv; v.Z *= inv;
        }

        /// <summary>
        /// Returns a new normalized Vector3 using FastInvSqrt.
        /// The original vector is not modified.
        /// Returns Vector3.Zero when length is below epsilon.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 Normalized( in Vector3 v, float epsilon = SMALL_NUMBER )
        {
            float sq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if ( sq < epsilon ) return Vector3.Zero;
            float inv = FastInvSqrt( sq );
            return new Vector3( v.X * inv, v.Y * inv, v.Z * inv );
        }

        /// <summary>
        /// Returns true when |v|² is within epsilon of 1.
        ///
        /// Hack: compares squared length against [1-ε, 1+ε] - no sqrt needed.
        /// Useful to assert invariants on hot paths without paying sqrt cost.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNormalized( in Vector3 v, float epsilon = KINDA_SMALL_NUMBER )
        {
            float sq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            // |sq - 1| < ε  ↔  sq ∈ (1-ε, 1+ε)
            float d = sq - 1f;
            return d > -epsilon & d < epsilon;   // bitwise & avoids branch
        }

        // ----------------------------------------------------------------
        // Vector2 - Normalize / Normalized / IsNormalized
        // ----------------------------------------------------------------

        /// <summary>
        /// Normalizes a Vector2 in-place using FastInvSqrt.
        /// Writes back through ref - zero allocation.
        /// Sets v to Vector2.Zero when length is below epsilon.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Normalize( ref Vector2 v, float epsilon = SMALL_NUMBER )
        {
            float sq = v.X * v.X + v.Y * v.Y;
            if ( sq < epsilon ) { v = Vector2.Zero; return; }
            float inv = FastInvSqrt( sq );
            v.X *= inv; v.Y *= inv;
        }

        /// <summary>
        /// Returns a new normalized Vector2 using FastInvSqrt.
        /// Returns Vector2.Zero when length is below epsilon.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2 Normalized( in Vector2 v, float epsilon = SMALL_NUMBER )
        {
            float sq = v.X * v.X + v.Y * v.Y;
            if ( sq < epsilon ) return Vector2.Zero;
            float inv = FastInvSqrt( sq );
            return new Vector2( v.X * inv, v.Y * inv );
        }

        /// <summary>
        /// Returns true when |v|² is within epsilon of 1 (no sqrt).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNormalized( in Vector2 v, float epsilon = KINDA_SMALL_NUMBER )
        {
            float sq = v.X * v.X + v.Y * v.Y;
            float d = sq - 1f;
            return d > -epsilon & d < epsilon;
        }

        // ----------------------------------------------------------------
        // Quaternion Normalize (in FastQuaternion.cs) - kept there for cohesion,
        // but Quaternion.IsNormalized goes here with the same pattern.
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns true when |q|² is within epsilon of 1 (no sqrt).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsNormalized( in Quaternion q, float epsilon = KINDA_SMALL_NUMBER )
        {
            float sq = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
            float d = sq - 1f;
            return d > -epsilon & d < epsilon;
        }

        // ----------------------------------------------------------------
        // Length helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Fast 3-D vector length.
        ///
        /// Hack: length = sq · FastInvSqrt(sq) - gets sqrt from the same
        /// Newton-Raphson pass we'd need for normalize anyway.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastLength( float x, float y, float z,
                                       float epsilon = SMALL_NUMBER )
        {
            float sq = x * x + y * y + z * z;
            if ( sq < epsilon ) return 0f;
            return sq * FastInvSqrt( sq );           // sqrt(sq) via single inv-sqrt
        }

        /// <summary>Fast 3-D length of a Godot Vector3.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastLength( in Vector3 v )
        {
            float sq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            if ( sq < SMALL_NUMBER ) return 0f;
            return sq * FastInvSqrt( sq );
        }

        /// <summary>
        /// Fast 2-D length.
        /// One fewer multiply than the 3-D path - worth a separate overload
        /// when processing thousands of 2-D vectors per frame.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastLength( in Vector2 v )
        {
            float sq = v.X * v.X + v.Y * v.Y;
            if ( sq < SMALL_NUMBER ) return 0f;
            return sq * FastInvSqrt( sq );
        }

        // ----------------------------------------------------------------
        // LengthSq helpers - cheapest possible magnitude check
        // ----------------------------------------------------------------

        /// <summary>Squared length of a Vector3 - no sqrt, use for comparisons.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float LengthSq( in Vector3 v )
            => v.X * v.X + v.Y * v.Y + v.Z * v.Z;

        /// <summary>Squared length of a Vector2.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float LengthSq( in Vector2 v )
            => v.X * v.X + v.Y * v.Y;

        // ----------------------------------------------------------------
        // Distance helpers
        // ----------------------------------------------------------------

        /// <summary>Fast 3-D distance via FastInvSqrt.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastDistance( float x1, float y1, float z1,
                                         float x2, float y2, float z2 )
        {
            float dx = x2 - x1, dy = y2 - y1, dz = z2 - z1;
            return FastLength( dx, dy, dz );
        }

        /// <summary>Fast 3-D distance between two Vector3 points.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastDistance( in Vector3 a, in Vector3 b )
        {
            float dx = b.X - a.X, dy = b.Y - a.Y, dz = b.Z - a.Z;
            float sq = dx * dx + dy * dy + dz * dz;
            if ( sq < SMALL_NUMBER ) return 0f;
            return sq * FastInvSqrt( sq );
        }

        /// <summary>Fast 2-D distance between two Vector2 points.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float FastDistance( in Vector2 a, in Vector2 b )
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float sq = dx * dx + dy * dy;
            if ( sq < SMALL_NUMBER ) return 0f;
            return sq * FastInvSqrt( sq );
        }

        /// <summary>
        /// Squared 3-D distance - zero sqrt, ideal for "closer than X" checks.
        /// Compare result against X*X instead of X.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float DistanceSquared( float x1, float y1, float z1,
                                            float x2, float y2, float z2 )
        {
            float dx = x2 - x1, dy = y2 - y1, dz = z2 - z1;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>Squared 3-D distance between two Vector3 points.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float DistanceSquared( in Vector3 a, in Vector3 b )
        {
            float dx = b.X - a.X, dy = b.Y - a.Y, dz = b.Z - a.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>Squared 2-D distance between two Vector2 points.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float DistanceSquared( in Vector2 a, in Vector2 b )
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            return dx * dx + dy * dy;
        }

        // ----------------------------------------------------------------
        // Dot products (Vector2 / Vector3)
        // ----------------------------------------------------------------

        /// <summary>3-D dot product - no division, no sqrt, purely additive.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float Dot( in Vector3 a, in Vector3 b )
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        /// <summary>2-D dot product.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float Dot( in Vector2 a, in Vector2 b )
            => a.X * b.X + a.Y * b.Y;

        /// <summary>
        /// 2-D perpendicular (perp) dot product - equivalent to the Z component
        /// of the 3-D cross product: a.X·b.Y − a.Y·b.X.
        /// Sign tells you left/right of the vector; magnitude = |a||b|sin θ.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static float PerpDot( in Vector2 a, in Vector2 b )
            => a.X * b.Y - a.Y * b.X;

        /// <summary>3-D cross product. Result is perpendicular to both a and b.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 Cross( in Vector3 a, in Vector3 b )
            => new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X );

        // ----------------------------------------------------------------
        // Reflect / Project - built on Dot, no stdlib calls
        // ----------------------------------------------------------------

        /// <summary>
        /// Reflects vector <paramref name="v"/> about normal <paramref name="n"/>
        /// (n must be unit length).  v' = v − 2·(v·n)·n.
        /// 6 muls + 3 adds - cheaper than any library call.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 Reflect( in Vector3 v, in Vector3 n )
        {
            float d2 = 2f * Dot( v, n );
            return new Vector3( v.X - d2 * n.X, v.Y - d2 * n.Y, v.Z - d2 * n.Z );
        }

        /// <summary>2-D reflection about unit normal n.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2 Reflect( in Vector2 v, in Vector2 n )
        {
            float d2 = 2f * Dot( v, n );
            return new Vector2( v.X - d2 * n.X, v.Y - d2 * n.Y );
        }

        /// <summary>
        /// Projects <paramref name="v"/> onto unit vector <paramref name="onto"/>.
        /// v_proj = (v·onto)·onto.  Result is parallel to onto.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 Project( in Vector3 v, in Vector3 onto )
        {
            float d = Dot( v, onto );
            return new Vector3( d * onto.X, d * onto.Y, d * onto.Z );
        }

        /// <summary>
        /// Projects <paramref name="v"/> onto non-unit vector <paramref name="onto"/>.
        /// Cheaper than normalizing onto first when both vectors are hot.
        ///
        /// Hack: uses FastInvSqrt to compute 1/|onto|² and avoid division.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 ProjectOnto( in Vector3 v, in Vector3 onto )
        {
            float sq = onto.X * onto.X + onto.Y * onto.Y + onto.Z * onto.Z;
            if ( sq < SMALL_NUMBER ) return Vector3.Zero;
            float invSq = FastInvSqrt( sq ) * FastInvSqrt( sq ); // 1/sq ≈ invSqrt²
            float scale = Dot( v, onto ) * invSq;
            return new Vector3( scale * onto.X, scale * onto.Y, scale * onto.Z );
        }

        /// <summary>
        /// Returns the component of <paramref name="v"/> perpendicular to
        /// unit vector <paramref name="onto"/> (v − Project(v, onto)).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 ProjectPerp( in Vector3 v, in Vector3 onto )
        {
            float d = Dot( v, onto );
            return new Vector3( v.X - d * onto.X, v.Y - d * onto.Y, v.Z - d * onto.Z );
        }

        // ----------------------------------------------------------------
        // Clamp vector lengths
        // ----------------------------------------------------------------

        /// <summary>
        /// Clamps <paramref name="v"/> so its length does not exceed
        /// <paramref name="maxLength"/>.  Returns v unchanged if already shorter.
        ///
        /// Hack: squared comparison avoids sqrt for the common fast path.
        /// FastInvSqrt is called only when the clamp is actually needed.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 ClampLength( in Vector3 v, float maxLength )
        {
            float sq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            float max2 = maxLength * maxLength;
            if ( sq <= max2 ) return v;                      // fast path - no sqrt
            float scale = maxLength * FastInvSqrt( sq );     // scale = maxLen / |v|
            return new Vector3( v.X * scale, v.Y * scale, v.Z * scale );
        }

        /// <summary>2-D version of ClampLength.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2 ClampLength( in Vector2 v, float maxLength )
        {
            float sq = v.X * v.X + v.Y * v.Y;
            float max2 = maxLength * maxLength;
            if ( sq <= max2 ) return v;
            float scale = maxLength * FastInvSqrt( sq );
            return new Vector2( v.X * scale, v.Y * scale );
        }

        // ----------------------------------------------------------------
        // MoveTowards - useful for AI steering, interpolated movement
        // ----------------------------------------------------------------

        /// <summary>
        /// Moves a Vector3 position toward <paramref name="target"/> by at most
        /// <paramref name="maxDelta"/> per call.  Does NOT overshoot.
        ///
        /// Hack: squared distance comparison before FastSqrt so the very common
        /// "already at target" case costs zero sqrt.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 MoveTowards( in Vector3 current, in Vector3 target,
                                          float maxDelta )
        {
            float dx = target.X - current.X;
            float dy = target.Y - current.Y;
            float dz = target.Z - current.Z;
            float sq = dx * dx + dy * dy + dz * dz;
            float max2 = maxDelta * maxDelta;
            if ( sq <= max2 || sq < SMALL_NUMBER ) return target;   // already close
            float scale = maxDelta * FastInvSqrt( sq );             // maxDelta / dist
            return new Vector3( current.X + dx * scale,
                               current.Y + dy * scale,
                               current.Z + dz * scale );
        }

        /// <summary>2-D MoveTowards.</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2 MoveTowards( in Vector2 current, in Vector2 target,
                                          float maxDelta )
        {
            float dx = target.X - current.X;
            float dy = target.Y - current.Y;
            float sq = dx * dx + dy * dy;
            float max2 = maxDelta * maxDelta;
            if ( sq <= max2 || sq < SMALL_NUMBER ) return target;
            float scale = maxDelta * FastInvSqrt( sq );
            return new Vector2( current.X + dx * scale,
                               current.Y + dy * scale );
        }

        // ----------------------------------------------------------------
        // Lerp Vector2 / Vector3
        // ----------------------------------------------------------------

        /// <summary>Component-wise linear interpolation for Vector3. t clamped to [0,1].</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 Lerp( in Vector3 a, in Vector3 b, float t )
        {
            t = Clamp01( t );
            return new Vector3( a.X + ( b.X - a.X ) * t,
                               a.Y + ( b.Y - a.Y ) * t,
                               a.Z + ( b.Z - a.Z ) * t );
        }

        /// <summary>Component-wise linear interpolation for Vector2. t clamped to [0,1].</summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector2 Lerp( in Vector2 a, in Vector2 b, float t )
        {
            t = Clamp01( t );
            return new Vector2( a.X + ( b.X - a.X ) * t,
                               a.Y + ( b.Y - a.Y ) * t );
        }

        // ----------------------------------------------------------------
        // Bit-manipulation helpers (private - shared across the library)
        // ----------------------------------------------------------------

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static unsafe int FloatToInt32Bits( float value ) => *(int*)&value;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static unsafe float Int32BitsToFloat( int value ) => *(float*)&value;
    }
}
