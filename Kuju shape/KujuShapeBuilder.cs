using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Editor_shapes;
using ShapeData.Geometry;
using System.Numerics;

namespace ShapeData.Kuju_shape
{
    class KujuShapeBuilder
    {
        const float boundingBoxMargin = 0.5f;

        private const string newLine = "\r\n";
        private const string tab = "	";
        private const string simisa = "SIMISA@@@@@@@@@@JINX0s1t______";

        public static (string s, string sd) BuildShapeFile(EditorShape shape)
        {
            var bb = GetBoundingBox(shape);

            var points = MakePointList(shape);
            var uvPoints = MakeUvPointList(shape);
            var normals = MakeNormalList(shape);

            return (
                AssembleShapeFileParts(shape, bb, points, uvPoints, normals),
                AssembleShapeDefinitionParts(shape, bb, true, false)
            );
        }

        private static string AssembleShapeFileParts(
            EditorShape shape,
            (Vector3, Vector3) bb,
            List<Vector3> points,
            List<Vector2> uvPoints,
            List<Vector3> normals)
        {
            var sb = new StringBuilder();

            var kujuShape = new DataBlock("shape", null);

            AddShapeHeader(kujuShape);
            AddShapeVolumes(kujuShape, bb);
            //AddShapeShaderNames(kujuShape, shape);
            AddShapeTextureFilterNames(kujuShape);
            AddShapePoints(kujuShape, points);
            AddShapeUvPoints(kujuShape, uvPoints);
            AddShapeNormals(kujuShape, normals);
            AddShapeSortVectorsAndColours(kujuShape);
            AddShapeMatrices(kujuShape);
            /*sb.Append(AddShapeImages());
            sb.Append(AddShapeTextures());
            sb.Append(AddShapeLightMaterialsAndConfigs());
            sb.Append(AddShapeVertexStates());
            sb.Append(AddShapePrimStates());
            sb.Append(AddShapeLodControls());*/

            //AddShapeFinish(sb);

            kujuShape.PrintBlock(sb);

            return sb.ToString();
        }

        private static string AssembleShapeDefinitionParts(
            EditorShape shape,
            (Vector3, Vector3) boundingBox,
            bool prohibitVisualObstruction = false,
            bool hasWinterTextures = false)
        {
            var sb = new StringBuilder();

            var kujuShape = new DataBlock("shape", new List<string> { shape.ShapeName });
            AddEsdDetailLevel(kujuShape);
            AddAlternativeTextures(kujuShape, hasWinterTextures);
            AddBoundingBox(kujuShape, boundingBox, prohibitVisualObstruction);

            return sb.ToString();
        }

        private static void AddShapeMatrices(DataBlock shape)
        {
            // implemented correctly for non-animated objects only
            // difference between Square, Box, Polygon matrices unknown but may be crucial
            var matrix = new DataBlock("matrix Box ", new List<string> { "1 0 0 0 1 0 0 0 1 0 0 0" });
            
            shape.Blocks.Add(new DataBlock("matrices", new List<string> { "1" }, new List<DataBlock> { matrix }));
        }

        private static void AddShapeSortVectorsAndColours(DataBlock shape)
        {
            shape.Blocks.Add(new DataBlock("sort_vectors", new List<string> { "0" }));
            
            shape.Blocks.Add(new DataBlock("colours", new List<string> { "0" }));
        }

        private static void AddShapeNormals(DataBlock shape, List<Vector3> normals)
        {
            var normalsBlock = new DataBlock("normals", new List<string> { normals.Count.ToString() });

            foreach (var n in normals)
                normalsBlock.Blocks.Add(new DataBlock("vector", new List<string> {  
                    n.X.ToString("0.0000"), n.Y.ToString("0.0000"), n.Z.ToString("0.0000") }));

            shape.Blocks.Add(normalsBlock);
        }

        private static void AddShapeUvPoints(DataBlock shape, List<Vector2> uvPoints)
        {
            var uvPointsBlock = new DataBlock("uv_points", new List<string> { uvPoints.Count.ToString() });

            foreach (var p in uvPoints)
                uvPointsBlock.Blocks.Add(new DataBlock("uv_point", new List<string> {
                    p.X.ToString("0.0000"), p.Y.ToString("0.0000") }));

            shape.Blocks.Add(uvPointsBlock);
        }

