using FellowOakDicom.Imaging;

namespace Models;

public interface IDicomSlice
{
    BitDepth BitDepth { get; }
    AnatomicPlane DefaultPlane { get; }
    float EchoTime { get; }
    ushort Height { get; }
    ushort Width { get; }
    ushort NumberOfTemporalPositions { get; }
    PhotometricInterpretation PhotometricInterpretation { get; }
    IReadOnlyList<byte> PixelData { get; }
    PixelRepresentation PixelRepresentation { get; }
    (double VerticalSpacing, double HorizontalSpacing) PixelSpacing { get; }
    float SliceLocation { get; }
    ushort TemporalPosition { get; }
    uint TriggerTime { get; }
    (float Window, float Level) WindowLevel { get; }
}