namespace BayoMotTool;

public interface IInterpolation
{
    void ReadBayo1(EndianBinaryReader reader, int count);
    void ReadBayo2(EndianBinaryReader reader, int count);
    void WriteBayo1(EndianBinaryWriter writer);
    void WriteBayo2(EndianBinaryWriter writer);
    float Interpolate(float frame);
    bool Resize(int count);
}
