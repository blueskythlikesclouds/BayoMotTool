using System.Numerics;

namespace BayoMotTool;

public static class MotionUtility
{
    public static Matrix4x4 GetTransform(Motion motion, int boneIndex, float frame)
    {
        float translationX = 0.0f;
        float translationY = 0.0f;
        float translationZ = 0.0f;     
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
        return Matrix4x4.CreateScale(scaleX, scaleY, scaleZ) * 
            Matrix4x4.CreateRotationX(rotationX) * 
            Matrix4x4.CreateRotationY(rotationY) *
            Matrix4x4.CreateRotationZ(rotationZ) *
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

        record.FrameCount = (ushort)frameCount;
        record.Interpolation = interpolation;
    }

    public static void AttachBone(Motion motion, int parentBoneIndex, int boneIndex)
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
            var parentTransform = GetTransform(motion, parentBoneIndex, i);
            var transform = GetTransform(motion, boneIndex, i);

            Matrix4x4.Invert(parentTransform, out var inverseParentTransform);

            var newTransform = transform * inverseParentTransform;

            Matrix4x4.Decompose(newTransform, out var scale, out var rotation, out var translation);

            translationX.Values[i] = translation.X;
            translationY.Values[i] = translation.Y;
            translationZ.Values[i] = translation.Z;
            rotationX.Values[i] = rotation.GetPitch();
            rotationY.Values[i] = rotation.GetYaw();
            rotationZ.Values[i] = rotation.GetRoll();
            scaleX.Values[i] = scale.X;
            scaleY.Values[i] = scale.Y;
            scaleZ.Values[i] = scale.Z;
        }

        void AddOrReplaceRecord(AnimationTrack animationTrack, InterpolationLinear interpolationLinear)
        {
            int frameCount = motion.FrameCount;
            IInterpolation interpolation = interpolationLinear;

            if (interpolationLinear.Values.All(x => MathF.Abs(x - interpolationLinear.Values[0]) < 0.0001f))
            {
                frameCount = 1;
                interpolation = new InterpolationConstant { Value = interpolationLinear.Values[0] };
            }

            MotionUtility.AddOrReplaceRecord(motion, boneIndex, animationTrack, frameCount, interpolation);
        }

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
        motion.Records = motion.Records.OrderBy(x => 
        {
            if (x.BoneIndex == 0xFFFF)
                return x.Interpolation is InterpolationNone ? int.MaxValue : 0;

            return x.BoneIndex + 1;
        }).ThenBy(x => x.AnimationTrack).ToList();
    }
}
