using System.Numerics;

namespace BayoMotTool;

public static class QuaternionEx
{
    public static float GetPitch(this Quaternion q)
    {
        float y = 2 * (q.Y * q.Z + q.W * q.X);
        float x = q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z;

        if (MathF.Abs(x) < 0.0001f && MathF.Abs(y) < 0.0001f)
            return 2.0f * MathF.Atan2(q.X, q.W);

        return MathF.Atan2(y, x);
    }

    public static float GetRoll(this Quaternion q) =>
        MathF.Atan2(2 * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);

    public static float GetYaw(this Quaternion q) =>
        MathF.Asin(MathF.Max(-1.0f, Math.Min(1.0f, -2 * (q.X * q.Z - q.W * q.Y))));
}