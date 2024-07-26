using System.Diagnostics;
using System.Numerics;

namespace BayoMotTool;

public static class MotionUtility
{
    public static Matrix4x4 GetTransform(Motion motion, int boneIndex, float frame, Vector3 boneTranslation)
    {
        float translationX = boneTranslation.X;
        float translationY = boneTranslation.Y;
        float translationZ = boneTranslation.Z;     
        float rotationX = 0.0f;
        float rotationY = 0.0f;
        float rotationZ = 0.0f;
        float scaleX = 1.0f;
        float scaleY = 1.0f;
        float scaleZ = 1.0f;

        foreach (var record in motion.Records)
        {
            if (record.BoneIndex == boneIndex) 
            {
                float value = record.Interpolation.Interpolate(frame);

                switch (record.AnimationTrack)
                {
                    case AnimationTrack.TranslationX:
                        translationX = value;
                        break;
                    case AnimationTrack.TranslationY:
                        translationY = value;
                        break;
                    case AnimationTrack.TranslationZ:
                        translationZ = value;
                        break;
                    case AnimationTrack.RotationX:
                        rotationX = value;
                        break;
                    case AnimationTrack.RotationY:
                        rotationY = value;
                        break;
                    case AnimationTrack.RotationZ:
                        rotationZ = value;
                        break;
                    case AnimationTrack.ScaleX:
                        scaleX = value;
                        break;
                    case AnimationTrack.ScaleY:
                        scaleY = value;
                        break;
                    case AnimationTrack.ScaleZ:
                        scaleZ = value;
                        break;
                }
            }
        }

        // TODO: Apparently the rotation order can change by definitions in the wmb???
        return
            Matrix4x4.CreateScale(scaleX, scaleY, scaleZ) *
            Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, rotationX) *
            Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, rotationY) *
            Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotationZ) *
            Matrix4x4.CreateTranslation(translationX, translationY, translationZ);
    }

    public static void AddOrReplaceRecord(Motion motion, int boneIndex, AnimationTrack animationTrack, int frameCount, IInterpolation interpolation)
    {
        var record = motion.Records.FirstOrDefault(x => 
            x.BoneIndex == boneIndex && x.AnimationTrack == animationTrack);

        if (record == null)
        {
            record = new Record
            {
                BoneIndex = (ushort)boneIndex,
                AnimationTrack = animationTrack,
            };

            motion.Records.Add(record);
        }

        if (interpolation is InterpolationLinear interpolationLinear && 
            interpolationLinear.Values.All(x => MathF.Abs(x - interpolationLinear.Values[0]) < 0.0001f))
        {
            frameCount = 2;
            interpolation = new InterpolationConstant { Value = interpolationLinear.Values[0] };
        }

        record.FrameCount = (ushort)frameCount;
        record.Interpolation = interpolation;
    }

    public static float AngleNormalize(float x)
    {
        float y = (x + MathF.PI) % MathF.Tau;
        return y < 0.0f ? y + MathF.PI : y - MathF.PI;
    }

    public static void UnrollAngles(InterpolationLinear interpolation)
    {
        for (int i = 0; i < interpolation.Values.Length; i++)
            interpolation.Values[i] = AngleNormalize(interpolation.Values[i]);

        for (int i = 1; i < interpolation.Values.Length; i++)
        {
            float rotationDifference = AngleNormalize(interpolation.Values[i] - interpolation.Values[i - 1]);
            interpolation.Values[i] = interpolation.Values[i - 1] + rotationDifference;
        }
    }

    public static void ToEulerAnglesXYZ(in Matrix4x4 matrix, out float x, out float y, out float z)
    {
        y = MathF.Asin(-MathF.Min(1.0f, MathF.Max(-1.0f, matrix.M13)));

        if (MathF.Abs(matrix.M13) < 0.9999999)
        {
            x = MathF.Atan2(matrix.M23, matrix.M33);
            z = MathF.Atan2(matrix.M12, matrix.M11);
        }
        else
        {
            x = MathF.Atan2(-matrix.M32, matrix.M22);
            z = 0;
        }
    }

    public static void ToEulerAnglesYZX(in Matrix4x4 matrix, out float x, out float y, out float z)
    {
        z = MathF.Asin(-MathF.Min(1.0f, MathF.Max(-1.0f, matrix.M21)));

        if (MathF.Abs(matrix.M21) < 0.9999999)
        {
            x = MathF.Atan2(matrix.M23, matrix.M22);
            y = MathF.Atan2(matrix.M31, matrix.M11);
        }
        else
        {
            x = 0;
            y = MathF.Atan2(-matrix.M13, matrix.M33);
        }
    }

    public static void AttachBone(Motion motion, int parentBoneIndex, Vector3 parentBoneTranslation, int boneIndex, Vector3 boneTranslation, bool invertParentTransform)
    {
        InterpolationLinear MakeInterpolationLinear() =>
            new InterpolationLinear { Values = new float[motion.FrameCount] };

        var translationX = MakeInterpolationLinear();
        var translationY = MakeInterpolationLinear();
        var translationZ = MakeInterpolationLinear();
        var rotationX = MakeInterpolationLinear();
        var rotationY = MakeInterpolationLinear();
        var rotationZ = MakeInterpolationLinear();
        var scaleX = MakeInterpolationLinear();
        var scaleY = MakeInterpolationLinear();
        var scaleZ = MakeInterpolationLinear();

        for (int i = 0; i < motion.FrameCount; i++)
        {
            var parentTransform = GetTransform(motion, parentBoneIndex, i, parentBoneTranslation);
            var transform = GetTransform(motion, boneIndex, i, boneTranslation);

            if (invertParentTransform)
                Matrix4x4.Invert(parentTransform, out parentTransform);

            var newTransform = transform * parentTransform;

            Matrix4x4.Decompose(newTransform, out var scale, out _, out var translation);

            translationX.Values[i] = translation.X;
            translationY.Values[i] = translation.Y;
            translationZ.Values[i] = translation.Z;

            ToEulerAnglesXYZ(newTransform,
                out rotationX.Values[i],
                out rotationY.Values[i],
                out rotationZ.Values[i]);

            scaleX.Values[i] = scale.X;
            scaleY.Values[i] = scale.Y;
            scaleZ.Values[i] = scale.Z;
        }

        UnrollAngles(rotationX);
        UnrollAngles(rotationY);
        UnrollAngles(rotationZ);

        void AddOrReplaceRecord(AnimationTrack animationTrack, InterpolationLinear interpolationLinear) => 
            MotionUtility.AddOrReplaceRecord(motion, boneIndex, animationTrack, motion.FrameCount, interpolationLinear);

        AddOrReplaceRecord(AnimationTrack.TranslationX, translationX);
        AddOrReplaceRecord(AnimationTrack.TranslationY, translationY);
        AddOrReplaceRecord(AnimationTrack.TranslationZ, translationZ);     
        AddOrReplaceRecord(AnimationTrack.RotationX, rotationX);
        AddOrReplaceRecord(AnimationTrack.RotationY, rotationY);
        AddOrReplaceRecord(AnimationTrack.RotationZ, rotationZ);        
        AddOrReplaceRecord(AnimationTrack.ScaleX, scaleX);
        AddOrReplaceRecord(AnimationTrack.ScaleY, scaleY);
        AddOrReplaceRecord(AnimationTrack.ScaleZ, scaleZ);
    }

    public static void SortRecords(Motion motion)
    {
        motion.Records = motion.Records.OrderBy(x => x.BoneIndex == 0xFFFF ? 0 : x.BoneIndex + 1).ThenBy(x => x.AnimationTrack).ToList();
        motion.Records.Add(new Record
        {
            BoneIndex = 0x7FFF,
            AnimationTrack = (AnimationTrack)0xFF,
            Interpolation = new InterpolationNone()
        });
    }

    private static readonly AnimationTrack[] _animationTracks =
    {
        AnimationTrack.TranslationX,
        AnimationTrack.TranslationY,
        AnimationTrack.TranslationZ,
        AnimationTrack.RotationX,
        AnimationTrack.RotationY,
        AnimationTrack.RotationZ,
        AnimationTrack.ScaleX,
        AnimationTrack.ScaleY,
        AnimationTrack.ScaleZ,
    };

    public static void AddDefaultRecords(Motion motion, int boneIndex, Vector3 boneTranslation)
    {
        foreach (var animationTrack in _animationTracks)
        {
            if (!motion.Records.Any(x => x.BoneIndex == boneIndex && x.AnimationTrack == animationTrack))
            {
                motion.Records.Add(new Record
                {
                    BoneIndex = (ushort)boneIndex,
                    AnimationTrack = animationTrack,
                    FrameCount = 2,
                    Interpolation = new InterpolationConstant 
                    { 
                        Value = animationTrack <= AnimationTrack.TranslationZ ? boneTranslation[(int)animationTrack] :
                            animationTrack >= AnimationTrack.ScaleX ? 1.0f : 0.0f 
                    }
                });
            }
        }
    }

    public static void ReorientBone(Motion motion, int boneIndex)
    {
        InterpolationLinear MakeInterpolationLinear() =>
            new InterpolationLinear { Values = new float[motion.FrameCount] };

        var rotationX = MakeInterpolationLinear();
        var rotationY = MakeInterpolationLinear();
        var rotationZ = MakeInterpolationLinear();

        for (int i = 0; i < motion.FrameCount; i++)
        {
            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;

            foreach (var record in motion.Records)
            {
                if (record.BoneIndex == boneIndex)
                {
                    float value = record.Interpolation.Interpolate(i);

                    switch (record.AnimationTrack)
                    {
                        case AnimationTrack.RotationX:
                            x = value;
                            break;
                        case AnimationTrack.RotationY:
                            y = value;
                            break;
                        case AnimationTrack.RotationZ:
                            z = value;
                            break;
                    }
                }
            }

            var transform =
                Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, x) *
                Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, y) *
                Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, z) *
                Matrix4x4.Identity;

            ToEulerAnglesYZX(transform,
                out rotationX.Values[i],
                out rotationY.Values[i],
                out rotationZ.Values[i]);
        }

        UnrollAngles(rotationX);
        UnrollAngles(rotationY);
        UnrollAngles(rotationZ);

        void AddOrReplaceRecord(AnimationTrack animationTrack, InterpolationLinear interpolationLinear) =>
            MotionUtility.AddOrReplaceRecord(motion, boneIndex, animationTrack, motion.FrameCount, interpolationLinear);

        AddOrReplaceRecord(AnimationTrack.RotationX, rotationX);
        AddOrReplaceRecord(AnimationTrack.RotationY, rotationY);
        AddOrReplaceRecord(AnimationTrack.RotationZ, rotationZ);
    }
}
