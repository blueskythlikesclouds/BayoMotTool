namespace BayoMotTool;

public class InterpolationNone : IInterpolation
{
    public void ReadBayo1(EndianBinaryReader reader, int count)
    {
    }

    public void ReadBayo2(EndianBinaryReader reader, int count)
    {
    }

    public void WriteBayo1(EndianBinaryWriter writer)
    {
    }

    public void WriteBayo2(EndianBinaryWriter writer)
    {
    }

    public float Interpolate(float frame)
    {
        return 0.0f;
    }

    public bool Resize(int count)
    {
        return false;
    }
}
