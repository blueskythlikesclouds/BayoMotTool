using System.Buffers.Binary;
using System.Numerics;

namespace BayoMotTool;

public class Model
{
    public List<short> BoneIndexTable { get; set; } = [];
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

        return 0xFFF;
    }

    public void ReadBayo1(EndianBinaryReader reader)
    {
        reader.BaseStream.Seek(0x30, SeekOrigin.Begin);

        int boneCount = reader.ReadInt32();
        _ = reader.ReadInt32();
        uint boneRelativePositionsOffset = reader.ReadUInt32();

        if (boneRelativePositionsOffset > reader.BaseStream.Length)
        {
            boneCount = BinaryPrimitives.ReverseEndianness(boneCount);
            boneRelativePositionsOffset = BinaryPrimitives.ReverseEndianness(boneRelativePositionsOffset);

            reader.IsBigEndian = true;
        }

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

        int j = 0;
        for (int i = 0; i < 16; i++)
        {
            short index = reader.ReadInt16();
            if (index != -1)
                ++j;

            BoneIndexTable.Add(index);
        }

        int k = 0;
        for (int i = 0; i < j * 16; i++)
        {
            short index = reader.ReadInt16();
            if (index != -1)
                ++k;

            BoneIndexTable.Add(index);
        }

        for (int i = 0; i < k * 16; i++)
            BoneIndexTable.Add(reader.ReadInt16());
    }

    public void LoadBayo1(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new EndianBinaryReader(stream);
        ReadBayo1(reader);
    }
}
