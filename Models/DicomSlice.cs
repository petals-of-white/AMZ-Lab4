using FellowOakDicom;
using FellowOakDicom.Imaging;

namespace Models;

public class DicomSlice
{
    public BitDepth BitDepth => throw new NotImplementedException();

    public AnatomicPlane DefaultPlane => throw new NotImplementedException();

    public float EchoTime => throw new NotImplementedException();

    public ushort Height => throw new NotImplementedException();

    public ushort NumberOfTemporalPositions => throw new NotImplementedException();
    public PhotometricInterpretation PhotometricInterpretation => throw new NotImplementedException();
    public IReadOnlyList<byte> PixelData => throw new NotImplementedException();
    public PixelRepresentation PixelRepresentation => throw new NotImplementedException();
    public (double VerticalSpacing, double HorizontalSpacing) PixelSpacing => throw new NotImplementedException();
    //public float SliceLocation { get; }
    public ushort TemporalPosition { get; }
    public uint TriggerTime { get; }
    public ushort Width { get; }
    public (float Window, float Level) WindowLevel => throw new NotImplementedException();

    public static DicomSlice FromDicomFile(DicomFile dicom)
    {
        //var ds = dicom.Dataset;
        //var pixData = DicomPixelData.Create(ds);
        throw new NotImplementedException();
        //pixelData = pixData;
        //dataset = ds;
        //bytes = new List<byte>(pixData.GetFrame(0).Data);
        //Depth = 1;
    }
    

    public static IReadOnlyList<DicomSlice> FromFiles(string [] filepaths)
    { throw new NotImplementedException(); 
        // sort
    }

    public static IReadOnlyList<DicomSlice> SortSlices(IEnumerable<DicomSlice> slices)
    {
        throw new NotImplementedException();
    }

}