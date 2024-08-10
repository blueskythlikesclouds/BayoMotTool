namespace BayoMotTool;

public class InterpolationHermite : IInterpolation
{
    public struct KeyFrame
    {
        public ushort Frame;
        public float Value;
        public float In;
        public float Out;

        public override string ToString() => $"{Frame}";
    }

    public KeyFrame[] KeyFrames { get; set; }

    public void ReadBayo1(EndianBinaryReader reader, int count)
    {
        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Frame = reader.ReadUInt16();
            _ = reader.ReadUInt16();
            keyFrame.Value = reader.ReadSingle();
            keyFrame.In = reader.ReadSingle();
            keyFrame.Out = reader.ReadSingle();
        }
    }

    public void ReadBayo2(EndianBinaryReader reader, int count)
    {
        ReadBayo1(reader, count);
    }

    public void WriteBayo1(EndianBinaryWriter writer)
    {
        foreach (var keyFrame in KeyFrames)
        {
            writer.Write(keyFrame.Frame);
            writer.Write((ushort)0);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }

    public void WriteBayo2(EndianBinaryWriter writer)
    {
        WriteBayo1(writer);
    }

    public float Interpolate(float frame)
    {
        ref var firstKeyFrame = ref KeyFrames[0];

        if (firstKeyFrame.Frame <= frame)
            return firstKeyFrame.Value;

        for (int i = 0; i < KeyFrames.Length - 1; i++)
        {
            ref var keyFrame = ref KeyFrames[i];
            ref var nextKeyFrame = ref KeyFrames[i + 1];

            if (keyFrame.Frame <= frame && frame <= nextKeyFrame.Frame)
            {
                float t = (frame - keyFrame.Frame) / (nextKeyFrame.Frame - keyFrame.Frame);

                return 
                    (2 * t * t * t - 3 * t * t + 1) * keyFrame.Value + 
                    (t * t * t - 2 * t * t + t) * keyFrame.Out + 
                    (-2 * t * t * t + 3 * t * t) * nextKeyFrame.Value +
                    (t * t * t - t * t) * nextKeyFrame.In;
            }
        }

        return KeyFrames[^1].Value;
    }

    public bool Resize(int count)
    {
        return false;
    }
}
