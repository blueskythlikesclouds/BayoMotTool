using System.Numerics;
using System.Text.Json.Serialization;

namespace BayoMotTool;

public record BoneToCreate(int BoneIndex, float X, float Y, float Z);
public record BoneToAttach(int ParentBoneIndex, int BoneIndex, float X, float Y, float Z);

public class BoneConfig
{
    public Dictionary<int, int> BoneMap { get; set; }
    public bool RemoveUnmappedBones { get; set; }
    public List<BoneToCreate> BonesToCreate { get; set; }
    public List<BoneToAttach> BonesToAttach { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BoneConfig))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}