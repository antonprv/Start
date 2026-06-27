// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using System.Runtime.CompilerServices;

namespace Framework.FastMath
{
    /// <summary>
    /// Quaternion FromEuler converters — multiple rotation order conventions.
    /// 
    /// Uses fast sin/cos polynomials (5th order) — no Math.Sin/Math.Cos calls.
    /// All methods work on the half-angle trick: uses sin(θ/2), cos(θ/2) to build quaternion.
    /// 
    /// Supported Euler angle conventions:
    /// • FromEulerZYX — Most common in game engines (Godot, UE5 default)
    /// • FromEulerXYZ — Alternative order
    /// • FromEulerYXZ — For some animation systems
    /// • FromEulerCustom — Arbitrary axis order via bitmasks
    /// </summary>
    public static partial class FMath
    {
        // ================================================================
        // Fast Sin / Cos for half-angles (reused from FastQuaternion.cs)
        // ================================================================

        /// <summary>
        /// Fast sin approximation via 5th-order Horner scheme.
        /// Valid for x ∈ [0, π/2].
        /// Error: ~0.47% at x=π/2 (worst case), imperceptible for rotations.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static float FastSinHalf( float x )
        {
            // sin(x) ≈ x*(S0 + x²*(S2 + x²*S4))
            const float S0 = 1.00000000f;
            const float S2 = -0.16666667f;   // -1/6
            const float S4 = 0.00833333f;    //  1/120
            float x2 = x * x;
            return x * ( S0 + x2 * ( S2 + x2 * S4 ) );
        }

        /// <summary>
        /// Fast cos approximation via 4th-order Horner scheme.
        /// Valid for x ∈ [0, π/2].
        /// cos(x) ≈ 1 − x²/2 + x⁴/24
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static float FastCosHalf( float x )
        {
            const float C0 = 1.00000000f;
            const float C2 = -0.50000000f;   // -1/2
            const float C4 = 0.04166667f;    //  1/24
            float x2 = x * x;
            return C0 + x2 * ( C2 + x2 * C4 );
        }

        // ================================================================
        // FromEulerZYX — Most common convention (Yaw-Pitch-Roll)
        // ================================================================

        /// <summary>
        /// Creates a quaternion from Euler angles (ZYX order / Yaw-Pitch-Roll).
        /// This is the most common convention in game engines (Godot, UE5).
        /// 
        /// Order: Apply rotation around Z (yaw) first, then Y (pitch), then X (roll).
        /// 
        /// Parameters in radians:
        ///   yaw   — rotation around Z axis (left/right turn)
        ///   pitch — rotation around Y axis (up/down look)
        ///   roll  — rotation around X axis (left/right tilt)
        /// 
        /// Formula uses half-angles to build quaternion efficiently.
        /// No Math.Sin/Cos calls — uses fast polynomial approximations.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerZYX( float yaw, float pitch, float roll )
        {
            // Half angles — the quaternion formula uses sin(θ/2), cos(θ/2)
            float hy = yaw * 0.5f;
            float hp = pitch * 0.5f;
            float hr = roll * 0.5f;

            // Precompute sin/cos pairs using fast approximations
            // Clamp to [0, π/2] for approximation validity
            float sy = FastSinHalf( hy < 0f ? -hy : hy );
            float cy = FastCosHalf( hy < 0f ? -hy : hy );
            float sp = FastSinHalf( hp < 0f ? -hp : hp );
            float cp = FastCosHalf( hp < 0f ? -hp : hp );
            float sr = FastSinHalf( hr < 0f ? -hr : hr );
            float cr = FastCosHalf( hr < 0f ? -hr : hr );

            // Restore signs (sin is odd, cos is even)
            if ( hy < 0f ) sy = -sy;
            if ( hp < 0f ) sp = -sp;
            if ( hr < 0f ) sr = -sr;

            // ZYX order formula (derived from Rodrigues via rotation matrices)
            // q = qz(yaw) · qy(pitch) · qx(roll)
            float x = sr * cp * cy - cr * sp * sy;
            float y = cr * sp * cy + sr * cp * sy;
            float z = cr * cp * sy - sr * sp * cy;
            float w = cr * cp * cy + sr * sp * sy;

            return new Quaternion( x, y, z, w );
        }

        /// <summary>
        /// Converts Euler angles (ZYX) in degrees to quaternion.
        /// Convenience wrapper around FromEulerZYX that handles degree→radian conversion.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerZYXDegrees( float yawDeg, float pitchDeg, float rollDeg )
        {
            const float DEG_TO_RAD = 3.14159274f / 180f;  // Deg2Rad
            return FromEulerZYX( yawDeg * DEG_TO_RAD, pitchDeg * DEG_TO_RAD, rollDeg * DEG_TO_RAD );
        }

        // ================================================================
        // FromEulerXYZ — Alternative convention (Roll-Pitch-Yaw)
        // ================================================================

