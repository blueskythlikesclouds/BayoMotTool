namespace BayoMotTool;

public class InterpolationNone : IInterpolation
{
    public void ReadBayo2(BinaryReader reader, int count)
    {
    }

    public void WriteBayo1(BinaryWriter writer)
    {
    }

    public float Interpolate(float frame)
    {
        return 0.0f;
    }
}
