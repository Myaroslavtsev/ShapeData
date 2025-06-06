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
        private const string newLine = "\r\n";

        public static (string s, string sd) BuildShapeFile(EditorShape shape)
        {
            var prepared = new ShapePreparator(shape);

            return (
                AssembleShapeFileParts(prepared.BoundingBox, prepared.Points, prepared.UvPoints, 
                    prepared.Normals, prepared.Shaders, prepared.Images),
                AssembleShapeDefinitionParts(shape.ShapeName, prepared.BoundingBox, true, false)
            );
        }

        private static string AssembleShapeFileParts(
            (Vector3, Vector3) bb,
            List<Vector3> points,
            List<Vector2> uvPoints,
            List<Vector3> normals,
            List<string> shaders,
            List<string> images)
        {
            var sb = new StringBuilder();

            var kujuShape = new DataBlock("shape", null);

            sb.Append("SIMISA@@@@@@@@@@JINX0s1t______" + newLine + newLine);

            AddShapeHeader(kujuShape);
            AddShapeVolumes(kujuShape, bb);
            AddShapeShaderNames(kujuShape, shaders);
            AddShapeTextureFilterNames(kujuShape);
            AddShapePoints(kujuShape, points);
            AddShapeUvPoints(kujuShape, uvPoints);
            AddShapeNormals(kujuShape, normals);
            AddShapeSortVectorsAndColours(kujuShape);
            AddShapeMatrices(kujuShape);
            AddShapeImages(kujuShape, images);
            /*sb.Append(AddShapeTextures());
            sb.Append(AddShapeLightMaterialsAndConfigs());
            sb.Append(AddShapeVertexStates());
            sb.Append(AddShapePrimStates());
            sb.Append(AddShapeLodControls());*/

            //AddShapeFinish(sb);

            kujuShape.PrintBlock(sb);

            return sb.ToString();
        }

        private static string AssembleShapeDefinitionParts(
            string shapeName,
            (Vector3, Vector3) boundingBox,
            bool prohibitVisualObstruction = false,
            bool hasWinterTextures = false)
        {
            var sb = new StringBuilder();

            sb.Append("SIMISA@@@@@@@@@@JINX0t1t______" + newLine);

            var kujuShape = new DataBlock("shape", new List<string> { shapeName });
            AddEsdDetailLevel(kujuShape);
            AddAlternativeTextures(kujuShape, hasWinterTextures);
            AddBoundingBox(kujuShape, boundingBox, prohibitVisualObstruction);

            return sb.ToString();
        }

        private static void AddShapeImages(DataBlock shape, List<string> images) =>
            AddItemsToShapeData(shape, images, s => new List<string> { s + ".ace" }, "images", "image");

        private static void AddShapeShaderNames(DataBlock shape, List<string> shaders) =>
            AddItemsToShapeData(shape, shaders, s => new List<string> { s }, "shader_names", "named_shader");

        private static void AddShapeNormals(DataBlock shape, List<Vector3> normals) =>
            AddItemsToShapeData(shape, normals, n => new List<string> { 
                n.X.ToString("0.0000"), n.Y.ToString("0.0000"), n.Z.ToString("0.0000") }, "normals", "vector");

        private static void AddShapeUvPoints(DataBlock shape, List<Vector2> uvPoints) =>
            AddItemsToShapeData(shape, uvPoints, p => new List<string> {
                p.X.ToString("0.0000"), p.Y.ToString("0.0000") }, "uv_points", "uv_point");

        private static void AddShapePoints(DataBlock shape, List<Vector3> points) =>
            AddItemsToShapeData(shape, points, p => new List<string> {
                p.X.ToString("0.0000"), p.Y.ToString("0.0000"), p.Z.ToString("0.0000") }, "points", "point");

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

        private static void AddShapeTextureFilterNames(DataBlock shape)
        {
            var filterMode = new DataBlock("named_filter_mode", new List<string> { "MipLinear" });

            shape.Blocks.Add(new DataBlock("named_filter_mode", new List<string> { "1" }, 
                new List<DataBlock> { filterMode }));
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

        private static void AddShapeHeader(DataBlock shape) =>
            shape.Blocks.Add(new DataBlock("shape_header", new List<string> {"00000000", "00000000"}));

        private static void AddItemsToShapeData<T>(
            DataBlock shape,
            List<T> items,
            Func<T, List<string>> getItemValues,
            string blockName,
            string itemName)
        {
            var dataBlock = new DataBlock(blockName, new List<string> { items.Count.ToString() });

            foreach (var i in items)
                dataBlock.Blocks.Add(new DataBlock(itemName, getItemValues(i)));

            shape.Blocks.Add(dataBlock);
        }
    }
}
