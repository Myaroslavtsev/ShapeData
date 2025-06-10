/// Data structure. Editor vertex is used to store all data about one corner of EditorPolygon, 
/// consisting of 3d coords and texture coords.

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
        
        public Vector3 Normal { get; set; }
        
        // Kuju shape array indexes
        public int KujuPointId { get; set; }
        public int KujuUvPointId { get; set; }
        public int KujuNormalId { get; set; }
        public int KujuVertexId { get; set; }

        public EditorVertex(float x, float y, float z, float u, float v)
        {
            Position = new Vector3(x, y, z);

            UvPosition = new Vector2(u, v);
        }
    }
}
