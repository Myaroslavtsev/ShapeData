using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ShapeData.Editor_shapes
{
    class ShapePreparator
    {
        const float boundingBoxMargin = 0.5f;

        public List<Vector3> Points { get; private set; }
        public List<Vector2> UvPoints { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<string> Shaders { get; private set; }
        public List<string> Images { get; private set; }
        public (Vector3, Vector3) BoundingBox { get; private set; }

        public ShapePreparator(EditorShape shape)
        {
            Points = MakePointList(shape);
            UvPoints = MakeUvPointList(shape);
            Normals = MakeNormalList(shape);
            Shaders = MakeShaderList(shape);
            Images = MakeImageList(shape);
        }

        private static List<string> MakeImageList(EditorShape shape)
        {
            var images = new List<string>();

            foreach (var poly in shape.Polygons())
            {
                var id = images.IndexOf(poly.TextureFilename);

                if (id < 0)
                {
                    id = images.Count;
                    images.Add(poly.TextureFilename);
                }

                poly.KujuImageId = id;
            }

            return images;
        }

        private static List<string> MakeShaderList(EditorShape shape)
        {
            var shaders = new List<string>();

            foreach (var poly in shape.Polygons())
            {
                var id = shaders.IndexOf(GetShaderName(poly));

                if (id < 0)
                {
                    id = shaders.Count;
                    shaders.Add(poly.TextureFilename);
                }

                poly.KujuShaderId = id;
            }

            return shaders;
        }

        private static string GetShaderName(EditorPolygon poly)
        {
            if (poly.MaterialType == Material.SolidNorm || poly.MaterialType == Material.SolidBright)
                return "TextDiff";
            if (poly.MaterialType == Material.TransNorm || poly.MaterialType == Material.TransBright)
                return "BlendATexDiff";
            return "BlendATexDiff"; // for Alph materials also
        }
        private static List<Vector2> MakeUvPointList(EditorShape shape)
        {
            var points = new List<Vector2>();

            foreach (var part in shape.Parts())
            {
                var partPoints = new List<Vector2>();

                foreach (var vertex in part.Vertices())
                {
                    var index = FindVectorInList(partPoints, vertex.UvPosition);

                    if (index == -1)
                    {
                        vertex.KujuUvPointId = points.Count;

                        partPoints.Add(vertex.UvPosition);
                        points.Add(vertex.UvPosition);
                    }
                    else
                        vertex.KujuUvPointId = index;
                }
            }

            return points;
        }

        private static List<Vector3> MakePointList(EditorShape shape)
        {
            var points = new List<Vector3>();

            foreach (var part in shape.Parts())
            {
                var partPoints = new List<Vector3>();

                foreach (var vertex in part.Vertices())
                {
                    var index = FindVectorInList(partPoints, vertex.Position);

                    if (index == -1)
                    {
                        vertex.KujuPointId = points.Count;

                        partPoints.Add(vertex.Position);
                        points.Add(vertex.Position);
                    }
                    else
                        vertex.KujuPointId = index;
                }
            }

            return points;
        }

        private static List<Vector3> MakeNormalList(EditorShape shape)
        {
            var normals = new List<Vector3>();

            foreach (var poly in shape.Polygons())
            {
                var basePoints = new List<Vector3> {
                    poly.Vertices[0].Position, poly.Vertices[1].Position, poly.Vertices[2].Position };
                var normal = Geometry.Geometry.MakePlaneFromFirstPoints(basePoints).Normal;

                var index = FindVectorInList(normals, normal);

                if (index == -1)
                {
                    poly.KujuNormalId = normals.Count;

                    normals.Add(normal);
                }
                else
                    poly.KujuNormalId = index;
            }

            return normals;
        }

        private static int FindVectorInList(List<Vector2> list, Vector2 vector)
        {
            for (var i = 0; i < list.Count; i++)
                if (AlmostEquals(list[i], vector))
                    return i;

            return -1;
        }

        private static int FindVectorInList(List<Vector3> list, Vector3 vector)
        {
            for (var i = 0; i < list.Count; i++)
                if (AlmostEquals(list[i], vector))
                    return i;

            return -1;
        }

        private static bool AlmostEquals(Vector2 v1, Vector2 v2)
        {
            const float accuracy = 5e-4f;

            return (Math.Abs(v2.X - v1.X) < accuracy) &&
                (Math.Abs(v2.Y - v1.Y) < accuracy);
        }

        private static bool AlmostEquals(Vector3 v1, Vector3 v2)
        {
            const float accuracy = 2e-4f;

            return (Math.Abs(v2.X - v1.X) < accuracy) &&
                (Math.Abs(v2.Y - v1.Y) < accuracy) &&
                (Math.Abs(v2.Z - v1.Z) < accuracy);
        }

        private static (Vector3, Vector3) GetBoundingBox(EditorShape shape)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var minZ = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxZ = float.MinValue;

            foreach (var lod in shape.Lods)
                foreach (var part in lod.Parts)
                    foreach (var poly in part.Polygons)
                        foreach (var v in poly.Vertices)
                        {
                            if (v.Position.X > maxX) maxX = v.Position.X;
                            if (v.Position.X < minX) minX = v.Position.X;
                            if (v.Position.Y > maxY) maxY = v.Position.Y;
                            if (v.Position.Y < minY) minY = v.Position.Y;
                            if (v.Position.Z > maxZ) maxZ = v.Position.Z;
                            if (v.Position.Z < minZ) minZ = v.Position.Z;
                        }

            maxX += boundingBoxMargin;
            maxY += boundingBoxMargin;
            maxZ += boundingBoxMargin;
            minX -= boundingBoxMargin;
            minY -= boundingBoxMargin;
            minZ -= boundingBoxMargin;

            return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }
    }
}
