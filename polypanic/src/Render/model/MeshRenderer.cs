using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PolyPanic.Bus;
using PolyPanic.Render.Camera;
using PolyPanic.Utils.Render;

namespace PolyPanic.Render.Mesh
{
    public class Mesh
    {
        public Vector3[] vertices { get; set; }
        public Vector3[] normals { get; set; }
        public Vector2[] textureCoords { get; set; }
        public uint[] indices { get; set; }
        
        public Mesh(Vector3[] vertices, Vector3[] normals, Vector2[] textureCoords, uint[] indices)
        {
            this.vertices = vertices;
            this.normals = normals;
            this.textureCoords = textureCoords;
            this.indices = indices;
        }
    }

    // claude helped with some of the scaffold and weird math
    public class MeshRenderer : RenderBase
    {
        protected override string VShaderPath => Path.Combine("src", "assets", "glsl", "mesh.vert");
        protected override string FShaderPath => Path.Combine("src", "assets", "glsl", "mesh.frag");

        private Mesh _mesh;
        private bool _loaded = false;

        [Subscribe]
        public void OnLoad(LoadGameEvent e)
        {
            base.Init();
        }

        public override void LoadVertexData(float[] vertices, uint[]? indices = null)
        {
            if (!initialized) Init();
            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            if (indices != null)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            }

            // 3 pos, 3 normals, 2 texcoords = 8
            int stride = 8 * sizeof(float);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
            _loaded = true;
        }

        public void LoadMesh(Mesh mesh)
        {
            _mesh = mesh;
            if (_mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh), "Mesh was NULL?");
            }

            float[] vertices = new float[_mesh.vertices.Length * 8];

            for (int i = 0; i < _mesh.vertices.Length; i++)
            {
                int offset = i * 8;

                // Position
                vertices[offset + 0] = mesh.vertices[i].X;
                vertices[offset + 1] = mesh.vertices[i].Y;
                vertices[offset + 2] = mesh.vertices[i].Z;

                // Normal
                if (mesh.normals != null && i < mesh.normals.Length)
                {
                    vertices[offset + 3] = mesh.normals[i].X;
                    vertices[offset + 4] = mesh.normals[i].Y;
                    vertices[offset + 5] = mesh.normals[i].Z;
                }
                else
                {
                    vertices[offset + 3] = 0.0f;
                    vertices[offset + 4] = 1.0f;
                    vertices[offset + 5] = 0.0f;
                }

                // Texture coordinates
                if (mesh.textureCoords != null && i < mesh.textureCoords.Length)
                {
                    vertices[offset + 6] = mesh.textureCoords[i].X;
                    vertices[offset + 7] = mesh.textureCoords[i].Y;
                }
                else
                {
                    vertices[offset + 6] = 0.0f;
                    vertices[offset + 7] = 0.0f;
                }
            }

            LoadVertexData(vertices, mesh.indices);
        }

        public override void EndRender()
        {
            if (!_loaded || _mesh == null)
                return;

            // Bind our VAO for drawing
            GL.BindVertexArray(vao);

            if (_mesh.indices != null)
            {
                GL.DrawElements(PrimitiveType.Triangles, _mesh.indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, _mesh.vertices.Length);
            }

            // CRITICAL: Restore OpenGL state properly
            GL.BindVertexArray(0);           // Unbind VAO
            GL.UseProgram(0);                // Unbind shader program
            GL.BindTexture(TextureTarget.Texture2D, 0);  // Unbind texture
            
            // Make sure depth testing is still enabled
            GL.Enable(EnableCap.DepthTest);
        }

        public void SetTransforms(Matrix4 model, Camera.Camera camera)
        {
            if (!initialized) Init();

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            shaderProgram.SetMatrix4("view", view);
            shaderProgram.SetMatrix4("projection", projection);
            shaderProgram.SetMatrix4("model", model);

            Matrix4 normalMatrix = Matrix4.Transpose(Matrix4.Invert(model));
            shaderProgram.SetMatrix4("normalMatrix", normalMatrix);
        }
        
        public void SetLighting(Vector3 lightPos, Vector3 lightColor, Camera.Camera camera)
        {
            if (!initialized) return;

            shaderProgram.SetVec3("lightPos", lightPos);
            shaderProgram.SetVec3("lightColor", lightColor);
            shaderProgram.SetVec3("viewPos", camera.Position);
        }


        public void SetMaterial(Vector3 objectColor, bool hasTexture = false, int textureId = 0)
        {
            if (!initialized) return;

            shaderProgram.SetVec3("objectColor", objectColor);
            shaderProgram.SetInt("hasTexture", hasTexture ? 1 : 0);

            if (hasTexture && textureId > 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                shaderProgram.SetInt("texture_diffuse1", 0);
            }
        }

        public void RenderMesh(Mesh mesh, Matrix4 model, Camera.Camera camera, 
                              Vector3 lightPos, Vector3 lightColor, 
                              Vector3 objectColor, bool hasTexture = false, int textureId = 0)
        {
            if (_mesh != mesh)
            {
                LoadMesh(mesh);
            }

            BeginRender();
            SetTransforms(model, camera);
            SetLighting(lightPos, lightColor, camera);
            SetMaterial(objectColor, hasTexture, textureId);
            EndRender();
        }
    }
}