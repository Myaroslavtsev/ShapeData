using ShapeData.Geometry;
using System.Numerics;

namespace ShapeData
{
    public class EditorVertex
    {
        // 3D space position
        public Vector3 Position { get; set; }

        // Texture coordinates
        public Vector2 UvPosition { get; set; }

        // Kuju shape array indexes
        public int KujuPointId;
        public int KujuUvPointId;

        public EditorVertex(float x, float y, float z, float u, float v)
        {
            Position = new Vector3(x, y, z);

            UvPosition = new Vector2(u, v);
        }
    }
}
