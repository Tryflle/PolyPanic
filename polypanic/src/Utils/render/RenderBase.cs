using OpenTK.Graphics.OpenGL;
using PolyPanic.Render.Shader;

namespace PolyPanic.Utils.Render
{
    // This is an extension class that I will use at some point to reduce boilerplate.
    public abstract class RenderBase : IDisposable
    {

        protected int vao, vbo, ebo;
        protected ShaderProgram shaderProgram;
        protected bool initialized = false;

        protected abstract string VShaderPath { get; }
        protected abstract string FShaderPath { get; }

        public virtual void Init()
        {
            // generate and bind vao, vbo and ebo
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            // create shader program
            shaderProgram = new ShaderProgram(VShaderPath, FShaderPath);

            initialized = true;
        }

        public virtual void LoadVertexData(float[] vertices, uint[] indices = null)
        {
            if (!initialized) Init();
            // todo: load vertex data accordingly
        }

        public virtual void BeginRender()
        {
            if (!initialized) Init();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // use shader program and bind vao, as required every frame.
            shaderProgram.Use();
            GL.BindVertexArray(vao);
        }

        public virtual void EndRender()
        {
            // todo: draw elements or arrays
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                shaderProgram?.Dispose();
            }

            // Dispose unmanaged OpenGL resources
            if (vao != 0)
            {
                GL.DeleteVertexArray(vao);
                vao = 0;
            }
            
            if (vbo != 0)
            {
                GL.DeleteBuffer(vbo);
                vbo = 0;
            }
            
            if (ebo != 0)
            {
                GL.DeleteBuffer(ebo);
                ebo = 0;
            }
        }
    }
}