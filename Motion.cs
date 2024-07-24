namespace BayoMotTool;

public interface IRecordValue
{
    void Read(BinaryReader reader, int count);
    void Write(BinaryWriter writer);
}

public class RecordValueConstant : IRecordValue
{
    public float Value { get; set; }

    public void Read(BinaryReader reader, int count)
    {
    }

    public void Write(BinaryWriter writer)
    {
    }

    public float Sample(float time)
    {
        return Value;
    }
}

public class RecordValueNull : IRecordValue
{
    public void Read(BinaryReader reader, int count)
    {
    }

    public void Write(BinaryWriter writer)
    {
    }

    public float Sample(float time)
    {
        return 0.0f;
    }
}

public class RecordValueSingle : IRecordValue
{
    public float[] Values { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        Values = new float[count];

        for (int i = 0; i < count; i++)
            Values[i] = reader.ReadSingle();
    }

    public void Write(BinaryWriter writer)
    {
        foreach (float value in Values)
            writer.Write(value);
    }
}

public class RecordValueSingleQuantized : IRecordValue
{
    public float ValueBias { get; set; }
    public float ValueScale { get; set; }
    public ushort[] Values { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadSingle();
        ValueScale = reader.ReadSingle();

        Values = new ushort[count];

        for (int i = 0; i < count; i++)
            Values[i] = reader.ReadUInt16();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);

        foreach (ushort value in Values)
            writer.Write(value);
    }
}

public class RecordValueHalfQuantized : IRecordValue
{
    public ushort ValueBias { get; set; }
    public ushort ValueScale { get; set; }
    public byte[] Values { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadUInt16();
        ValueScale = reader.ReadUInt16();
        Values = reader.ReadBytes(count);
    }

    public static uint ConvertHalfToSingle(ushort customFloat)
    {
        uint sign = (uint)((customFloat & 0x8000) >> 15);

        int exponent = (customFloat & 0x7E00) >> 9;
        exponent -= 47;

        uint significand = (uint)(customFloat & 0x01FF);

        if (exponent == -47)
        {
            if (significand == 0)
            {
                return sign == 0 ? 0 : 0x80000000;
            }
            else
            {
                while ((significand & 0x0200) == 0)
                {
                    significand <<= 1;
                    exponent--;
                }
                significand &= 0x01FF;
                exponent++;
            }
        }

        exponent += 127;
        uint result = (sign << 31) | ((uint)exponent << 23) | (significand << 14);
        float value = BitConverter.UInt32BitsToSingle(result);
        return result;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ConvertHalfToSingle(ValueBias));
        writer.Write(ConvertHalfToSingle(ValueScale));
        writer.Write(Values);
    }
}

public class RecordValueSingleHermite : IRecordValue
{
    public struct KeyFrame
    {
        public ushort Index; // Absolute
        public float Value;
        public float In;
        public float Out;
    }

    public KeyFrame[] KeyFrames { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Index = reader.ReadUInt16();
            _ = reader.ReadUInt16();
            keyFrame.Value = reader.ReadSingle();
            keyFrame.In = reader.ReadSingle();
            keyFrame.Out = reader.ReadSingle();
        }
    }

    public void Write(BinaryWriter writer)
    {
        foreach (var keyFrame in KeyFrames)
        {
            writer.Write(keyFrame.Index);
            writer.Write((ushort)0);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }
}

public class RecordValueSingleHermiteQuantized : IRecordValue
{
    public struct KeyFrame
    {
        public ushort Index; // Absolute
        public ushort Value;
        public ushort In;
        public ushort Out;
    }

    public float ValueBias { get; set; }
    public float ValueScale { get; set; }
    public float InBias { get; set; }
    public float InScale { get; set; }
    public float OutBias { get; set; }
    public float OutScale { get; set; }
    public KeyFrame[] KeyFrames { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadSingle();
        ValueScale = reader.ReadSingle();
        InBias = reader.ReadSingle();
        InScale = reader.ReadSingle();
        OutBias = reader.ReadSingle();
        OutScale = reader.ReadSingle();

        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Index = reader.ReadUInt16();
            keyFrame.Value = reader.ReadUInt16();
            keyFrame.In = reader.ReadUInt16();
            keyFrame.Out = reader.ReadUInt16();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);
        writer.Write(InBias);
        writer.Write(InScale);
        writer.Write(OutBias);
        writer.Write(OutScale);

