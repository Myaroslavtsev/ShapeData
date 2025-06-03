using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

namespace ShapeData
{
    public class EditorVertex
    {
        // 3D space position
        public Point Position { get; }

        // Texture coordinates
        public float U; // horizontal
        public float V; // vertical

        public EditorVertex(float x, float y, float z, float u, float v)
        {
            Position = new Point(x, y, z);

            U = u;
            V = v;
        }
    }
}
