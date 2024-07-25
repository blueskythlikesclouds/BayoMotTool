namespace BayoMotTool;

public interface IInterpolation
{
    void ReadBayo2(BinaryReader reader, int count);
    void WriteBayo1(BinaryWriter writer);
    float Interpolate(float frame);
}
