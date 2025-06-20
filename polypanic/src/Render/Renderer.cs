using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PolyPanic.Bus;
using PolyPanic.Render.Shader;

namespace PolyPanic.Render
{
    public class Renderer
    {
        // The first real OpenGL challenge: Drawing a triangle.
        int VertexBufferObject; // VBO
        int VertexArrayObject; // VAO
        bool initialized = false; // Used to check if the renderer is initialized.

        ShaderProgram shader;

        float[] vertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            0.5f, -0.5f, 0.0f, //Bottom-right vertex
            0.0f,  0.5f, 0.0f  //Top vertex
        };
        [Bus.Subscribe]
        public void OnLoad(Bus.LoadGameEvent e)
        {
            // Generate and bind VBO
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            // Pass buffer data
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            // Generate and bind VAO
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            // Pass vertex data
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // Use shader program to compile and link shaders
            shader = new ShaderProgram(Path.Combine("src", "assets", "glsl", "basic.vert"), Path.Combine("src", "assets", "glsl", "basic.frag"));
            initialized = true;
        }

        [Bus.Subscribe]
        public void OnUpdateFrame(Bus.UpdateFrameEvent e)
        {
            // This is the render loop.
            if (!initialized) return;
            GL.Clear(ClearBufferMask.ColorBufferBit);
            shader.Use();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        [Bus.Subscribe]
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