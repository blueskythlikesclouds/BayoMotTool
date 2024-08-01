namespace BayoMotTool;

public enum AnimationTrack
{
    TranslationX = 0,
    TranslationY = 1,
    TranslationZ = 2,
    RotationX = 3,
    RotationY = 4,
    RotationZ = 5,
    ScaleX = 7,
    ScaleY = 8,
    ScaleZ = 9
}

public class Record
{
    public ushort BoneIndex { get; set; }
    public AnimationTrack AnimationTrack { get; set; }
    public ushort FrameCount { get; set; }
    public ushort Unknown { get; set; }
    public IInterpolation Interpolation { get; set; }

    public override string ToString() => 
        $"{BoneIndex}, {AnimationTrack}, {FrameCount}, {Interpolation}";

    // 0
    // 4
    // 6
    // 7
    private static readonly Dictionary<Type, int> _bayo1InterpolationTypes = new()
    {
        { typeof(InterpolationConstant), 0 },
        { typeof(InterpolationLinear), 1 },
        { typeof(InterpolationHermite), 2 },
        { typeof(InterpolationLinearQuantized), 3 },
        { typeof(InterpolationHermiteQuantized), 4 },
        { typeof(InterpolationLinearQuantizedHalf), 5 },
        { typeof(InterpolationHermiteQuantizedHalfRelative), 6 },
        { typeof(InterpolationHermiteQuantizedHalf), 7 },
        { typeof(InterpolationHermiteQuantizedHalf2), 7 },
        { typeof(InterpolationNone), 255 },
    };

    private static readonly Dictionary<int, Func<IInterpolation>> _bayo1InterpolationFactory = new()
    {
        { 0, () => new InterpolationConstant() },
        { 4, () => new InterpolationHermiteQuantized() },
        { 6, () => new InterpolationHermiteQuantizedHalfRelative() },
        { 7, () => new InterpolationHermiteQuantizedHalf() },
        { 255, () => new InterpolationNone() }
    };

    private static readonly Dictionary<int, Func<IInterpolation>> _bayo2InterpolationFactory = new()
    {
        { 0, () => new InterpolationConstant() },
        { 1, () => new InterpolationLinear() },
        { 2, () => new InterpolationLinearQuantized() },
        { 3, () => new InterpolationLinearQuantizedHalf() },
        { 4, () => new InterpolationHermite() },
        { 5, () => new InterpolationHermiteQuantized() },
        { 6, () => new InterpolationHermiteQuantizedHalf() },
        { 7, () => new InterpolationHermiteQuantizedHalfRelative() },
        { 8, () => new InterpolationHermiteQuantizedHalf2() },
        { 255, () => new InterpolationNone() }
    };

    public void ReadBayo1(BinaryReader reader)
    {
        BoneIndex = reader.ReadUInt16();
        AnimationTrack = (AnimationTrack)reader.ReadByte();
        byte interpolationType = reader.ReadByte();
        FrameCount = reader.ReadUInt16();
        Unknown = reader.ReadUInt16();

        Interpolation = _bayo1InterpolationFactory[interpolationType]();

        if (Interpolation is InterpolationConstant interpolationConstant)
        {
            interpolationConstant.Value = reader.ReadSingle();
        }
        else if (Interpolation is not InterpolationNone)
        {
            reader.BaseStream.Seek(reader.ReadUInt32(), SeekOrigin.Begin);

            Interpolation.ReadBayo1(reader, FrameCount);
        }
    }

    public void ReadBayo2(BinaryReader reader, long recordOffset)
    {
        reader.BaseStream.Seek(recordOffset, SeekOrigin.Begin);

        BoneIndex = reader.ReadUInt16();
        AnimationTrack = (AnimationTrack)reader.ReadByte();
        byte interpolationType = reader.ReadByte();
        FrameCount = reader.ReadUInt16();
        Unknown = reader.ReadUInt16();
        Interpolation = _bayo2InterpolationFactory[interpolationType]();

        if (Interpolation is InterpolationConstant interpolationConstant)
        {
            interpolationConstant.Value = reader.ReadSingle();
        }
        else if (Interpolation is not InterpolationNone)
        {
            reader.BaseStream.Seek(recordOffset + reader.ReadUInt32(), SeekOrigin.Begin);

            Interpolation.ReadBayo2(reader, FrameCount);
        }
    }

    public void WriteBayo1(BinaryWriter writer, long valueOffset)
    {
        writer.Write(BoneIndex);
        writer.Write((byte)AnimationTrack);
        writer.Write((byte)_bayo1InterpolationTypes[Interpolation.GetType()]);
        writer.Write(FrameCount);
        writer.Write(Unknown);

        if (Interpolation is InterpolationConstant valueConstant)
        {
            writer.Write(valueConstant.Value);
        }
        else if (Interpolation is not InterpolationNone)
        {
            writer.Write((uint)valueOffset);
        }
    }
}
