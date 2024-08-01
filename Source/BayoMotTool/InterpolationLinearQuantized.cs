namespace BayoMotTool;

public class InterpolationLinearQuantized : IInterpolation
{
    public float ValueBias { get; set; }
    public float ValueScale { get; set; }
    public ushort[] Values { get; set; }

    public void ReadBayo1(BinaryReader reader, int count)
    {
        throw new NotImplementedException();
    }

    public void ReadBayo2(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadSingle();
        ValueScale = reader.ReadSingle();

        Values = new ushort[count];

        for (int i = 0; i < count; i++)
            Values[i] = reader.ReadUInt16();
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);

        foreach (ushort value in Values)
            writer.Write(value);
    }

    public float Interpolate(float frame)
    {
        int index = (int)frame;
        if (index >= Values.Length - 1)
            return ValueBias + ValueScale * Values[^1];

        float v0 = ValueBias + ValueScale * Values[index];
        float v1 = ValueBias + ValueScale * Values[index + 1];

        return v0 + (v1 - v0) * (frame - index);
    }
}
