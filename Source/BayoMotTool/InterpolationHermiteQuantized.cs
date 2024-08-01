namespace BayoMotTool;

public class InterpolationHermiteQuantized : IInterpolation
{
    public struct KeyFrame
    {
        public ushort Frame;
        public ushort Value;
        public ushort In;
        public ushort Out;

        public override string ToString() => $"{Frame}";
    }

    public float ValueBias { get; set; }
    public float ValueScale { get; set; }
    public float InBias { get; set; }
    public float InScale { get; set; }
    public float OutBias { get; set; }
    public float OutScale { get; set; }
    public KeyFrame[] KeyFrames { get; set; }

    public void ReadBayo1(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadSingle();
        ValueScale = reader.ReadSingle();
        InBias = reader.ReadSingle();
        InScale = reader.ReadSingle();
        OutBias = reader.ReadSingle();
        OutScale = reader.ReadSingle();

        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Frame = reader.ReadUInt16();
            keyFrame.Value = reader.ReadUInt16();
            keyFrame.In = reader.ReadUInt16();
            keyFrame.Out = reader.ReadUInt16();
        }
    }

    public void ReadBayo2(BinaryReader reader, int count)
    {
        ReadBayo1(reader, count);
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);
        writer.Write(InBias);
        writer.Write(InScale);
        writer.Write(OutBias);
        writer.Write(OutScale);

        foreach (var keyFrame in KeyFrames)
        {
            writer.Write(keyFrame.Frame);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }

    public float Interpolate(float frame)
    {
        ref var firstKeyFrame = ref KeyFrames[0];

        if (frame <= firstKeyFrame.Frame)
            return ValueBias + ValueScale * firstKeyFrame.Value;

        for (int i = 0; i < KeyFrames.Length - 1; i++)
        {
            ref var keyFrame = ref KeyFrames[i];
            ref var nextKeyFrame = ref KeyFrames[i + 1];

            if (keyFrame.Frame <= frame && frame <= nextKeyFrame.Frame)
            {
                float t = (frame - keyFrame.Frame) / (nextKeyFrame.Frame - keyFrame.Frame);

                return
                    (2 * t * t * t - 3 * t * t + 1) * (ValueBias + ValueScale * keyFrame.Value) +
                    (t * t * t - 2 * t * t + t) * (OutBias + OutScale * keyFrame.Out) +
                    (-2 * t * t * t + 3 * t * t) * (ValueBias + ValueScale * nextKeyFrame.Value) +
                    (t * t * t - t * t) * (InBias + InScale * nextKeyFrame.In);
            }
        }

        return ValueBias + ValueScale * KeyFrames[^1].Value;
    }

    public bool Resize(int count)
    {
        return false;
    }
}
