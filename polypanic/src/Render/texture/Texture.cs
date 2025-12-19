using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace PolyPanic.Render.TextureHelper
{
    public class Texture : IDisposable
    {
        public readonly int Handle;
        private bool _disposed = false;

        public Texture(int handle)
        {
            Handle = handle;
        }

        public static Texture LoadFromFile(string path)
        {
            // generate and bind texture handle
            int _handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _handle);

            // load image data but flip it vertically because opengl moment
            StbImage.stbi_set_flip_vertically_on_load(1);

            using (Stream stream = File.OpenRead(path))
            {
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            // loaded, but need to set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // generate mipmaps for the texture
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            // return the texture object
            return new Texture(_handle);
        }
        public void Use(TextureUnit unit)
        {
            // bind the texture to the specified texture unit
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if any
                }

                // Dispose unmanaged resources
                GL.DeleteTexture(Handle);
                _disposed = true;
            }
        }

        ~Texture()
        {
            Dispose(false);
        }
    }
}