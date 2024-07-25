namespace BayoMotTool;

public class InterpolationLinearQuantizedHalf : IInterpolation
{
    public ushort ValueBias { get; set; }
    public ushort ValueScale { get; set; }
    public byte[] Values { get; set; }

    public void ReadBayo2(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadUInt16();
        ValueScale = reader.ReadUInt16();
        Values = reader.ReadBytes(count);
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        writer.Write(PgHalf.ToSingle(ValueBias));
        writer.Write(PgHalf.ToSingle(ValueScale));
        writer.Write(Values);
    }

    public float Interpolate(float frame)
    {
        float valueBias = PgHalf.ToSingle(ValueBias);
        float valueScale = PgHalf.ToSingle(ValueScale);

        int index = (int)frame;
        if (index >= Values.Length - 1)
            return valueBias + valueScale * Values[^1];

        float v0 = valueBias + valueScale * Values[index];
        float v1 = valueBias + valueScale * Values[index + 1];

        return v0 + (v1 - v0) * (frame - index);
    }
}
