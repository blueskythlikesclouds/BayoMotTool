using BayoMotTool;
using System.Numerics;
using System.Text.Json;

var boneConfig = JsonSerializer.Deserialize(
    File.ReadAllText("BoneConfig.json"), SourceGenerationContext.Default.BoneConfig);

var motion = new Motion();
motion.LoadBayo2(args[0]);

motion.Records.RemoveAll(x => x.BoneIndex == 0x7FFF || (x.BoneIndex != 0xFFFF && !boneConfig.BoneMap.ContainsKey(x.BoneIndex)));

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

motion.SaveBayo1(args.Length > 1 ? args[1] : args[0]);