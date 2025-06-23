using OpenTK.Graphics.OpenGL;
using PolyPanic.Render.Shader;

namespace PolyPanic.Utils.Render
{
    // This is an extension class that I will use at some point to reduce boilerplate.
    public abstract class RenderBase
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
        }

        public virtual void BeginRender()
        {
            if (!initialized)
            {
                Init();
                initialized = true;
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shaderProgram.Use();
            GL.BindVertexArray(vao);
        }
    }
}