using System.Diagnostics;
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

        private readonly Stopwatch stopwatch = new Stopwatch();

        ShaderProgram shader;

        readonly float[] triangleVertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            0.5f, -0.5f, 0.0f, //Bottom-right vertex
            0.0f,  0.5f, 0.0f  //Top vertex
        };
        readonly float[] squareVertices = {
            // Positions        // Colors
            0.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  // top right - red
            0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  // bottom right - green  
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  // bottom left - blue
            -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f   // top left - yellow
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
            // int texLoc = shader.GetAttribLocation("aTexCoord");
            // int stride = 3 * sizeof(float); // original stride
            int stride = 6 * sizeof(float);
            // GL.VertexAttribPointer(colorLoc, 4, VertexAttribPointerType.Float, false, stride, 12); // original colorloc
            // GL.VertexAttribPointer(texLoc, 2, VertexAttribPointerType.Float, false, stride, 28); // original texloc
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(posLoc);
            GL.EnableVertexAttribArray(colorLoc);
            GL.EnableVertexAttribArray(0);

            // Initialize the stopwatch, this measures time and can be used for animations.
            stopwatch.Start();
            // Initialize the renderer.
            initialized = true;
        }

        [Subscribe]
        public void OnUpdateFrame(UpdateFrameEvent e)
        {
            // This is the render loop.
            if (!initialized) return;
            // Render
            GL.Clear(ClearBufferMask.ColorBufferBit);
            shader.Use();
            GL.BindVertexArray(VertexArrayObject);
            //args: primtype, amount of vertices to draw, type of ebo elements, offset. since we want to draw everything, 0.
            GL.DrawElements(PrimitiveType.Triangles, squareIndices.Length, DrawElementsType.UnsignedInt, 0);
        }

        [Subscribe]
        public void OnKey(KeyboardEvent e)
        {
            // Exit the game cleanly!!!
            if (e.key == Keys.Escape)
            {
                Console.WriteLine("Escape key pressed, exiting game.");
                Environment.Exit(0);
            }
        }
    }
}