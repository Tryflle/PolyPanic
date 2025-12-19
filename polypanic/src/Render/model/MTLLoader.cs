using OpenTK.Mathematics;
using System.Globalization;

namespace PolyPanic.Render.Mesh
{
    // claude. I'M SORRY I DONT WANT TO FIGURE OUT EVERY LITTLE BIT OF HOW MTL FILES WORK...
    public class Material
    {
        public string Name { get; set; } = "";

        // Ambient color (Ka)
        public Vector3 Ambient { get; set; } = Vector3.Zero;

        // Diffuse color (Kd) - main object color
        public Vector3 Diffuse { get; set; } = new Vector3(0.8f, 0.8f, 0.8f);

        // Specular color (Ks)
        public Vector3 Specular { get; set; } = Vector3.Zero;

        // Specular exponent (Ns) - shininess
        public float Shininess { get; set; } = 32.0f;

        // Transparency (d or Tr)
        public float Alpha { get; set; } = 1.0f;

        // Optical density (Ni)
        public float OpticalDensity { get; set; } = 1.0f;

        // Illumination model (illum)
        public int IlluminationModel { get; set; } = 2;

        // Texture maps
        public string DiffuseTexture { get; set; } = "";
        public string SpecularTexture { get; set; } = "";
        public string NormalTexture { get; set; } = "";
        public string BumpTexture { get; set; } = "";
        public string AlphaTexture { get; set; } = "";

        // Emissive color (Ke)
        public Vector3 Emissive { get; set; } = Vector3.Zero;
    }

    public static class MTLLoader
    {
        public static Dictionary<string, Material> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"MTL file not found: {filePath}");

            var materials = new Dictionary<string, Material>();
            Material currentMaterial = null;
            
            string[] lines = File.ReadAllLines(filePath);
            string basePath = Path.GetDirectoryName(filePath) ?? "";

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                switch (parts[0].ToLower())
                {
                    case "newmtl": // New material
                        if (parts.Length > 1)
                        {
                            currentMaterial = new Material { Name = parts[1] };
                            materials[parts[1]] = currentMaterial;
                        }
                        break;

                    case "ka": // Ambient color
                        if (currentMaterial != null && parts.Length >= 4)
                        {
                            currentMaterial.Ambient = ParseVector3(parts, 1);
                        }
                        break;

                    case "kd": // Diffuse color
                        if (currentMaterial != null && parts.Length >= 4)
                        {
                            currentMaterial.Diffuse = ParseVector3(parts, 1);
                        }
                        break;

                    case "ks": // Specular color
                        if (currentMaterial != null && parts.Length >= 4)
                        {
                            currentMaterial.Specular = ParseVector3(parts, 1);
                        }
                        break;

                    case "ke": // Emissive color
                        if (currentMaterial != null && parts.Length >= 4)
                        {
                            currentMaterial.Emissive = ParseVector3(parts, 1);
                        }
                        break;

                    case "ns": // Specular exponent (shininess)
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            currentMaterial.Shininess = ParseFloat(parts[1]);
                        }
                        break;

                    case "d": // Transparency (alpha)
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            currentMaterial.Alpha = ParseFloat(parts[1]);
                        }
                        break;

                    case "tr": // Transparency (inverted alpha)
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            currentMaterial.Alpha = 1.0f - ParseFloat(parts[1]);
                        }
                        break;

                    case "ni": // Optical density
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            currentMaterial.OpticalDensity = ParseFloat(parts[1]);
                        }
                        break;

                    case "illum": // Illumination model
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            if (int.TryParse(parts[1], out int illum))
                            {
                                currentMaterial.IlluminationModel = illum;
                            }
                        }
                        break;

                    case "map_kd": // Diffuse texture
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            string texturePath = string.Join(" ", parts.Skip(1));
                            currentMaterial.DiffuseTexture = ResolvePath(basePath, texturePath);
                        }
                        break;

                    case "map_ks": // Specular texture
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            string texturePath = string.Join(" ", parts.Skip(1));
                            currentMaterial.SpecularTexture = ResolvePath(basePath, texturePath);
                        }
                        break;

                    case "map_bump":
                    case "bump": // Bump/normal map
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            string texturePath = string.Join(" ", parts.Skip(1));
                            currentMaterial.BumpTexture = ResolvePath(basePath, texturePath);
                        }
                        break;

                    case "map_kn":
                    case "norm": // Normal map
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            string texturePath = string.Join(" ", parts.Skip(1));
                            currentMaterial.NormalTexture = ResolvePath(basePath, texturePath);
                        }
                        break;

                    case "map_d": // Alpha texture
                        if (currentMaterial != null && parts.Length > 1)
                        {
                            string texturePath = string.Join(" ", parts.Skip(1));
                            currentMaterial.AlphaTexture = ResolvePath(basePath, texturePath);
                        }
                        break;
                }
            }

            return materials;
        }

        public static Dictionary<string, Material> LoadFromObjDirectory(string objFilePath)
        {
            string directory = Path.GetDirectoryName(objFilePath) ?? "";
            string baseName = Path.GetFileNameWithoutExtension(objFilePath);
            string mtlPath = Path.Combine(directory, baseName + ".mtl");
            
            if (File.Exists(mtlPath))
            {
                return LoadFromFile(mtlPath);
            }
            
            return new Dictionary<string, Material>();
        }

        private static Vector3 ParseVector3(string[] parts, int startIndex)
        {
            if (parts.Length < startIndex + 3)
                return Vector3.Zero;

            float x = ParseFloat(parts[startIndex]);
            float y = ParseFloat(parts[startIndex + 1]);
            float z = ParseFloat(parts[startIndex + 2]);

            return new Vector3(x, y, z);
        }

        private static float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private static string ResolvePath(string basePath, string texturePath)
        {
            // Clean up the texture path (remove quotes if present)
            texturePath = texturePath.Trim('"', '\'');
            
            // If it's already an absolute path, return it
            if (Path.IsPathRooted(texturePath))
                return texturePath;
            
            // Otherwise, combine with base path
            return Path.Combine(basePath, texturePath);
        }
    }
}