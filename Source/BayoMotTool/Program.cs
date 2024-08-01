using BayoMotTool;
using System.Numerics;
using System.Text.Json;

string inputFilePath = null;
string outputFilePath = null;
string jsonFilePath = null;

foreach (var arg in args)
{
    if (arg.EndsWith(".mot", StringComparison.OrdinalIgnoreCase))
    {
        if (inputFilePath == null)
            inputFilePath = arg;
        else if (outputFilePath == null)
            outputFilePath = arg;
    }
    else if (arg.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        jsonFilePath = arg;
}

if (inputFilePath == null)
{
    Console.WriteLine("Usage: [input] [output] [json]");
    return;
}

if (jsonFilePath == null)
{
    jsonFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), "BoneConfig.json");
    if (!File.Exists(jsonFilePath))
        jsonFilePath = "BoneConfig.json";
}

var boneConfig = JsonSerializer.Deserialize(
    File.ReadAllText(jsonFilePath), SourceGenerationContext.Default.BoneConfig);

var motion = new Motion();
motion.LoadBayo2(inputFilePath);

motion.Records.RemoveAll(x => x.BoneIndex == 0x7FFF || (boneConfig.BoneMap != null && boneConfig.RemoveUnmappedBones && 
    x.BoneIndex != 0xFFFF && !boneConfig.BoneMap.ContainsKey(x.BoneIndex)));

foreach (var record in motion.Records)
{
    if (boneConfig.BoneMap != null && boneConfig.BoneMap.TryGetValue(record.BoneIndex, out var boneIndex))
        record.BoneIndex = (ushort)boneIndex;

    if (record.Interpolation is InterpolationConstant)
        record.FrameCount = 2;
}

if (boneConfig.BonesToCreate != null)
{ 
    foreach (var boneToCreate in boneConfig.BonesToCreate)
        MotionUtility.AddDefaultRecords(motion, boneToCreate.BoneIndex, new Vector3(boneToCreate.BoneTranslationX, boneToCreate.BoneTranslationY, boneToCreate.BoneTranslationZ));
}

if (boneConfig.BonesToAttach != null)
{ 
    foreach (var boneToAttach in boneConfig.BonesToAttach)
    { 
        MotionUtility.AttachBone(motion,
            boneToAttach.ParentBoneIndex, 
            new Vector3(boneToAttach.ParentBoneTranslationX, boneToAttach.ParentBoneTranslationY, boneToAttach.ParentBoneTranslationZ),
            boneToAttach.BoneIndex, 
            new Vector3(boneToAttach.BoneTranslationX, boneToAttach.BoneTranslationY, boneToAttach.BoneTranslationZ),
            boneToAttach.InvertParentTransform);
    }
}

if (boneConfig.BonesToReorient != null)
{ 
    foreach (int boneToReorient in boneConfig.BonesToReorient)
        MotionUtility.ReorientBone(motion, boneToReorient);
}

if (boneConfig.BonesToDuplicate != null)
{
    foreach (var boneToDuplicate in boneConfig.BonesToDuplicate)
    {
        motion.Records.RemoveAll(x => x.BoneIndex == boneToDuplicate.DestinationBoneIndex);

        var duplicatedRecords = motion.Records.Where(x => x.BoneIndex == boneToDuplicate.SourceBoneIndex).Select(x => new Record
        {
            BoneIndex = (ushort)boneToDuplicate.DestinationBoneIndex,
            AnimationTrack = x.AnimationTrack,
            FrameCount = x.FrameCount,
            Interpolation = x.Interpolation
        }).ToList();

        motion.Records.AddRange(duplicatedRecords);
    }
}

MotionUtility.SortRecords(motion);

motion.SaveBayo1(outputFilePath ?? inputFilePath);