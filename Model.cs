using System.Numerics;

namespace BayoMotTool;

public class Model
{
    public short[] BoneIndexTable { get; set; }
    public Vector3[] BoneRelativePositions { get; set; }

    public int MapIndex(int boneIndex)
    {
        short index = BoneIndexTable[(boneIndex >> 8) & 0xF];
        if (index != -1)
        {
            index = BoneIndexTable[((boneIndex >> 4) & 0xF) + index];
            if (index != -1)
                return BoneIndexTable[(boneIndex & 0xF) + index];
        }

        return -1;
    }

    public void ReadBayo1(BinaryReader reader)
    {
        reader.BaseStream.Seek(0x30, SeekOrigin.Begin);

        int boneCount = reader.ReadInt32();
        _ = reader.ReadInt32();
        uint boneRelativePositionsOffset = reader.ReadUInt32();
        _ = reader.ReadInt32();
        uint boneIndexTableOffset = reader.ReadUInt32();

        reader.BaseStream.Seek(boneRelativePositionsOffset, SeekOrigin.Begin);

        BoneRelativePositions = new Vector3[boneCount];

        for (int i = 0; i < BoneRelativePositions.Length; i++)
        {
            ref var relativePosition = ref BoneRelativePositions[i];

            relativePosition.X = reader.ReadSingle();
            relativePosition.Y = reader.ReadSingle();
            relativePosition.Z = reader.ReadSingle();
        }

        reader.BaseStream.Seek(boneIndexTableOffset, SeekOrigin.Begin);

        BoneIndexTable = new short[272];

        for (int i = 0; i < BoneIndexTable.Length; i++)
            BoneIndexTable[i] = reader.ReadInt16();
    }

    public void LoadBayo1(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new EndianBinaryReader(stream);
        ReadBayo1(reader);
    }
}
