using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PolyPanic.Bus;
using PolyPanic.Render.Shader;

namespace PolyPanic.Render
{
    public class Renderer
    {
        // The first real OpenGL challenge: Drawing a square.
        int VertexBufferObject; // VBO
        int VertexArrayObject; // VAO
        bool initialized = false; // Used to check if the renderer is initialized.
        int ElementBufferObject; // EBO

        ShaderProgram shader;

        readonly float[] triangleVertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            0.5f, -0.5f, 0.0f, //Bottom-right vertex
            0.0f,  0.5f, 0.0f  //Top vertex
        };

        readonly float[] squareVertices = {
            0.5f,  0.5f, 0.0f,  // top right
            0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f   // top left
        };

        readonly uint[] squareIndices = {
            0, 1, 2, // first triangle
            0, 2, 3  // second triangle
        };


        [Subscribe]
        public void OnLoad(LoadGameEvent e)
        {
            // Generate and bind VBO
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            // Pass buffer data
            GL.BufferData(BufferTarget.ArrayBuffer, squareVertices.Length * sizeof(float), squareVertices, BufferUsageHint.StaticDraw);
            // Generate and bind VAO
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            // Generate and bind EBO
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, squareIndices.Length * sizeof(uint), squareIndices, BufferUsageHint.StaticDraw);
            // Use shader program to compile and link shaders
            shader = new ShaderProgram(Path.Combine("src", "assets", "glsl", "basic.vert"), Path.Combine("src", "assets", "glsl", "basic.frag"));
            // Pass vertex data
            int posLoc = shader.GetAttribLocation("aPos");
            int colorLoc = shader.GetAttribLocation("aColor");
            int texLoc = shader.GetAttribLocation("aTexCoord");
            int stride = 3 * sizeof(float);
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(colorLoc, 4, VertexAttribPointerType.Float, false, stride, 12);
            GL.VertexAttribPointer(texLoc, 2, VertexAttribPointerType.Float, false, stride, 28);
            GL.EnableVertexAttribArray(0);
            // Initialize the renderer.
            initialized = true;
        }

        [Subscribe]
        public void OnUpdateFrame(UpdateFrameEvent e)
        {
            // This is the render loop.
            if (!initialized) return;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            shader.Use();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, squareIndices.Length, DrawElementsType.UnsignedInt, 0);
        }

        [Subscribe]
        public void OnKey(KeyboardEvent e)
        {
            if (e.key == Keys.Escape)
            {
                Console.WriteLine("Escape key pressed, exiting game.");
                Environment.Exit(0);
            }
        }
    }
}