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

public record BoneToReorient(
    int BoneIndex,
    bool IsYZX);

public record BoneToDuplicate(
    int SourceBoneIndex,
    int DestinationBoneIndex);

public class BoneConfig
{
    public Dictionary<int, int> BoneMap { get; set; }
    public bool RemoveUnmappedBones { get; set; }
    public int CameraId { get; set; } = -1;
    public List<BoneToAttach> BonesToAttach { get; set; }
    public List<BoneToCreate> BonesToCreate { get; set; }
    public List<BoneToReorient> BonesToReorient { get; set; }
    public List<BoneToDuplicate> BonesToDuplicate { get; set; }

    public void PrintDuplicates()
    {
        foreach (var group in BoneMap.GroupBy(x => x.Value).Where(x => x.Count() > 1))
        {
            Console.WriteLine(group.Key);
            foreach (var v in group)
                Console.WriteLine("  {0}", v.Key);
        }
    }

    public void InvertConfig()
    {
        if (BoneMap != null)
            BoneMap = BoneMap.DistinctBy(x => x.Value).ToDictionary(x => x.Value, x => x.Key);

        if (BonesToAttach != null)
        { 
            BonesToAttach = BonesToAttach.Select(x => new BoneToAttach(
                BoneMap?[x.ParentBoneIndex] ?? x.ParentBoneIndex,
                x.ParentBoneTranslationX,
                x.ParentBoneTranslationY,
                x.ParentBoneTranslationZ,
                BoneMap?[x.BoneIndex] ?? x.BoneIndex,
                0.0f,
                0.0f,
                0.0f,
                false)).ToList();
        }

        if (BoneMap.TryGetValue(CameraId, out var cameraId))
            CameraId = cameraId;
        else
            CameraId = -1;

        BonesToCreate = null;

        if (BonesToReorient != null)
            BonesToReorient = BonesToReorient.Select(x => new BoneToReorient(x.BoneIndex, true)).ToList();

        BonesToDuplicate = null;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(BoneConfig))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}