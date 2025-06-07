using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ShapeData.Editor_shapes
{
    struct PrimState
    {
        public string Name;
        public int ShaderId;
        public int TextureId;
        public int IsBright;
        public int IsTransparent;

        public PrimState(string name, int shaderId, int textureId, bool isBright, bool isTransparent)
        {
            Name = name;
            ShaderId = shaderId;
            TextureId = textureId;
            IsBright = isBright ? 1 : 0;
            IsTransparent = isTransparent ? 1 : 0;
        }
    }

    class ShapePreparator
    {
        const float boundingBoxMargin = 0.5f;

        public List<Vector3> Points { get; private set; }
        public List<Vector2> UvPoints { get; private set; }
        public List<Vector3> Normals { get; private set; }
        public List<string> Shaders { get; private set; }
        public List<string> Images { get; private set; }
        public List<int> LightMatIds { get; private set; }
        public List<PrimState> PrimStates { get; private set; }
        public (Vector3, Vector3) BoundingBox { get; private set; }  
        public List<List<(int pointId, int normalId, int uvPointId)>> VerticeLists { get; private set; }

        public ShapePreparator(EditorShape shape)
        {            
            BoundingBox = GetBoundingBox(shape);
            Points = MakePointList(shape);
            UvPoints = MakeUvPointList(shape);            
            Normals = MakeNormalList(shape);
            Shaders = MakeShaderList(shape);
            Images = MakeImageList(shape);
            LightMatIds = MakeLightMatIdList(shape);
            PrimStates = MakePrimStateList(shape);
            VerticeLists = MakeVerticeLists(shape);
        }

        private static List<List<(int, int, int)>> MakeVerticeLists(EditorShape shape)
        {
            var result = new List<List<(int, int, int)>>();

            foreach (var lod in shape.Lods)
            {
                var verticeList = new List<(int, int, int)>();
                
                foreach (var v in lod.Vertices())
                {
                    var id = verticeList.IndexOf((v.KujuPointId, v.KujuNormalId, v.KujuUvPointId));

                    if (id < 0)
                    {
                        id = verticeList.Count;
                        verticeList.Add((v.KujuPointId, v.KujuNormalId, v.KujuUvPointId));                        
                    }

                    v.KujuVertexId = id;
                }

                result.Add(verticeList);
                lod.KujuVerticeList = verticeList;                
            }

            return result;
        }

        private static List<PrimState> MakePrimStateList(EditorShape shape) =>
            MakePolygonPropertiesList(shape, p => GetPolygonPrimState(p), (p, id) => p.KujuPrimStateId = id);

        private static List<string> MakeImageList(EditorShape shape) =>
            MakePolygonPropertiesList(shape, p => p.TextureFilename, (p, id) => p.KujuImageId = id);

        private static List<string> MakeShaderList(EditorShape shape) =>
            MakePolygonPropertiesList(shape, p => GetShaderName(p), (p, id) => p.KujuShaderId = id);

        private static List<int> MakeLightMatIdList(EditorShape shape) =>
            MakePolygonPropertiesList(shape, p => GetLightMatId(p), (p, id) => p.KujuLightMatId = id);

        private static PrimState GetPolygonPrimState(EditorPolygon poly) => 
            new ("PS_" + poly.MaterialType.ToString(), // For animated shapes better use matrix name instead of "PS"
                poly.KujuShaderId,
                poly.KujuImageId,
                poly.MaterialType == Material.SolidBright || poly.MaterialType == Material.TransBright,
                poly.MaterialType == Material.TransNorm || poly.MaterialType == Material.TransBright);        

        private static string GetShaderName(EditorPolygon poly)
        {
            if (poly.MaterialType == Material.SolidNorm || poly.MaterialType == Material.SolidBright)
                return "TexDiff";
            if (poly.MaterialType == Material.TransNorm || poly.MaterialType == Material.TransBright)
                return "BlendATexDiff";
            return "BlendATexDiff"; // valid for Alph materials also
        }

        private static int GetLightMatId(EditorPolygon poly)
        {
            if (poly.MaterialType == Material.SolidBright || poly.MaterialType == Material.TransBright)
                return -8;
            return -5;
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

        private static void MakePolyAndPointNormals(EditorShape shape)
        {            

            foreach (var poly in shape.Polygons())
            {
                var basePoints = new List<Vector3> {
                    poly.Vertices[0].Position, poly.Vertices[1].Position, poly.Vertices[2].Position };
                var normal = Geometry.Geometry.MakePlaneFromFirstPoints(basePoints).Normal;

                poly.Normal = normal;

                foreach (var vertex in poly.Vertices)                
                    vertex.Normal = normal;                
            }

            
        }

        private static void JoinSmoothedNormalsInShape(EditorShape shape)
        {
            foreach(var part in shape.Parts())
                if (part.Smoothed)                
                    JoinSmoothedNormalsInPart(part);                
        }

        private static void JoinSmoothedNormalsInPart(EditorPart part)
        {
            var pointNormals = new Dictionary<int, List<Vector3>>();

            foreach (var v in part.Vertices())
            {
                if (!pointNormals.ContainsKey(v.KujuPointId))
                    pointNormals.Add(v.KujuPointId, new List<Vector3>());

                pointNormals[v.KujuPointId].Add(v.Normal);
            }

            var newNormals = new Dictionary<int, Vector3>();

            foreach(var normalSet in pointNormals)
            {
                var newNormal = new Vector3(0, 0, 0);

                foreach (var oldNormal in normalSet.Value)
                    newNormal += oldNormal;

                if (normalSet.Value.Count > 1)
                    newNormal = Vector3.Normalize(newNormal);

                newNormals.Add(normalSet.Key, newNormal);
            }

            foreach (var v in part.Vertices())
                v.Normal = newNormals[v.KujuPointId];
        }

        private static List<Vector3> MakeNormalList(EditorShape shape)
        {
            MakePolyAndPointNormals(shape);

            JoinSmoothedNormalsInShape(shape);

            return SetNormalIds(shape);
        }

        private static List<Vector3> SetNormalIds(EditorShape shape)
        {            
            var normals = new List<Vector3>();

            foreach (var poly in shape.Polygons())
            {
                var index = FindVectorInList(normals, poly.Normal);

                if (index == -1)
                {
                    poly.KujuNormalId = normals.Count;
                    normals.Add(poly.Normal);
                }
                else
                    poly.KujuNormalId = index;
            }

            foreach (var vertex in shape.Vertices())
            {
                var index = FindVectorInList(normals, vertex.Normal);

                if (index == -1)
                {
                    vertex.KujuNormalId = normals.Count;
                    normals.Add(vertex.Normal);
                }
                else
                    vertex.KujuNormalId = index;
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

            foreach (var v in shape.Vertices())
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

        private static List<T> MakePolygonPropertiesList<T>(
            EditorShape shape,
            Func<EditorPolygon, T> getPropertyValue,
            Action<EditorPolygon, int> kujuIndexSetter
            )
        {
            var list = new List<T>();

            foreach (var poly in shape.Polygons())
            {
                var value = getPropertyValue(poly);
                var itemId = list.IndexOf(value);

                if (itemId < 0)
                {
                    itemId = list.Count;
                    list.Add(value);
                }

                kujuIndexSetter(poly, itemId);
            }

            return list;
        }
    }
}
