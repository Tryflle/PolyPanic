using System.Runtime.InteropServices;
using FreeTypeSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PolyPanic.Bus;
using PolyPanic.Render.Shader;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace PolyPanic.Render.Font
{
    // claude sonnet 4 rewrote my dysfunctional font renderer with this one that actually works.
    // I unfortunately do not know unsafe memory management or C stuff, so I messed up a lot.
    // thankfully ai exists so this won't be a headache.
    public unsafe class FontRenderer
    {
        private static FontRenderer _instance;
        public static FontRenderer Instance => _instance;

        private int _vbo;
        private int _vao;
        private ShaderProgram _textShader;
        private bool _initialized = false;

        private FT_LibraryRec_* lib;
        private FT_FaceRec_* face;

        private Dictionary<char, CharacterInfo> _characters = new Dictionary<char, CharacterInfo>();
        private Queue<TextRenderCommand> _renderQueue = new Queue<TextRenderCommand>();

        // For 2D text rendering - we need orthographic projection
        private Matrix4 _projection;
        private int _windowWidth = 800;
        private int _windowHeight = 600;

        private struct CharacterInfo
        {
            public int TextureId;
            public int Width;
            public int Height;
            public int BearingX;
            public int BearingY;
            public int Advance;
        }

        private struct TextRenderCommand
        {
            public string Text;
            public float X;
            public float Y;
            public float Scale;
            public Vector3 Color;
        }

        [Subscribe]
        public void OnResize(ResizeWindowEvent e)
        {
            _windowWidth = e.Width;
            _windowHeight = e.Height;
            // Update projection matrix for text rendering
            _projection = Matrix4.CreateOrthographicOffCenter(0, _windowWidth, 0, _windowHeight, -1, 1);
        }

        [Subscribe]
        public void OnLoad(LoadGameEvent e)
        {
            _instance = this;

            // Initialize FreeType (your existing code)
            FT_LibraryRec_* localLib;
            var error = FT_Init_FreeType(&localLib);
            if (error != 0) throw new Exception($"FreeType init failed: {error}");
            lib = localLib;

            string fontPath = Path.Combine("src", "assets", "font", "HomeVideo.ttf");
            var fontPathAnsi = Marshal.StringToHGlobalAnsi(fontPath);
            try
            {
                FT_FaceRec_* localFace;
                error = FT_New_Face(lib, (byte*)fontPathAnsi, 0, &localFace);
                if (error != 0) throw new Exception($"Font load failed: {error}");
                face = localFace;
            }
            finally
            {
                Marshal.FreeHGlobal(fontPathAnsi);
            }

            error = FT_Set_Pixel_Sizes(face, 0, 48);
            if (error != 0) throw new Exception($"Set pixel size failed: {error}");

            // Setup text shader
            _textShader = new ShaderProgram(
                Path.Combine("src", "assets", "glsl", "text.vert"),
                Path.Combine("src", "assets", "glsl", "text.frag")
            );

            // Setup OpenGL buffers
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Get attribute locations from shader
            int posLoc = _textShader.GetAttribLocation("vertex");
            GL.EnableVertexAttribArray(posLoc);
            GL.VertexAttribPointer(posLoc, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            // Setup projection matrix
            _projection = Matrix4.CreateOrthographicOffCenter(0, _windowWidth, 0, _windowHeight, -1, 1);

            // Pre-load common ASCII characters
            for (char c = ' '; c <= '~'; c++)
            {
                LoadCharacter(c);
            }

            _initialized = true;
        }

        [Subscribe]
        public void OnUpdateFrame(UpdateFrameEvent e)
        {
            if (!_initialized) return;

            // Process all queued text rendering commands
            while (_renderQueue.Count > 0)
            {
                var command = _renderQueue.Dequeue();
                RenderTextInternal(command.Text, command.X, command.Y, command.Scale, command.Color);
            }
        }

        // Static methods for external use
        public static void RenderText(string text, float x, float y, float scale = 1.0f)
        {
            RenderText(text, x, y, scale, Vector3.One);
        }

        public static void RenderText(string text, float x, float y, float scale, Vector3 color)
        {
            if (_instance == null || !_instance._initialized) return;

            _instance._renderQueue.Enqueue(new TextRenderCommand
            {
                Text = text,
                X = x,
                Y = y,
                Scale = scale,
                Color = color
            });
        }

        private void LoadCharacter(char c)
        {
            if (_characters.ContainsKey(c)) return;

            var glyphIndex = FT_Get_Char_Index(face, c);
            var error = FT_Load_Glyph(face, glyphIndex, FT_LOAD_RENDER);
            if (error != 0) throw new Exception($"Load glyph failed: {error}");

            var glyph = face->glyph;
            var bitmap = glyph->bitmap;

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRed,
                (int)bitmap.width, (int)bitmap.rows, 0, PixelFormat.Red, PixelType.UnsignedByte,
                new IntPtr(bitmap.buffer));

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            _characters[c] = new CharacterInfo
            {
                TextureId = texture,
                Width = (int)bitmap.width,
                Height = (int)bitmap.rows,
                BearingX = glyph->bitmap_left,
                BearingY = glyph->bitmap_top,
                Advance = (int)(glyph->advance.x >> 6)
            };
        }

        private void RenderTextInternal(string text, float x, float y, float scale, Vector3 color)
        {
            // Enable blending for text transparency
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _textShader.Use();
            _textShader.SetMatrix4("projection", _projection);
            _textShader.SetVec3("textColor", color);
            _textShader.SetInt("text", 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(_vao);

            float currentX = x;

            foreach (char c in text)
            {
                if (!_characters.ContainsKey(c))
                    LoadCharacter(c);

                var ch = _characters[c];

                float xpos = currentX + ch.BearingX * scale;
                float ypos = y - (ch.Height - ch.BearingY) * scale;
                float w = ch.Width * scale;
                float h = ch.Height * scale;

                // Create quad vertices (x, y, u, v)
                float[] vertices = {
                    xpos,     ypos + h,   0.0f, 0.0f,  // Bottom-left
                    xpos,     ypos,       0.0f, 1.0f,  // Top-left
                    xpos + w, ypos,       1.0f, 1.0f,  // Top-right

                    xpos,     ypos + h,   0.0f, 0.0f,  // Bottom-left
                    xpos + w, ypos,       1.0f, 1.0f,  // Top-right
                    xpos + w, ypos + h,   1.0f, 0.0f   // Bottom-right
                };

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                currentX += ch.Advance * scale;
            }

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);
        }

        public void Dispose()
        {
            if (face != null) FT_Done_Face(face);
            if (lib != null) FT_Done_FreeType(lib);

            foreach (var character in _characters.Values)
                GL.DeleteTexture(character.TextureId);

            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
        }
    }
}