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
        public Material MaterialType { get; set; }

        public bool Trackbed { get; set; }

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
            List<EditorVertex> vertexList,
            Material material = Material.SolidNorm,
            string textureName = "blank",
            bool isTrackbed = false)
        {
            MaterialType = material;
            TextureFilename = textureName;
            vertices = vertexList;
            Trackbed = isTrackbed;
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

            return new EditorPolygon(verticesCopy, MaterialType, TextureFilename);
        }

        // to think about: how to block from deleting last 3 points but allow increasing point number
        // when deserializer requires adding line by line
        // so creation with less than 2 vertices should also be possible

        public string IsTrackbed() =>
            Trackbed ? "Trackbed" : "Typical";

        public void Flip() => Vertices.Reverse();
        
        public Vector3 MakeNormal()
        {
            if (Vertices.Count < 3)
                return new Vector3();

            Normal = Geometry.Geometry.MakePlaneFromFirstPoints(
                new List<Vector3> { Vertices[0].Position, Vertices[1].Position, Vertices[2].Position }).Normal;

            return Normal;
        }
    }
}
