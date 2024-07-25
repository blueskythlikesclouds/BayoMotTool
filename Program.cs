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

motion.Records.RemoveAll(x => x.BoneIndex == 0x7FFF || (boneConfig.RemoveUnmappedBones && x.BoneIndex != 0xFFFF && !boneConfig.BoneMap.ContainsKey(x.BoneIndex)));

foreach (var record in motion.Records)
{
    if (record.BoneIndex != 0xFFFF)
        record.BoneIndex = (ushort)boneConfig.BoneMap[record.BoneIndex];

    if (record.Interpolation is InterpolationConstant)
        record.FrameCount = 2;
}

foreach (var boneToAttach in boneConfig.BonesToAttach)
    MotionUtility.AttachBone(motion, boneToAttach.ParentBoneIndex, boneToAttach.BoneIndex, new Vector3(boneToAttach.X, boneToAttach.Y, boneToAttach.Z));

foreach (var boneToCreate in boneConfig.BonesToCreate)
    MotionUtility.AddDefaultRecords(motion, boneToCreate.BoneIndex, new Vector3(boneToCreate.X, boneToCreate.Y, boneToCreate.Z));

MotionUtility.SortRecords(motion);

motion.SaveBayo1(outputFilePath ?? inputFilePath);