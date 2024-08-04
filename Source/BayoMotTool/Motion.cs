using System.Buffers.Binary;

namespace BayoMotTool;

public enum MotionFormat
{
    Bayonetta1,
    Bayonetta2
}

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
        Flags = reader.ReadUInt16();
        FrameCount = reader.ReadUInt16();
        uint recordOffset = reader.ReadUInt32();

        if (recordOffset > reader.BaseStream.Length)
        {
            Flags = BinaryPrimitives.ReverseEndianness(FrameCount);
            FrameCount = BinaryPrimitives.ReverseEndianness(FrameCount);
            recordOffset = BinaryPrimitives.ReverseEndianness(recordOffset);
            reader.IsBigEndian = true;
        }

        uint recordCount = reader.ReadUInt32();

        for (int i = 0; i < recordCount; i++)
        {
            var record = new Record();
            record.ReadBayo2(reader, recordOffset + i * 12);
            Records.Add(record);
        }
    }

    public MotionFormat Read(EndianBinaryReader reader)
    {
        reader.BaseStream.Seek(8, SeekOrigin.Begin);
        uint recordOffset = reader.ReadUInt32();
        var motionFormat = recordOffset != 0x10 && recordOffset != 0x10000000 ? MotionFormat.Bayonetta2 : MotionFormat.Bayonetta1;

        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        if (motionFormat == MotionFormat.Bayonetta2)
            ReadBayo2(reader);
        else 
            ReadBayo1(reader);

        return motionFormat;
    }

    public void WriteBayo1(BinaryWriter writer)
    {
        const int headerSize = 0x10;

        writer.Write(0x746F6D);
        writer.Write(Flags);
        writer.Write(FrameCount);
        writer.Write(headerSize);
        writer.Write(Records.Count);

        var recordOffsets = new long[Records.Count];

        writer.BaseStream.Seek(headerSize + Records.Count * 12, SeekOrigin.Begin);
        for (int i = 0; i < Records.Count; i++)
        {
            recordOffsets[i] = writer.BaseStream.Position;
            Records[i].Interpolation.WriteBayo1(writer);

            while ((writer.BaseStream.Position % 4) != 0)
                writer.Write((byte)0);
        }

        for (int i = 0; i < Records.Count; i++)
        {
            writer.BaseStream.Seek(headerSize + i * 12, SeekOrigin.Begin);

            Records[i].WriteBayo1(writer, recordOffsets[i]);
        }
    }

    public void WriteBayo2(BinaryWriter writer)
    {
        const int headerSize = 0x2C;

        writer.Write(0x746F6D);
        writer.Write(0x20120405);
        writer.Write(Flags);
        writer.Write(FrameCount);
        writer.Write(headerSize);
        writer.Write(Records.Count);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);

        var recordOffsets = new long[Records.Count];

        writer.BaseStream.Seek(headerSize + Records.Count * 12, SeekOrigin.Begin);
        for (int i = 0; i < Records.Count; i++)
        {
            recordOffsets[i] = writer.BaseStream.Position - (headerSize + i * 12);
            Records[i].Interpolation.WriteBayo2(writer);

            while ((writer.BaseStream.Position % 4) != 0)
                writer.Write((byte)0);
        }

        for (int i = 0; i < Records.Count; i++)
        {
            writer.BaseStream.Seek(headerSize + i * 12, SeekOrigin.Begin);

            Records[i].WriteBayo2(writer, recordOffsets[i]);
        }
    }
    
    public void Write(BinaryWriter writer, MotionFormat format)
    {
        if (format == MotionFormat.Bayonetta2)
            WriteBayo2(writer);
        else
            WriteBayo1(writer);
    }

    public MotionFormat Load(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new EndianBinaryReader(stream);
        return Read(reader);
    }

    public void Save(string filePath, MotionFormat format)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);
        Write(writer, format);
    }
}
