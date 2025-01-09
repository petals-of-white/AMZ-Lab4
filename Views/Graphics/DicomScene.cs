using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Models;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using ViewModels;
using static Views.Graphics.OpenGLHelpers;

namespace Views.Graphics;

public class DicomScene : IDisposable
{
    private static readonly float [] coords = {
        -1f, -1f,     0f, 0f,
        -1f, 1f,      0f, 1f,
        1f,  -1f,     1f, 0f,
        1f,  1f,      1f, 1f,
    };

    private readonly TextureWrapMode wrapMode = TextureWrapMode.MirroredRepeat;
    private DicomToGLConverter? dicomGLData;
    private bool disposed;
    private DicomSeries? lastLoadedSeries;
    private uint texture3D;
    private uint [] textures;
    private uint vao;
    private uint vbo;
    private int vertShader, fragShader, program;
    //private CoordsPixelLength? volumePixSize;

    public DicomScene()
    {
        while (GL.GetError() is not ErrorCode.NoError) ;

        CreateVertices();
        CreateProgram();
        //CreateTexture();

        UnbindAll();
    }

    public static string FragShaderLoc { get; } = "Shaders/shader.frag";
    public static string VertShaderLoc { get; } = "Shaders/shader.vert";
    public bool IsTextureLoaded { get; private set; } = false;

    public static Matrix4 ToOpenTKMatrix(Matrix4x4 matrix) => new(
        matrix.M11, matrix.M12, matrix.M13, matrix.M14,
        matrix.M21, matrix.M22, matrix.M23, matrix.M24,
        matrix.M31, matrix.M32, matrix.M33, matrix.M34,
        matrix.M41, matrix.M42, matrix.M43, matrix.M44);

    public void Dispose()
    {
        if (!disposed)
        {
            UnbindAll();
            GL.DeleteTextures(1, [texture3D]);
            GL.DeleteTextures(textures.Length, textures);
            GL.DeleteVertexArrays(1, [vao]);
            GL.DeleteBuffers(1, [vbo]);
            GL.DeleteProgram(program);
            GL.DeleteShader(fragShader);
            GL.DeleteShader(vertShader);
            disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    public void DrawVertices(AnatomicPlane targetPlane, uint spaceLocation, uint timeLocation)
    {
        CoordsPixelLength coordsPixelLength = new(lastLoadedSeries!, targetPlane);
        if (IsTextureLoaded)
        {
            float relativeDepth = (float) spaceLocation / coordsPixelLength.ZPixels;

            //switch (targetPlane)
            //{
            //    case (AnatomicPlane.Axial):

            //        relativeDepth = (float) spaceLocation / coordsPixelLength.ZPixels;
            //        break;

            //    case (AnatomicPlane.Sagittal):
            //        relativeDepth = (float) spaceLocation / coordsPixelLength.XPixels;
            //        break;

            //    case (AnatomicPlane.Coronal):
            //        relativeDepth = (float) spaceLocation / coordsPixelLength.YPixels;
            //        break;

            //    default:
            //        throw new NotImplementedException("Dicom HUH");
            //}

            //Matrix4x4 addDepth = AnatomicPlaneRelations.AddDepth((AnatomicPlane) sourcePlane!, targetPlane, relativeDepth);

            // Bind All
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindTexture(TextureTarget.Texture3D, textures [timeLocation]);
            GL.UseProgram(program);
            GL.ActiveTexture(TextureUnit.Texture0);
            ThrowIfGLError();

            // Upload a transformation matrix
            var sourcePlane = lastLoadedSeries!.Plane;
            Matrix4x4 changePlanes = AnatomicPlaneRelations.PlaneTransform(sourcePlane, targetPlane);
            var transformMatrix = Matrix4x4.CreateTranslation(0, 0, relativeDepth) * changePlanes;
            var opentkmatrix = ToOpenTKMatrix(transformMatrix);
            var transMatLoc = GL.GetUniformLocation(program, "u_transform_matrix");
            GL.UniformMatrix4(transMatLoc, false, ref opentkmatrix);
            ThrowIfGLError();

            ApplyNormalization(0f, 600);
            //ApplyWindowLevel(900, 500);

            //Draw vertices

            int numberOfVertices = 4;
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, numberOfVertices);
            // unbind
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture3D, 0);
            GL.UseProgram(0);
            ThrowIfGLError();
        }
    }

