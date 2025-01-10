using FellowOakDicom;
using FellowOakDicom.Imaging;

namespace Models;

public record class DicomSlice
{
    public required BitDepth BitDepth { get; init; }

    public required AnatomicPlane DefaultPlane { get; init; }

    public required uint EchoTime { get; init; }

    public required ushort Height { get; init; }

    public required ushort NumberOfTemporalPositions { get; init; }
    public required PhotometricInterpretation PhotometricInterpretation { get; init; }
    public required IReadOnlyList<byte> PixelData { get; init; }
    public required PixelRepresentation PixelRepresentation { get; init; }
    public required (double VerticalSpacing, double HorizontalSpacing) PixelSpacing { get; init; }
    public required ushort TemporalPosition { get; init; }

    public required uint TriggerTime { get; init; }
    public required ushort Width { get; init; }
    public required (float Window, float Level) WindowLevel { get; init; }

    public static DicomSlice FromDicomFile(DicomFile dicom)
    {
        var pixData = DicomPixelData.Create(dicom.Dataset);
        uint echoTime = dicom.Dataset.GetSingleValue<uint>(DicomTag.EchoTime);
        ushort tempPosition = dicom.Dataset.GetSingleValue<ushort>(DicomTag.TemporalPositionIdentifier);
        ushort numberoftemppositions = dicom.Dataset.GetSingleValue<ushort>(DicomTag.NumberOfTemporalPositions);
        double [] spacing = dicom.Dataset.GetValues<double>(DicomTag.PixelSpacing);
        (double vertspacing, double horzspacing) = (spacing [0], spacing [1]);
        uint triggerTime = dicom.Dataset.GetSingleValue<uint>(DicomTag.TriggerTime);
        float wl = dicom.Dataset.GetSingleValue<float>(DicomTag.WindowCenter);
        float ww = dicom.Dataset.GetSingleValue<float>(DicomTag.WindowWidth);

        return new DicomSlice
        {
            BitDepth = pixData.BitDepth,
            PhotometricInterpretation = pixData.PhotometricInterpretation,
            PixelRepresentation = pixData.PixelRepresentation,
            DefaultPlane = AnatomicPlane.Axial,
            Width = pixData.Width,
            Height = pixData.Height,
            PixelData = pixData.GetFrame(0).Data,
            EchoTime = echoTime,
            TemporalPosition = tempPosition,
            NumberOfTemporalPositions = numberoftemppositions,
            PixelSpacing = (vertspacing, horzspacing),
            TriggerTime = triggerTime,
            WindowLevel = (ww, wl)
        };
    }
}