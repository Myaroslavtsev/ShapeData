using System;
using System.Collections.Generic;
using System.Numerics;

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
        public uint PolygonId { get; set; }

        public Material MaterialType { get; set; }

        public string TextureFilename { get; set; }

        // Kuju shape array indexes
        public int KujuImageId { get; set; }
        public int KujuShaderId { get; set; }
        public int KujuLightMatId { get; set; }
        public int KujuPrimStateId { get; set; }
        public int KujuNormalId { get; set; }

        public Vector3 Normal { get; set; }

        private readonly List<EditorVertex> vertices;

        public List<EditorVertex> Vertices => vertices;

        public EditorPolygon(
            uint polygonId,
            List<EditorVertex> vertexList,
            Material material = Material.SolidNorm,
            string textureName = "blank")
        {
            PolygonId = polygonId;
            MaterialType = material;
            TextureFilename = textureName;
            vertices = vertexList;
        }

        public EditorVertex AddVertex(EditorVertex vertex)
        {
            if (vertex == null)
                return null;

            vertices.Add(vertex);
            return vertex;
        }

        public EditorPolygon Copy()
        {
            var verticesCopy = new List<EditorVertex>();

            foreach (var v in vertices)
                verticesCopy.Add(
                    new EditorVertex(v.Position.X, v.Position.Y, v.Position.Z, v.UvPosition.X, v.UvPosition.Y));

            //throw new NotImplementedException("Vertex normals copying"); // this data is needed only after copying!

            return new EditorPolygon(PolygonId, verticesCopy, MaterialType, TextureFilename);
        }

        // to think about: how to block from deleting last 3 points but allow increasing point number
        // when deserializer requires adding line by line
        // so creation with less than 2 vertices should also be possible
    }
}
