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

        private static string AssembleShapeFileParts(EditorShape shape, (Point, Point) bb)
        {
            var sb = new StringBuilder();

            sb.Append(AddShapeStart());
            sb.Append(AddShapeHeader());
            sb.Append(AddShapeVolumes(bb));
            sb.Append(AddShapeShaderNames());
            sb.Append(AddShapeTextureFilterNames());
            sb.Append(AddShapePoints());
            sb.Append(AddShapeUvPoints());
            sb.Append(AddShapeNormals());
            sb.Append(AddShapeSortVectorsAndColors());
            sb.Append(AddShapeMatrices());
            sb.Append(AddShapeImages());
            sb.Append(AddShapeTextures());
            sb.Append(AddShapeLightMaterialsAndConfigs());
            sb.Append(AddShapeVertexStates());
            sb.Append(AddShapePrimStates());
            sb.Append(AddShapeLodControls());
            sb.Append(AddShapeFinish());

            return sb.ToString();
        }

        private static string AssembleShapeDefinitionParts(
            EditorShape shape, 
            (Point, Point) bb,
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

        private static (Point, Point) GetBoundingBox(EditorShape shape)
        {
            const double margin = 0.5d;

            var minPoint = new Point(double.MaxValue, double.MaxValue, double.MaxValue);
            var maxPoint = new Point(double.MinValue, double.MinValue, double.MinValue);

            foreach(var lod in shape.Lods)
                foreach(var part in lod.Parts)
                    foreach(var poly in part.Polygons)
                        foreach(var v in poly.Vertices)
                        {
                            if (v.Position.X > maxPoint.X) maxPoint.X = v.Position.X;
                            if (v.Position.X < minPoint.X) minPoint.X = v.Position.X;
                            if (v.Position.Y > maxPoint.Y) maxPoint.Y = v.Position.Y;
                            if (v.Position.Y < minPoint.Y) minPoint.Y = v.Position.Y;
                            if (v.Position.Z > maxPoint.Z) maxPoint.Z = v.Position.Z;
                            if (v.Position.Z < minPoint.Z) minPoint.Z = v.Position.Z;
                        }

            maxPoint.X += margin;
            maxPoint.Y += margin;
            maxPoint.Z += margin;
            minPoint.X -= margin;
            minPoint.Y -= margin;
            minPoint.Z -= margin;

            return (minPoint, maxPoint);
        }

        private static string AddShapeVolumes((Point minPoint, Point maxPoint) bb)
        {
            Vector3 min = new Vector3(0, 0, 0);
            Vector3 max = new Vector3(0, 0, 0);


            var sum = min - max;

            var sumX = bb.maxPoint.X + bb.minPoint.X;
            var sumY = bb.maxPoint.Y + bb.minPoint.Y;
            var sumZ = bb.maxPoint.Z + bb.minPoint.Z;

            var dX = bb.maxPoint.X - bb.minPoint.X;
            var dY = bb.maxPoint.Y - bb.minPoint.Y;
            var dZ = bb.maxPoint.Z - bb.minPoint.Z;

            return Tabs(1) + "volumes ( 1" + newLine +
                Tabs(2) + "vol_sphere (" + newLine +
                Tabs(3) + "vector ( " + 0.5 * sumX + " " + 0.5 * sumY + " " + 0.5 * sumZ + " ) " +
                Math.Sqrt(0.5 * (dX*dX + dY*dY + dZ*dZ)) + newLine +
                Tabs(2) + ")" + newLine +
                Tabs(1) + ")" + newLine;
         }

        private static string AddBoundingBox((Point minPoint, Point maxPoint) bb, bool prohibitVisualObstruction)
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
