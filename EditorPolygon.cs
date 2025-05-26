using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    public enum Material
    {
        SolidNorm,
        TransNorm,
        SolidBright,
        TransBright
    }

    public class EditorPolygon
    {
        // GeneralProperties
        public uint PolygonId;

        public Material MaterialType;

        public string TextureFilename;

        private readonly List<EditorVertex> vertices;

        public List<EditorVertex> Vertices;

        public EditorPolygon(
            uint polygonId,
            List<EditorVertex> vertexList,
            Material material = Material.SolidNorm, 
            string textureName = "blank.ace")
        {
            if (vertexList == null || vertexList.Count < 3)
            {
                throw new Exception("Vertex count should be at least 3");
            }

            PolygonId = polygonId;
            MaterialType = material;
            TextureFilename = textureName;
            vertices = vertexList;
        }

        // to think about: how to block from deleting last 3 points but allow increasing point number
    }
}
