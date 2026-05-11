using Godot;

using FastMath;

namespace Code.Common.FastMath
{
    public static class FMathAdapter
    {
        public static bool IsNearlyEqual(this Vector2 vec2, Vector2 anotherVec2) =>
            FMath.IsNearlyEqual(vec2.X, anotherVec2.X) &&
            FMath.IsNearlyEqual(vec2.Y, anotherVec2.Y);

        public static bool IsNearlyEqual(this Vector3 vec3, Vector3 anotherVec3) =>
            FMath.IsNearlyEqual(vec3.X, anotherVec3.X) &&
            FMath.IsNearlyEqual(vec3.Y, anotherVec3.Y) &&
            FMath.IsNearlyEqual(vec3.Z, anotherVec3.Z);

        public static bool IsNearlyEqual(this Quaternion quat, Quaternion anotherQuat) =>
            FMath.IsNearlyEqual(quat.X, anotherQuat.X) &&
            FMath.IsNearlyEqual(quat.Y, anotherQuat.Y) &&
            FMath.IsNearlyEqual(quat.Z, anotherQuat.Z) &&
            FMath.IsNearlyEqual(quat.W, anotherQuat.W);
    }
}