        foreach (var keyFrame in KeyFrames)
        {
            writer.Write(keyFrame.Index);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }
}

public class RecordValueHalfHermiteQuantized : IRecordValue
{
    public struct KeyFrame
    {
        public byte Index; // Absolute
        public byte Value;
        public byte In;
        public byte Out;
    }

    public ushort ValueBias { get; set; }
    public ushort ValueScale { get; set; }
    public ushort InBias { get; set; }
    public ushort InScale { get; set; }
    public ushort OutBias { get; set; }
    public ushort OutScale { get; set; }
    public KeyFrame[] KeyFrames { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadUInt16();
        ValueScale = reader.ReadUInt16();
        InBias = reader.ReadUInt16();
        InScale = reader.ReadUInt16();
        OutBias = reader.ReadUInt16();
        OutScale = reader.ReadUInt16();

        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Index = reader.ReadByte();
            keyFrame.Value = reader.ReadByte();
            keyFrame.In = reader.ReadByte();
            keyFrame.Out = reader.ReadByte();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);
        writer.Write(InBias);
        writer.Write(InScale);
        writer.Write(OutBias);
        writer.Write(OutScale);

        foreach (var keyFrame in KeyFrames)
        {
            writer.Write((ushort)keyFrame.Index);
            writer.Write((byte)0);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }
}

public class RecordValueHalfHermiteQuantizedRelative : IRecordValue
{
    public struct KeyFrame
    {
        public byte Index; // Absolute
        public byte Value;
        public byte In;
        public byte Out;
    }

    public ushort ValueBias { get; set; }
    public ushort ValueScale { get; set; }
    public ushort InBias { get; set; }
    public ushort InScale { get; set; }
    public ushort OutBias { get; set; }
    public ushort OutScale { get; set; }
    public KeyFrame[] KeyFrames { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadUInt16();
        ValueScale = reader.ReadUInt16();
        InBias = reader.ReadUInt16();
        InScale = reader.ReadUInt16();
        OutBias = reader.ReadUInt16();
        OutScale = reader.ReadUInt16();

        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Index = reader.ReadByte();
            keyFrame.Value = reader.ReadByte();
            keyFrame.In = reader.ReadByte();
            keyFrame.Out = reader.ReadByte();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);
        writer.Write(InBias);
        writer.Write(InScale);
        writer.Write(OutBias);
        writer.Write(OutScale);

        foreach (var keyFrame in KeyFrames)
        {
            writer.Write(keyFrame.Index);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }
}

public class RecordValueHalfHermiteQuantizedAbsolute : IRecordValue
{
    public struct KeyFrame
    {
        public ushort Index;
        public byte Value;
        public byte In;
        public byte Out;
    }

    public ushort ValueBias { get; set; }
    public ushort ValueScale { get; set; }
    public ushort InBias { get; set; }
    public ushort InScale { get; set; }
    public ushort OutBias { get; set; }
    public ushort OutScale { get; set; }
    public KeyFrame[] KeyFrames { get; set; }

    public void Read(BinaryReader reader, int count)
    {
        ValueBias = reader.ReadUInt16();
        ValueScale = reader.ReadUInt16();
        InBias = reader.ReadUInt16();
        InScale = reader.ReadUInt16();
        OutBias = reader.ReadUInt16();
        OutScale = reader.ReadUInt16();

        KeyFrames = new KeyFrame[count];

        for (int i = 0; i < count; i++)
        {
            ref var keyFrame = ref KeyFrames[i];

            keyFrame.Index = reader.ReadUInt16();
            keyFrame.Value = reader.ReadByte();
            keyFrame.In = reader.ReadByte();
            keyFrame.Out = reader.ReadByte();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ValueBias);
        writer.Write(ValueScale);
        writer.Write(InBias);
        writer.Write(InScale);
        writer.Write(OutBias);
        writer.Write(OutScale);

        foreach (var keyFrame in KeyFrames)
        {
            writer.Write(keyFrame.Index);
            writer.Write((byte)0);
            writer.Write(keyFrame.Value);
            writer.Write(keyFrame.In);
            writer.Write(keyFrame.Out);
        }
    }
}

