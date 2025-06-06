using System.Collections.Generic;

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

        // Kuju shape array indexes
        public int KujuNormalId;
        public int KujuImageId;
        public int KujuShaderId;

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

            return new EditorPolygon(PolygonId, verticesCopy, MaterialType, TextureFilename);
        }

        // to think about: how to block from deleting last 3 points but allow increasing point number
        // when deserializer requires adding line by line
        // so creation with less than 2 vertices should also be possible
    }
}