    public void LoadDicomSeries(DicomSeries dicomSeries)
    {
        if (dicomSeries != lastLoadedSeries)
        {
            var converter = new DicomToGLConverter(dicomSeries);
            var tempPositions = dicomSeries.Slices [0].NumberOfTemporalPositions;

            //volumePixSize = new CoordsPixelLength(dicomSeries);
            textures = new uint [tempPositions];
            GL.GenTextures(tempPositions, textures);

            var groups = dicomSeries.Slices.GroupBy(slice => slice.TemporalPosition).ToArray();

            for (int i = 0; i < groups.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture3D, textures [i]);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int) wrapMode);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int) wrapMode);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int) wrapMode);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Nearest);

                var combinedData = groups [i].SelectMany(slice => slice.PixelData).ToArray();
                GL.TexImage3D(TextureTarget.Texture3D, 0, converter.InternalFormat,
                        converter.Width, converter.Height, (int) dicomSeries.NumberOfSpacePositions,
                        0, converter.Format, converter.Type, combinedData);

                GL.BindTexture(TextureTarget.Texture3D, 0);
            }
            lastLoadedSeries = dicomSeries;
            IsTextureLoaded = true;
        }
    }


    public void UnbindAll()
    {
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.UseProgram(0);
        GL.BindTexture(TextureTarget.Texture3D, 0);
    }

    private void ApplyNormalization(float minPeak, float maxPeak)
    {
        // upload normalization uniforms
        int minPeakLoc = GL.GetUniformLocation(program, "minPeak");
        int maxPeakLoc = GL.GetUniformLocation(program, "maxPeak");

        GL.Uniform1(minPeakLoc, minPeak);
        GL.Uniform1(maxPeakLoc, maxPeak);
    }

    private void ApplyNormalization(byte [] byteArray)
    {
        // upload normalization uniforms
        int minPeakLoc = GL.GetUniformLocation(program, "minPeak");
        int maxPeakLoc = GL.GetUniformLocation(program, "maxPeak");
        short [] texArray = new short [byteArray.Length / 2];
        System.Buffer.BlockCopy(byteArray, 0, texArray, 0, byteArray.Length);
        var maxVal = texArray.Max();
        var minVal = texArray.Min();
        GL.Uniform1(minPeakLoc, (float) minVal);
        GL.Uniform1(maxPeakLoc, (float) maxVal);
    }

    private void ApplyWindowLevel(float windowWidth, float windowLevel)
    {
        // Upload window-level uniforms
        int widthLoc = GL.GetUniformLocation(program, "windowWidth");
        int levelLoc = GL.GetUniformLocation(program, "windowLevel");

        GL.Uniform1(widthLoc, windowWidth);
        GL.Uniform1(levelLoc, windowLevel);
    }

    private void BindAll()
    {
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

        GL.UseProgram(program);

        GL.BindTexture(TextureTarget.Texture3D, texture3D);
    }

    private void CheckTexture()
    {
        short [] texture = new short [dicomGLData!.Width * dicomGLData.Height * dicomGLData.Depth];

        int internalFormat;
        int texWidth, texHeight, texDepth;

        GL.GetTexLevelParameter(TextureTarget.Texture3D, 0, GetTextureParameter.TextureInternalFormat, out internalFormat);
        GL.GetTexLevelParameter(TextureTarget.Texture3D, 0, GetTextureParameter.TextureWidth, out texWidth);
        GL.GetTexLevelParameter(TextureTarget.Texture3D, 0, GetTextureParameter.TextureHeight, out texHeight);
        GL.GetTexLevelParameter(TextureTarget.Texture3D, 0, GetTextureParameter.TextureDepth, out texDepth);

        if ((PixelInternalFormat) internalFormat != dicomGLData!.InternalFormat
            || texWidth != dicomGLData!.Width
            || texHeight != dicomGLData!.Height
            || texDepth != dicomGLData!.Depth)
        {
            var message = $"Mismatch! Expected: Width{dicomGLData.Width}, Height {dicomGLData.Height}, Depth{dicomGLData.Depth}," +
                $"Internal format {dicomGLData.InternalFormat}. Got: width {texWidth}, height {texHeight}, depth {texDepth}, internal format {(PixelInternalFormat) internalFormat}";
            Debug.WriteLine(message);
            //throw new Exception(message);
        }

        GL.GetTexImage(TextureTarget.Texture3D, 0, PixelFormat.RedInteger, PixelType.Short, texture);
    }

    private void CreateProgram()
    {
        vertShader = MakeShader(ShaderType.VertexShader, File.ReadAllText(VertShaderLoc));
        fragShader = MakeShader(ShaderType.FragmentShader, File.ReadAllText(FragShaderLoc));

        program = GL.CreateProgram();
        GL.AttachShader(program, vertShader);
        GL.AttachShader(program, fragShader);
        GL.LinkProgram(program);
        GL.ValidateProgram(program);

        // TODO: validate progam
    }

    private void CreateTexture()
    {
        uint [] textures = new uint [1];
        GL.GenTextures(1, textures);
        texture3D = textures [0];

        GL.UseProgram(program);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture3D, texture3D);

        ThrowIfGLError();

        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int) wrapMode);
        ThrowIfGLError();
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int) wrapMode);
        ThrowIfGLError();
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int) wrapMode);
        ThrowIfGLError();
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        ThrowIfGLError();
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Nearest);

        ThrowIfGLError();

        var texUniLoc = GL.GetUniformLocation(program, "u_texture");
        int sampler3d = 0;
        GL.Uniform1(texUniLoc, sampler3d);

        ThrowIfGLError();

        // unbind all
        GL.BindTexture(TextureTarget.Texture3D, 0);
        GL.UseProgram(0);
    }

    private void CreateVertices()
    {
        uint [] vaos = new uint [1];
        uint [] vbos = new uint [1];
        GL.GenVertexArrays(1, vaos);
        GL.BindVertexArray(vaos [0]);

        GL.GenBuffers(1, vbos);

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbos [0]);
        GL.BufferData(BufferTarget.ArrayBuffer, coords.Length * sizeof(float), coords, BufferUsageHint.StaticDraw);

        ThrowIfGLError();

        // buffer data...
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // unbindg vao
        GL.BindVertexArray(0);

        // unbind vbo

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        vbo = vbos [0];
        vao = vaos [0];

        ThrowIfGLError();
    }
}