public class Record
{
    public ushort BoneIndex { get; set; }
    public byte ComponentIndex { get; set; }
    public ushort ValueCount { get; set; }
    public ushort Unknown { get; set; }
    public IRecordValue Value { get; set; }

    public void Read(BinaryReader reader, long recordOffset)
    {
        reader.BaseStream.Seek(recordOffset, SeekOrigin.Begin);

        BoneIndex = reader.ReadUInt16();
        ComponentIndex = reader.ReadByte();
        byte valueType = reader.ReadByte();
        ValueCount = reader.ReadUInt16();
        Unknown = reader.ReadUInt16();

        if (valueType == 0)
        {
            Value = new RecordValueConstant
            {
                Value = reader.ReadSingle()
            };
        }
        else if (valueType == 0xFF)
        {
            Value = new RecordValueNull();
        }
        else
        {
            switch (valueType)
            {
                case 1: Value = new RecordValueSingle(); break;
                case 2: Value = new RecordValueSingleQuantized(); break;
                case 3: Value = new RecordValueHalfQuantized(); break;
                case 4: Value = new RecordValueSingleHermite(); break;
                case 5: Value = new RecordValueSingleHermiteQuantized(); break;
                case 6: Value = new RecordValueHalfHermiteQuantized(); break;
                case 7: Value = new RecordValueHalfHermiteQuantizedRelative(); break;
                case 8: Value = new RecordValueHalfHermiteQuantizedAbsolute(); break;
                default: throw new ArgumentOutOfRangeException(nameof(valueType));
            }

            reader.BaseStream.Seek(recordOffset + reader.ReadUInt32(), SeekOrigin.Begin);

            Value.Read(reader, ValueCount);
        }
    }

    public void Write(BinaryWriter writer, long valueOffset)
    {
        byte valueType;

        if (Value is RecordValueConstant) valueType = 0;
        else if (Value is RecordValueSingle) valueType = 1;
        else if (Value is RecordValueSingleHermite) valueType = 2;
        else if (Value is RecordValueSingleQuantized) valueType = 3;
        else if (Value is RecordValueSingleHermiteQuantized) valueType = 4;
        else if (Value is RecordValueHalfQuantized) valueType = 5;
        else if (Value is RecordValueHalfHermiteQuantizedRelative) valueType = 6;
        else if (Value is RecordValueHalfHermiteQuantized || Value is RecordValueHalfHermiteQuantizedAbsolute) valueType = 7;
        else if (Value is RecordValueNull) valueType = 0xFF;
        else throw new InvalidDataException();

        writer.Write(BoneIndex);
        writer.Write(ComponentIndex);
        writer.Write(valueType);
        writer.Write(ValueCount);
        writer.Write(Unknown);

        if (Value is RecordValueConstant valueConstant)
        {
            writer.Write(valueConstant.Value);
        }
        else if (Value is not RecordValueNull)
        {
            writer.Write((uint)valueOffset);
        }
    }
}

public class Motion
{
    public ushort Flags { get; set; }
    public ushort FrameCount { get; set; }
    public List<Record> Records { get; set; } = [];

    public void Read(BinaryReader reader)
    {
        uint signature = reader.ReadUInt32();
        uint hash = reader.ReadUInt32();
        Flags = reader.ReadUInt16();
        FrameCount = reader.ReadUInt16();
        uint recordOffset = reader.ReadUInt32();
        uint recordCount = reader.ReadUInt32();

        for (int i = 0; i < recordCount; i++)
        {
            var record = new Record();
            record.Read(reader, recordOffset + i * 12);
            Records.Add(record);
        }
    }

    public void Write(BinaryWriter writer)
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
            Records[i].Value.Write(writer);

            while ((writer.BaseStream.Position % 4) != 0)
                writer.Write((byte)0);
        }

        for (int i = 0; i < Records.Count; i++)
        {
            writer.BaseStream.Seek(16 + i * 12, SeekOrigin.Begin);

            Records[i].Write(writer, recordOffsets[i]);
        }
    }

    public void Load(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);
        Read(reader);
    }

    public void Save(string filePath)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);
        Write(writer);
    }
}
