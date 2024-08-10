using BayoMotTool;
using System.Numerics;
using System.Text.Json;

string inputFilePath = null;
string outputFilePath = null;
string jsonFilePath = null;
bool isBigEndian = false;

foreach (var arg in args)
{
    if (arg.Equals("-b", StringComparison.OrdinalIgnoreCase) || arg.Equals("--big-endian", StringComparison.OrdinalIgnoreCase))
    { 
        isBigEndian = true;
    }
    else if (arg.EndsWith(".mot", StringComparison.OrdinalIgnoreCase))
    {
        if (inputFilePath == null)
            inputFilePath = arg;
        else if (outputFilePath == null)
            outputFilePath = arg;
    }
    else if (arg.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
    { 
        jsonFilePath = arg;
    }
}

if (inputFilePath == null)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  [options] [input] [output] [json]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -b or --big-endian: Save output as big endian");
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
var format = motion.Load(inputFilePath);
var targetFormat = format == MotionFormat.Bayonetta2 ? MotionFormat.Bayonetta1 : MotionFormat.Bayonetta2;

if (targetFormat == MotionFormat.Bayonetta2)
{ 
    boneConfig.InvertConfig();
    motion.Flags &= 0xFFFE;
    motion.Name = Path.GetFileNameWithoutExtension(outputFilePath ?? inputFilePath);
}
else
{
    motion.Flags |= 0x1;
}

motion.Records.RemoveAll(x => x.BoneIndex == 0x7FFF || (boneConfig.BoneMap != null && boneConfig.RemoveUnmappedBones && 
    x.BoneIndex != 0xFFFF && !boneConfig.BoneMap.ContainsKey(x.BoneIndex)));

foreach (var record in motion.Records)
{
    if (format == MotionFormat.Bayonetta1 && record.BoneIndex == boneConfig.CameraId)
    {
        if (record.AnimationTrack == AnimationTrack.RotationX)
            record.AnimationTrack = AnimationTrack.Fovy;
        if (record.AnimationTrack == AnimationTrack.RotationY)
            record.AnimationTrack = AnimationTrack.Roll;
    }
    
    if (boneConfig.BoneMap != null && boneConfig.BoneMap.TryGetValue(record.BoneIndex, out var boneIndex))
        record.BoneIndex = (ushort)boneIndex;

    if (record.Interpolation is InterpolationConstant)
        record.FrameCount = (ushort)(targetFormat == MotionFormat.Bayonetta2 ? 2 : 0);

    else if (targetFormat == MotionFormat.Bayonetta1 && record.Interpolation.Resize(motion.FrameCount))
        record.FrameCount = motion.FrameCount;

    if (targetFormat == MotionFormat.Bayonetta2)
        record.Unknown = 0;
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

if (boneConfig.BonesToCreate != null)
{
    foreach (var boneToCreate in boneConfig.BonesToCreate)
        MotionUtility.AddDefaultRecords(motion, boneToCreate.BoneIndex, new Vector3(boneToCreate.BoneTranslationX, boneToCreate.BoneTranslationY, boneToCreate.BoneTranslationZ));
}

if (boneConfig.BonesToReorient != null)
{ 
    foreach (var boneToReorient in boneConfig.BonesToReorient)
        MotionUtility.ReorientBone(motion, boneToReorient.BoneIndex, boneToReorient.IsYZX);
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

MotionUtility.SortRecords(motion, targetFormat == MotionFormat.Bayonetta1);

motion.Save(outputFilePath ?? inputFilePath, targetFormat, isBigEndian);