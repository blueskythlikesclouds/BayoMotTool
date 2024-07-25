namespace BayoMotTool;

public class InterpolationHermiteQuantizedHalf2 : IInterpolation
{
    public struct KeyFrame
    {
        public ushort Frame;
        public byte Value;
        public byte In;
        public byte Out;
    }

    public ushort ValueBias { get; set; }
    public ushort ValueScale { get; set; }
    public ushort InBias { get; set; }
    public ushort InScale { get; set; }
    public ushort OutBias { get; set; }
    public ushort OutScale { get; set; }
    public KeyFrame[] KeyFrames { get; set; }

    public void ReadBayo2(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadUInt16();
        ValueScale = reader.ReadUInt16();
        InBias = reader.ReadUInt16();
        InScale = reader.ReadUInt16();
        OutBias = reader.ReadUInt16();
        OutScale = reader.ReadUInt16();

        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Frame = reader.ReadUInt16();
            keyFrame.Value = reader.ReadByte();
            keyFrame.In = reader.ReadByte();
            keyFrame.Out = reader.ReadByte();
        }
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
            writer.Write((byte)0);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }

    public float Interpolate(float frame)
    {
        float valueBias = PgHalf.ToSingle(ValueBias);
        float valueScale = PgHalf.ToSingle(ValueScale);

        ref var firstKeyFrame = ref KeyFrames[0];

        if (frame <= firstKeyFrame.Frame)
            return valueBias + valueScale * firstKeyFrame.Value;

        float inBias = PgHalf.ToSingle(InBias);
        float inScale = PgHalf.ToSingle(InScale);
        float outBias = PgHalf.ToSingle(OutBias);
        float outScale = PgHalf.ToSingle(OutScale);

        for (int i = 0; i < KeyFrames.Length - 1; i++)
        {
            ref var keyFrame = ref KeyFrames[i];
            ref var nextKeyFrame = ref KeyFrames[i + 1];

            if (keyFrame.Frame <= frame && frame <= nextKeyFrame.Frame)
            {
                float t = (frame - keyFrame.Frame) / (nextKeyFrame.Frame - keyFrame.Frame);

                return
                    (2 * t * t * t - 3 * t * t + 1) * (valueBias + valueScale * keyFrame.Value) +
                    (t * t * t - 2 * t * t + t) * (inBias + inScale * keyFrame.In) +
                    (-2 * t * t * t + 3 * t * t) * (valueBias + valueScale * nextKeyFrame.Value) +
                    (t * t * t - t * t) * (outBias + outScale * nextKeyFrame.Out);
            }
        }

        return valueBias + valueScale * KeyFrames[^1].Value;
    }
}
