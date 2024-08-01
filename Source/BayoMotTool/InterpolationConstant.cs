namespace BayoMotTool;

public class InterpolationConstant : IInterpolation
{
    public float Value { get; set; }

    public void ReadBayo1(BinaryReader reader, int count)
    {
    }

    public void ReadBayo2(BinaryReader reader, int count)
    {
    }

    public void WriteBayo1(BinaryWriter writer)
    {
    }

    public float Interpolate(float frame)
    {
        return Value;
    }

    public bool Resize(int count)
    {
        return false;
    }

    public override string ToString() => $"{Value}";
}
