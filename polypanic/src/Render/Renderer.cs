using System.Diagnostics;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PolyPanic.Bus;
using PolyPanic.Main;
using PolyPanic.Render.Shader;

namespace PolyPanic.Render
{
    public class Renderer
    {
        // The second real OpenGL challenge: Drawing a square.
        int VertexBufferObject; // VBO
        int VertexArrayObject; // VAO
        bool initialized = false; // Used to check if the renderer is initialized.
        // int ElementBufferObject; // EBO
        private Matrix4 _view;
        private Matrix4 _projection;

        private int _windowWidth = 800; // Default width, can be changed later.
        private int _windowHeight = 600; // Default height, can be changed later.

        private readonly Stopwatch stopwatch = new Stopwatch();

        private ShaderProgram _shader;

        // readonly float[] triangleVertices = {
        //     -0.5f, -0.5f, 0.0f, //Bottom-left vertex
        //     0.5f, -0.5f, 0.0f, //Bottom-right vertex
        //     0.0f,  0.5f, 0.0f  //Top vertex
        // };
        // readonly float[] squareVertices = {
        //     // Positions        // Colors
        //     0.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  // top right - red
        //     0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  // bottom right - green  
        //     -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  // bottom left - blue
        //     -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f   // top left - yellow
        // };

        float[] cubeVertices = {
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };

        // readonly uint[] squareIndices = {
        //     0, 1, 2, // first triangle
        //     0, 2, 3  // second triangle
        // };

        // Handles resizes
        [Subscribe]
        public void OnResize(ResizeWindowEvent e)
        {
            _windowWidth = e.Width;
            _windowHeight = e.Height;
        }

        [Subscribe]
        public void OnLoad(LoadGameEvent e)
        {
            // Generate and bind VBO
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            // Pass buffer data
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);
            // Generate and bind VAO
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            // Generate and bind EBO
            // ElementBufferObject = GL.GenBuffer();
            // GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            // GL.BufferData(BufferTarget.ElementArrayBuffer, squareIndices.Length * sizeof(uint), squareIndices, BufferUsageHint.StaticDraw);
            // Use shader program to compile and link shaders
            _shader = new ShaderProgram(Path.Combine("src", "assets", "glsl", "basic3d.vert"), Path.Combine("src", "assets", "glsl", "basic3d.frag"));
            // Pass vertex data
            int posLoc = _shader.GetAttribLocation("aPos");
            int texLoc = _shader.GetAttribLocation("aTexCoord");
            int stride = 5 * sizeof(float); // 3 for position + 2 for texture

            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(texLoc, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.EnableVertexAttribArray(posLoc);
            GL.EnableVertexAttribArray(texLoc);
            
            GL.EnableVertexAttribArray(0);
            // make it 3d
            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _windowWidth / (float)_windowHeight, 0.1f, 100.0f);

            GL.Enable(EnableCap.DepthTest);
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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();
            GL.BindVertexArray(VertexArrayObject);

            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(stopwatch.ElapsedMilliseconds / 10f));

            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);
            //args: primtype, amount of vertices to draw, type of ebo elements, offset. since we want to draw everything, 0.
            // GL.DrawElements(PrimitiveType.Triangles, squareIndices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // Draw the cube with 36 vertices
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