using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using StbImageSharp;

namespace MukiaEngine.Graphics;

public static class Texture
{
    /// <summary>
    /// The base texture directory.
    /// </summary>
    public const string TextureDirectory = @"resources/textures";
    /// <summary>
    /// Shown when a texture couldn't found.
    /// </summary>
    public const string ErrorTextureName = "error_texture.png";
    public const string NullTextureName = "null.png";

    private static readonly Dictionary<string, int> Textures = [];

    /// <summary>
    /// Loads the file to be used for rendering.
    /// </summary>
    /// <param name="path">The path of the texture.</param>
    /// <returns>The texture ID to GL</returns>
    public static int LoadFromFile(string path)
    {
        int handle = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, handle);

        StbImage.stbi_set_flip_vertically_on_load(1);

        using (Stream stream = File.OpenRead(path))
        {
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        return handle;
    }

    private static readonly string[] AcceptedExtensions = [".png", ".jpeg", ".jpg", ".bmp"];

    /// <summary>
    /// Load all textures for rendering.
    /// </summary>
    public static void LoadTextures()
    {
        string[] textureNames = Directory.GetFiles(TextureDirectory);

        foreach (string file in textureNames)
        {
            string ext = Path.GetExtension(file),
            fileName = Path.GetFileName(file);
            if (!AcceptedExtensions.Contains(ext))
            {
                Console.WriteLine($"{ext} is not a valid file type (file)");
                continue;
            }
            int handle = LoadFromFile(file);
            Textures.Add(fileName, handle);
        }
    }

    /// <summary>
    /// Gets the texture from <paramref name="fileName"/>.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="fileName"/> is empty then returns a transparent texture,<br/>
    /// or if not found, then use a purple and black check pattern.
    /// </remarks>
    /// <param name="fileName">The file name</param>
    /// <returns>Texture ID</returns>
    public static int GetTexture(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return Textures[NullTextureName];
        }

        if (!Textures.TryGetValue(fileName, out int value))
        {
            return Textures[ErrorTextureName];
        }
        return value;
    }
}

