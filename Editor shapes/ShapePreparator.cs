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
        public int DrawOrder;

        public PrimState(string name, int shaderId, int textureId, bool isBright, bool isTransparent, bool isTrackbed)
        {
            Name = name;
            ShaderId = shaderId;
            TextureId = textureId;
            IsBright = isBright ? 1 : 0;
            IsTransparent = isTransparent ? 1 : 0;
            DrawOrder = isTrackbed ? 3 : 1;
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
            shape.OrderLods();

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
            MakeItemList(shape.Polygons(), p => GetPolygonPrimState(p), (s, sList) => sList.IndexOf(s), (p, id) => p.KujuPrimStateId = id);

        private static List<string> MakeImageList(EditorShape shape) =>
            MakeItemList(shape.Polygons(), p => p.TextureFilename, (i, iList) => iList.IndexOf(i), (p, id) => p.KujuImageId = id);

        private static List<string> MakeShaderList(EditorShape shape) =>
            MakeItemList(shape.Polygons(), p => GetShaderName(p), (s, sList) => sList.IndexOf(s), (p, id) => p.KujuShaderId = id);

        private static List<int> MakeLightMatIdList(EditorShape shape) =>
            MakeItemList(shape.Polygons(), p => GetLightMatId(p), (m, mList) => mList.IndexOf(m), (p, id) => p.KujuLightMatId = id);

        private static PrimState GetPolygonPrimState(EditorPolygon poly) => 
            new ("PS_" + poly.MaterialType.ToString(), // For animated shapes better use matrix name instead of "PS"
                poly.KujuShaderId,
                poly.KujuImageId,
                poly.MaterialType == Material.SolidBright || poly.MaterialType == Material.TransBright,
                poly.MaterialType == Material.TransNorm || poly.MaterialType == Material.TransBright,
                poly.Trackbed);        

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

        private static List<Vector2> MakeUvPointList(EditorShape shape) =>
            MakeItemList(shape.Vertices(), vertex => vertex.UvPosition, 
                (pos, points) => FindVector2InList(points, pos), (vertex, id) => vertex.KujuUvPointId = id);

        private static List<Vector3> MakePointList(EditorShape shape) =>
            MakeItemList(shape.Vertices(), vertex => vertex.Position,
                (pos, points) => FindVector3InList(points, pos), (vertex, id) => vertex.KujuPointId = id);

        private static void MakePolyAndPointNormals(EditorShape shape)
        {            
            foreach (var poly in shape.Polygons())
            {
                var normal = poly.MakeNormal();

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
                var index = FindVector3InList(normals, poly.Normal);

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
                var index = FindVector3InList(normals, vertex.Normal);

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

        private static int FindVector2InList(List<Vector2> list, Vector2 vector)
        {
            for (var i = 0; i < list.Count; i++)
                if (AlmostEquals(list[i], vector))
                    return i;

            return -1;
        }

        private static int FindVector3InList(List<Vector3> list, Vector3 vector)
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

            foreach (var vertex in shape.Vertices())
            {
                 if (vertex.Position.X > maxX) maxX = vertex.Position.X;
                 if (vertex.Position.X < minX) minX = vertex.Position.X;
                 if (vertex.Position.Y > maxY) maxY = vertex.Position.Y;
                 if (vertex.Position.Y < minY) minY = vertex.Position.Y;
                 if (vertex.Position.Z > maxZ) maxZ = vertex.Position.Z;
                 if (vertex.Position.Z < minZ) minZ = vertex.Position.Z;
            }

            maxX += boundingBoxMargin;
            maxY += boundingBoxMargin;
            maxZ += boundingBoxMargin;
            minX -= boundingBoxMargin;
            minY -= boundingBoxMargin;
            minZ -= boundingBoxMargin;

            return (new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        }

        private static List<T> MakeItemList<T, U>(
            IEnumerable<U> objects,
            Func<U, T> getItemFromObject,
            Func<T, List<T>, int> getItemIndex,
            Action<U, int> setItemIndex)
        {
            var itemList = new List<T>();

            foreach (var obj in objects)
            {
                var index = getItemIndex(getItemFromObject(obj), itemList);

                if (index == -1)
                {
                    index = itemList.Count;
                    itemList.Add(getItemFromObject(obj));
                }

                setItemIndex(obj, index);
            }

            return itemList;
        }
    }
}
