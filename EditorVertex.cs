using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    public class EditorVertex
    {
        // 3D space position
        public float X;
        public float Y;
        public float Z;

        // Texture coordinates
        public float U; // horizontal
        public float V; // vertical

        public EditorVertex(float x, float y, float z, float u, float v)
        {
            X = x;
            Y = y;
            Z = z;
            U = u;
            V = v;
        }
    }
}
