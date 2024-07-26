using System.Numerics;
using System.Text.Json.Serialization;

namespace BayoMotTool;

public record BoneToCreate(
    int BoneIndex, 
    float BoneTranslationX,
    float BoneTranslationY, 
    float BoneTranslationZ);

public record BoneToAttach(
    int ParentBoneIndex,
    float ParentBoneTranslationX,
    float ParentBoneTranslationY,
    float ParentBoneTranslationZ,
    int BoneIndex, 
    float BoneTranslationX, 
    float BoneTranslationY, 
    float BoneTranslationZ,
    bool InvertParentTransform);

public record BoneToDuplicate(
    int SourceBoneIndex,
    int DestinationBoneIndex
    );

public class BoneConfig
{
    public Dictionary<int, int> BoneMap { get; set; }
    public bool RemoveUnmappedBones { get; set; }
    public List<BoneToCreate> BonesToCreate { get; set; }
    public List<BoneToAttach> BonesToAttach { get; set; }
    public List<int> BonesToReorient { get; set; }
    public List<BoneToDuplicate> BonesToDuplicate { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BoneConfig))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}