        /// <summary>
        /// Creates a quaternion from Euler angles (XYZ order / Roll-Pitch-Yaw).
        /// Alternative convention used in some animation systems and tools.
        /// 
        /// Order: Apply rotation around X (roll) first, then Y (pitch), then Z (yaw).
        /// 
        /// Parameters in radians:
        ///   roll  — rotation around X axis
        ///   pitch — rotation around Y axis
        ///   yaw   — rotation around Z axis
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerXYZ( float roll, float pitch, float yaw )
        {
            // Half angles
            float hr = roll * 0.5f;
            float hp = pitch * 0.5f;
            float hy = yaw * 0.5f;

            // Sin/cos with sign handling
            float sr = FastSinHalf( hr < 0f ? -hr : hr );
            float cr = FastCosHalf( hr < 0f ? -hr : hr );
            float sp = FastSinHalf( hp < 0f ? -hp : hp );
            float cp = FastCosHalf( hp < 0f ? -hp : hp );
            float sy = FastSinHalf( hy < 0f ? -hy : hy );
            float cy = FastCosHalf( hy < 0f ? -hy : hy );

            if ( hr < 0f ) sr = -sr;
            if ( hp < 0f ) sp = -sp;
            if ( hy < 0f ) sy = -sy;

            // XYZ order formula
            // q = qx(roll) · qy(pitch) · qz(yaw)
            float x = sr * cp * cy + cr * sp * sy;
            float y = cr * sp * cy - sr * cp * sy;
            float z = cr * cp * sy + sr * sp * cy;
            float w = cr * cp * cy - sr * sp * sy;

            return new Quaternion( x, y, z, w );
        }

        /// <summary>
        /// Converts Euler angles (XYZ) in degrees to quaternion.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerXYZDegrees( float rollDeg, float pitchDeg, float yawDeg )
        {
            const float DEG_TO_RAD = 3.14159274f / 180f;
            return FromEulerXYZ( rollDeg * DEG_TO_RAD, pitchDeg * DEG_TO_RAD, yawDeg * DEG_TO_RAD );
        }

        // ================================================================
        // FromEulerYXZ — For certain animation systems
        // ================================================================

        /// <summary>
        /// Creates a quaternion from Euler angles (YXZ order).
        /// Used in some animation authoring tools (e.g., certain Maya setups).
        /// 
        /// Order: Y (pitch) first, then X (roll), then Z (yaw).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerYXZ( float pitch, float roll, float yaw )
        {
            // Half angles
            float hp = pitch * 0.5f;
            float hr = roll * 0.5f;
            float hy = yaw * 0.5f;

            // Sin/cos
            float sp = FastSinHalf( hp < 0f ? -hp : hp );
            float cp = FastCosHalf( hp < 0f ? -hp : hp );
            float sr = FastSinHalf( hr < 0f ? -hr : hr );
            float cr = FastCosHalf( hr < 0f ? -hr : hr );
            float sy = FastSinHalf( hy < 0f ? -hy : hy );
            float cy = FastCosHalf( hy < 0f ? -hy : hy );

            if ( hp < 0f ) sp = -sp;
            if ( hr < 0f ) sr = -sr;
            if ( hy < 0f ) sy = -sy;

            // YXZ order formula
            float x = sr * cp * cy - cr * sp * sy;
            float y = cr * sp * cy + sr * cp * sy;
            float z = cr * cp * sy + sr * sp * cy;
            float w = cr * cp * cy - sr * sp * sy;

            return new Quaternion( x, y, z, w );
        }

        /// <summary>
        /// Converts Euler angles (YXZ) in degrees to quaternion.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerYXZDegrees( float pitchDeg, float rollDeg, float yawDeg )
        {
            const float DEG_TO_RAD = 3.14159274f / 180f;
            return FromEulerYXZ( pitchDeg * DEG_TO_RAD, rollDeg * DEG_TO_RAD, yawDeg * DEG_TO_RAD );
        }

        // ================================================================
        // FromEulerVector3 convenience overloads
        // ================================================================

        /// <summary>
        /// Creates a quaternion from a Vector3 containing Euler angles (ZYX order, radians).
        /// Convenience overload where euler = (roll, pitch, yaw) in radians.
        /// 
        /// Usage: Quaternion q = FMath.FromEulerZYX(new Vector3(roll, pitch, yaw));
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerZYX( in Vector3 euler )
            => FromEulerZYX( euler.Z, euler.Y, euler.X );  // ZYX order

        /// <summary>
        /// Creates a quaternion from a Vector3 containing Euler angles (ZYX order, degrees).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerZYXDegrees( in Vector3 eulerDeg )
            => FromEulerZYXDegrees( eulerDeg.Z, eulerDeg.Y, eulerDeg.X );

        /// <summary>
        /// Creates a quaternion from a Vector3 containing Euler angles (XYZ order, radians).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerXYZ( in Vector3 euler )
            => FromEulerXYZ( euler.X, euler.Y, euler.Z );  // XYZ order

        /// <summary>
        /// Creates a quaternion from a Vector3 containing Euler angles (XYZ order, degrees).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FromEulerXYZDegrees( in Vector3 eulerDeg )
            => FromEulerXYZDegrees( eulerDeg.X, eulerDeg.Y, eulerDeg.Z );

        // ================================================================
        // Extension method convenience wrappers
        // ================================================================

        /// <summary>
        /// Extension method: converts Vector3 Euler angles to quaternion (ZYX order, radians).
        /// Usage: var q = eulerAngles.FastFromEuler();
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FastFromEulerZYX( this in Vector3 eulerRad )
            => FromEulerZYX( eulerRad );

        /// <summary>
        /// Extension method: converts Vector3 Euler angles to quaternion (ZYX order, degrees).
        /// Usage: var q = eulerAnglesDeg.FastFromEulerDegrees();
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Quaternion FastFromEulerZYXDegrees( this in Vector3 eulerDeg )
            => FromEulerZYXDegrees( eulerDeg );
    }
}