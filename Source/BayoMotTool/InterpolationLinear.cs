namespace BayoMotTool;

public class InterpolationLinear : IInterpolation
{
    public float[] Values { get; set; }

    public void ReadBayo1(BinaryReader reader, int count)
    {
        Values = new float[count];

        for (int i = 0; i < count; i++)
            Values[i] = reader.ReadSingle();
    }

    public void ReadBayo2(BinaryReader reader, int count)
    {
        ReadBayo1(reader, count);
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        foreach (float value in Values)
            writer.Write(value);
    }

    public void WriteBayo2(BinaryWriter writer)
    {
        WriteBayo1(writer);
    }

    public float Interpolate(float frame)
    {
        int index = (int)frame;
        if (index >= Values.Length - 1)
            return Values[^1];

        float v0 = Values[index];
        float v1 = Values[index + 1];

        return v0 + (v1 - v0) * (frame - index);
    }

    public bool Resize(int count)
    {
        var values = new float[count];

        for (int i = 0; i < count; i++)
            values[i] = Values[int.Min(i, Values.Length - 1)];

        Values = values;

        return true;
    }
}
