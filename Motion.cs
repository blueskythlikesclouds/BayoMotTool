using System.Buffers.Binary;

namespace BayoMotTool;

public class Motion
{
    public ushort Flags { get; set; }
    public ushort FrameCount { get; set; }
    public List<Record> Records { get; set; } = [];

    public void ReadBayo1(EndianBinaryReader reader)
    {
        uint signature = reader.ReadUInt32();
        Flags = reader.ReadUInt16();
        FrameCount = reader.ReadUInt16();
        uint recordOffset = reader.ReadUInt32();

        if (recordOffset != 0x10)
        {
            Flags = BinaryPrimitives.ReverseEndianness(Flags);
            FrameCount = BinaryPrimitives.ReverseEndianness(FrameCount);
            recordOffset = BinaryPrimitives.ReverseEndianness(recordOffset);
            reader.IsBigEndian = true;
        }

        uint recordCount = reader.ReadUInt32();

        for (int i = 0; i < recordCount; i++)
        {
            reader.BaseStream.Seek(recordOffset + i * 12, SeekOrigin.Begin);

            var record = new Record();
            record.ReadBayo1(reader);
            Records.Add(record);
        }
    }

    public void ReadBayo2(EndianBinaryReader reader)
    {
        uint signature = reader.ReadUInt32();
        uint version = reader.ReadUInt32();
        reader.IsBigEndian = version != 0x20120405;
        Flags = reader.ReadUInt16();
        Flags = 0x1;
        FrameCount = reader.ReadUInt16();
        uint recordOffset = reader.ReadUInt32();
        uint recordCount = reader.ReadUInt32();

        for (int i = 0; i < recordCount; i++)
        {
            var record = new Record();
            record.ReadBayo2(reader, recordOffset + i * 12);
            Records.Add(record);
        }
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        writer.Write(0x746F6D);
        writer.Write(Flags);
        writer.Write(FrameCount);
        writer.Write(0x10);
        writer.Write(Records.Count);

        var recordOffsets = new long[Records.Count];

        writer.BaseStream.Seek(16 + Records.Count * 12, SeekOrigin.Begin);
        for (int i = 0; i < Records.Count; i++)
        {
            recordOffsets[i] = writer.BaseStream.Position;
            Records[i].Interpolation.WriteBayo1(writer);

            while ((writer.BaseStream.Position % 4) != 0)
                writer.Write((byte)0);
        }

        for (int i = 0; i < Records.Count; i++)
        {
            writer.BaseStream.Seek(16 + i * 12, SeekOrigin.Begin);

            Records[i].WriteBayo1(writer, recordOffsets[i]);
        }
    }

    public void LoadBayo1(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new EndianBinaryReader(stream);
        ReadBayo1(reader);
    }

    public void LoadBayo2(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new EndianBinaryReader(stream);
        ReadBayo2(reader);
    }

    public void SaveBayo1(string filePath)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);
        WriteBayo1(writer);
    }
}
