namespace BayoMotTool;

public interface IInterpolation
{
    void ReadBayo1(BinaryReader reader, int count);
    void ReadBayo2(BinaryReader reader, int count);
    void WriteBayo1(BinaryWriter writer);
    float Interpolate(float frame);
    bool Resize(int count);
}
