using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PolyPanic.Bus;
using PolyPanic.Render.Shader;

namespace PolyPanic.Render
{
    public class Renderer
    {
        // The second real OpenGL challenge: Drawing a square.
        int VertexBufferObject; // VBO
        int VertexArrayObject; // VAO
        bool initialized = false; // Used to check if the renderer is initialized.
        int ElementBufferObject; // EBO
        private Matrix4 _view;
        private Matrix4 _projection;
        private int _windowWidth = 800; // Default width, can be changed later.
        private int _windowHeight = 600; // Default height, can be changed later.

        private readonly Stopwatch stopwatch = new Stopwatch();

        private ShaderProgram _shader;
        private TextureHelper.Texture _texture;
        private Camera.Camera _camera;
        private float _cameraSpeed = 200f;
        private float _lastDeltaTime = 0f;
        private bool _firstMouse = true; // Used to check if the mouse is moved for the first time.
        private Vector2 _lastMousePosition;

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

        readonly uint[] indices = {
            0, 1, 2, // first triangle
            0, 2, 3  // second triangle
        };

        // Handles resizes
        [Subscribe]
        public void OnResize(ResizeWindowEvent e)
        {
            _windowWidth = e.Width;
            _windowHeight = e.Height;
            GL.Viewport(0, 0, _windowWidth, _windowHeight);
            if (_camera != null)
            {
                _camera.AspectRatio = _windowWidth / _windowHeight;
            }
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
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
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

            // define and use texture
            _texture = TextureHelper.Texture.LoadFromFile(Path.Combine("src", "assets", "texture", "test.png"));
            _texture.Use(TextureUnit.Texture0);
            _shader.SetInt("texture0", 0);
            GL.EnableVertexAttribArray(0);
            // make it 3d
            // _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            // _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _windowWidth / (float)_windowHeight, 0.1f, 100.0f);
            _camera = new Camera.Camera(Vector3.UnitZ * 3, _windowWidth / _windowHeight);
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
            _lastDeltaTime = e.DeltaTime;
            // Render
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();
            _texture.Use(TextureUnit.Texture0);
            GL.BindVertexArray(VertexArrayObject);

            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(stopwatch.ElapsedMilliseconds / 10f));

            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            //args: primtype, amount of vertices to draw, type of ebo elements, offset. since we want to draw everything, 0.
            // GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
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
            // Movement keys
            // if (e.action != KeyboardEvent.KeyAction.Pressed) return;
            Console.WriteLine($"Key pressed: {e.key}, lastdeltatime {_lastDeltaTime}");
            switch (e.key)
            {
                case Keys.W:
                    _camera.Position += _camera.Front * _cameraSpeed * _lastDeltaTime;
                    break;
                case Keys.S:
                    _camera.Position -= _camera.Front * _cameraSpeed * _lastDeltaTime;
                    break;
                case Keys.A:
                    _camera.Position -= Vector3.Cross(_camera.Front, _camera.Up).Normalized() * _cameraSpeed * _lastDeltaTime;
                    break;
                case Keys.D:
                    _camera.Position += Vector3.Cross(_camera.Front, _camera.Up).Normalized() * _cameraSpeed * _lastDeltaTime;
                    break;
                case Keys.Space:
                    _camera.Position += _camera.Up * _cameraSpeed * _lastDeltaTime;
                    break;
                case Keys.LeftShift:
                    _camera.Position -= _camera.Up * _cameraSpeed * _lastDeltaTime;
                    break;
            }
        }

        [Subscribe]
        public void OnMouseMove(MouseMoveEvent e)
        {
            if (_firstMouse)
            {
                _lastMousePosition = new Vector2(e.X, e.Y);
                _firstMouse = false;
            }
            else
            {
                var deltaX = e.X - _lastMousePosition.X;
                var deltaY = e.Y - _lastMousePosition.Y;
                _lastMousePosition = new Vector2(e.X, e.Y);

                _camera.Yaw += deltaX * 0.25f;
                _camera.Pitch -= deltaY * 0.25f;
            }
        }
    }
}