        private static void AddShapePoints(DataBlock shape, List<Vector3> points)
        {
            var pointsBlock = new DataBlock("points", new List<string> { points.Count.ToString() });

            foreach (var p in points)
                pointsBlock.Blocks.Add(new DataBlock("point", new List<string> {
                    p.X.ToString("0.0000"), p.Y.ToString("0.0000"), p.Z.ToString("0.0000") }));

            shape.Blocks.Add(pointsBlock);
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

        private static void AddShapeTextureFilterNames(DataBlock shape)
        {
            var filterMode = new DataBlock("named_filter_mode", new List<string> { "MipLinear" });

            shape.Blocks.Add(new DataBlock("named_filter_mode", new List<string> { "1" }, 
                new List<DataBlock> { filterMode }));
        }

        private static string AddShapeShaderNames(EditorShape shape)
        {
            var shaderNames = new HashSet<string>();

            foreach (var lod in shape.Lods)
                foreach (var part in lod.Parts)
                    foreach (var poly in part.Polygons)
                    {
                        if (poly.MaterialType == Material.SolidNorm || poly.MaterialType == Material.SolidBright)
                            shaderNames.Add("TextDiff");
                        if (poly.MaterialType == Material.TransNorm || poly.MaterialType == Material.TransBright)
                            shaderNames.Add("BlendATexDiff");
                    }

            var s = Tabs(1) + "shader_names( " + shaderNames.Count + newLine;

            foreach (var shader in shaderNames)
                s += Tabs(2) + "named_shader ( " + shader + " )" + newLine;

            return s + Tabs(1) + ")" + newLine;
        }

        private static void AddShapeVolumes(DataBlock shape, (Vector3 minPoint, Vector3 maxPoint) bb)
        {
            var sum = bb.minPoint + bb.maxPoint;
            var delta = bb.maxPoint - bb.minPoint;

            var vector = new DataBlock("vector", new List<string> {
                            (0.5 * sum.X).ToString("0.0000"),
                            (0.5 * sum.Y).ToString("0.0000"),
                            (0.5 * sum.Z).ToString("0.0000")
                        }, null, new List<string> {
                            (0.5 * delta.Length()).ToString("0.0000")
                        });

            var vol_sphere = new DataBlock("vol_sphere", null, new List<DataBlock> { vector });

            shape.Blocks.Add (new DataBlock("volumes", new List<string> { "1" }, new List<DataBlock> {  vol_sphere } ));            
        }

        private static void AddBoundingBox(DataBlock shape, (Vector3 minPoint, Vector3 maxPoint) bb, bool prohibitVisualObstruction)
        {
            if (prohibitVisualObstruction)
                shape.Blocks.Add(new DataBlock("ESD_No_Visual_Obstruction", null));
            else
                shape.Blocks.Add(new DataBlock("ESD_Bounding_Box", new List<string> {
                    bb.minPoint.X.ToString("0.0000"), bb.minPoint.Y.ToString("0.0000"), bb.minPoint.Z.ToString("0.0000"),
                    bb.maxPoint.X.ToString("0.0000"), bb.maxPoint.Y.ToString("0.0000"), bb.maxPoint.Z.ToString("0.0000") }));
        }

        private static void AddAlternativeTextures(DataBlock shape, bool hasWinterTextures)
        {
            if (hasWinterTextures)
                shape.Blocks.Add(new DataBlock("ESD_Alternative_Texture", new List<string> { "1" }));
            else
                shape.Blocks.Add(new DataBlock("ESD_Alternative_Texture", new List<string> { "0" }));
        }

        private static void AddEsdDetailLevel(DataBlock shape) =>
            shape.Blocks.Add(new DataBlock("ESD_Detail_Level", new List<string> { "0" }));

        private static void AddShapeStart(StringBuilder sb) =>
            sb.Append(simisa + newLine + newLine);

        private static void AddShapeHeader(DataBlock shape) =>
            shape.Blocks.Add(new DataBlock("shape_header", new List<string> {"00000000", "00000000"}));

        private static void AddMultiLineBlock(StringBuilder sb, int tab, string blockName, List<string> data)
        {
            sb.Append(Tabs(tab) + blockName + " ( " + data.Count + newLine);

            sb.Append(Tabs(tab) + " )" + newLine);
        }

        private static void AddOneLineBlock(StringBuilder sb, int tab, string blockName, string data)
        {
            sb.Append( Tabs(tab) + blockName + " ( " + data + " )" + newLine);
        }

        private static string Tabs(int count)
        {
            var s = "";
            for (int i = 0; i < count; i++)
                s += tab;
            return s;
        }
    }
}
