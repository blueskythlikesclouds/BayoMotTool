namespace BayoMotTool;

public class Motion
{
    public ushort Flags { get; set; }
    public ushort FrameCount { get; set; }
    public List<Record> Records { get; set; } = [];

    public void ReadBayo2(BinaryReader reader)
    {
        uint signature = reader.ReadUInt32();
        uint version = reader.ReadUInt32();
        Flags = reader.ReadUInt16();
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
        writer.Write((ushort)0x1);
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

    public void LoadBayo2(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);
        ReadBayo2(reader);
    }

    public void SaveBayo1(string filePath)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);
        WriteBayo1(writer);
    }
}
