using ShapeData.Geometry;
using System.Numerics;

namespace ShapeData
{
    public class EditorVertex
    {
        // 3D space position
        public Vector3 Position { get; set; }

        // Texture coordinates
        public float U; // horizontal
        public float V; // vertical

        public EditorVertex(float x, float y, float z, float u, float v)
        {
            Position = new Vector3(x, y, z);

            U = u;
            V = v;
        }
    }
}
