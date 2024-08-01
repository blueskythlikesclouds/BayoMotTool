namespace BayoMotTool;

public class InterpolationLinear : IInterpolation
{
    public float[] Values { get; set; }

    public void ReadBayo1(BinaryReader reader, int count)
    {
        throw new NotImplementedException();
    }

    public void ReadBayo2(BinaryReader reader, int count)
    {
        Values = new float[count];

        for (int i = 0; i < count; i++)
            Values[i] = reader.ReadSingle();
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        foreach (float value in Values)
            writer.Write(value);
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
}
