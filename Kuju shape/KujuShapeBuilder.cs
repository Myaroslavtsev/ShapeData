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
            
            return (
                AssembleShapeFileParts(shape, bb),
                AssembleShapeDefinitionParts(shape, bb, true, false)
            );
        }

        private static string AssembleShapeFileParts(EditorShape shape, (Vector3, Vector3) bb)
        {
            var sb = new StringBuilder();

            sb.Append(AddShapeStart());
            sb.Append(AddShapeHeader());
            sb.Append(AddShapeVolumes(bb));
            sb.Append(AddShapeShaderNames(shape));
            sb.Append(AddShapeTextureFilterNames());
            /*sb.Append(AddShapePoints());
            sb.Append(AddShapeUvPoints());
            sb.Append(AddShapeNormals());
            sb.Append(AddShapeSortVectorsAndColors());
            sb.Append(AddShapeMatrices());
            sb.Append(AddShapeImages());
            sb.Append(AddShapeTextures());
            sb.Append(AddShapeLightMaterialsAndConfigs());
            sb.Append(AddShapeVertexStates());
            sb.Append(AddShapePrimStates());
            sb.Append(AddShapeLodControls());*/
            sb.Append(AddShapeFinish());

            return sb.ToString();
        }

        private static string AssembleShapeDefinitionParts(
            EditorShape shape, 
            (Vector3, Vector3) bb,
            bool prohibitVisualObstruction = false,
            bool hasWinterTextures = false)
        {
            var sb = new StringBuilder();
            
            sb.Append(AddShapeHeader());
            sb.Append(shape.ShapeName + newLine);
            sb.Append(AddEsdDetailLevel());
            sb.Append(AddAlternativeTextures(hasWinterTextures));
            sb.Append(AddBoundingBox(bb, prohibitVisualObstruction));
            sb.Append(AddShapeFinish());

            return sb.ToString();
        }

        private static (Vector3, Vector3) GetBoundingBox(EditorShape shape)
        {            
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var minZ = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxZ = float.MinValue;

            foreach(var lod in shape.Lods)
                foreach(var part in lod.Parts)
                    foreach(var poly in part.Polygons)
                        foreach(var v in poly.Vertices)
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

        private static string AddShapeTextureFilterNames()
        {
            return Tabs(1) + "texture_filter_names( 1" + newLine +
                Tabs(2) + "named_filter_mode( MipLinear )" + newLine +
                Tabs(1) + ")" + newLine;
        }

        private static string AddShapeShaderNames(EditorShape shape)
        {
            var shaderNames = new List<string>();

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

        private static string AddShapeVolumes((Vector3 minPoint, Vector3 maxPoint) bb)
        {
            var sum = bb.minPoint + bb.maxPoint;
            var delta = bb.maxPoint - bb.minPoint;

            return Tabs(1) + "volumes ( 1" + newLine +
                Tabs(2) + "vol_sphere (" + newLine +
                Tabs(3) + "vector ( " + 0.5 * sum.X + " " + 0.5 * sum.Y + " " + 0.5 * sum.Z + " ) " + 
                0.5 * delta.Length() + newLine +
                Tabs(2) + ")" + newLine +
                Tabs(1) + ")" + newLine;
         }

        private static string AddBoundingBox((Vector3 minPoint, Vector3 maxPoint) bb, bool prohibitVisualObstruction)
        {
            if (prohibitVisualObstruction)
                return Tabs(1) + "ESD_No_Visual_Obstruction ()" + newLine;
            else
                return Tabs(1) + "ESD_Bounding_Box ( " +
                    bb.minPoint.X + " " + bb.minPoint.Y + " " + bb.minPoint.Z + " " +
                    bb.maxPoint.X + " " + bb.maxPoint.Y + " " + bb.maxPoint.Z + " " +
                    " )" + newLine;
        }

        private static string AddAlternativeTextures(bool hasWinterTextures)
        {
            if (hasWinterTextures)
                return Tabs(1) + "ESD_Alternative_Texture(1)" + newLine;
            else
                return Tabs(1) + "ESD_Alternative_Texture(0)" + newLine;
        }

        private static string AddEsdDetailLevel() =>
            Tabs(1) + "ESD_Detail_Level ( 0 )" + newLine;

        private static string AddShapeStart() =>
            simisa + newLine + newLine + "shape(" + newLine;

        private static string AddShapeHeader() =>
            Tabs(1) + "shape_header( 00000000 00000000 )" + newLine;

        private static string AddShapeFinish() =>
            ")" + newLine;

        private static string Tabs(int count)
        {
            var s = "";
            for (int i = 0; i < count; i++)
                s += tab;
            return s;
        }
    }
}
