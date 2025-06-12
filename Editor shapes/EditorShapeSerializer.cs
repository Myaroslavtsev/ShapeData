/// Converts EditorShape into string of .csv format with ; separator, which can be read by Excel.

using System.Linq;
using System.Text;

namespace ShapeData
{
    class EditorShapeSerializer
    {
        public static string MakeCsvFromEditorShape(EditorShape shape)
        {
            var sb = new StringBuilder();

            AddShapeDataToSb(shape, sb);

            sb.AppendLine("");

            return sb.ToString();
        }

        private static void AddShapeDataToSb(EditorShape shape, StringBuilder sb)
        {
            sb.AppendLine("Shape" + ';' + shape.ShapeName.Replace(';', ':') + ';' + shape.ShapeComment.Replace(';', ':'));

            foreach (var lod in shape.Lods.OrderBy(l => l.Distance))
            {
                AddLodDataToSb(lod, sb);
            }
        }

        private static void AddLodDataToSb(EditorLod lod, StringBuilder sb)
        {
            sb.AppendLine(';' + "Lod" + ';' + lod.Distance);

            foreach (var part in lod.Parts)
            {
                AddPartDataToSb(part, sb);
            }
        }

        private static void AddPartDataToSb(EditorPart part, StringBuilder sb)
        {
            var smoothed = part.Smoothed ? "Smoothed" : "Unsmoothed";

            var dataString = ";;" + "Part" + ';' +
                part.PartName + ';' +
                smoothed + ';' +
                part.Replication.ReplicationMethod;
            //part.Replication.StretchInWidthMethod;
            //part.Replication.ScalingMethod;
            //part.Replication.BendPart;
            //part.Replication.LeaveAtLeastOne;
            //part.Replication.ScaleTexture;

            foreach (var (Name, Value) in part.Replication.GetReplicationParams())
            {
                dataString += ';' + Name + ';' + Value.ToString("0.0000");
            }

            sb.AppendLine(dataString);

            foreach (var poly in part.Polygons)
            {
                AddPolygonDataToSb(poly, sb);
            }
        }

        private static void AddPolygonDataToSb(EditorPolygon polygon, StringBuilder sb)
        {
            sb.AppendLine(";;;" + "Polygon" + ';' +
                polygon.MaterialType + ';' +
                polygon.TextureFilename + ';' +
                polygon.IsTrackbed());

            foreach (var vertex in polygon.Vertices)
            {
                AddVertexDataToSb(vertex, sb);
            }
        }

        private static void AddVertexDataToSb(EditorVertex vertex, StringBuilder sb)
        {
            sb.AppendLine(";;;;V;" +
                vertex.Position.X.ToString("0.0000") + ';' +
                vertex.Position.Y.ToString("0.0000") + ';' +
                vertex.Position.Z.ToString("0.0000") + ';' +
                vertex.UvPosition.X.ToString("0.00000") + ';' +
                vertex.UvPosition.Y.ToString("0.00000"));
        }
    }
}
