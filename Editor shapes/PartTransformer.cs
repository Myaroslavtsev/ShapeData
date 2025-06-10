/// Performs various transformations with EditorShape instances and their polygons

using ShapeData.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace ShapeData.Editor_shapes
{
    class PartTransformer
    {
        public static EditorPart TransposePart(EditorPart part, Direction direction)
        {
            foreach (var v in part.Vertices()) 
                v.Position = Transfigurations.TransposePoint(v.Position, direction);

            return part;
        }

        public static EditorPolygon TransposePoly(EditorPolygon polygon, Direction direction)
        {
            foreach (var v in polygon.Vertices)
                v.Position = Transfigurations.TransposePoint(v.Position, direction);

            return polygon;
        }

        public static List<EditorPolygon> ScaleAndBendPart(EditorPart part, EditorTrackSection subsection)
        {            
            float scaleFactor = 1;

            if (part.Replication.GetReplicationParam("OriginalLength", out var originalLength))
                scaleFactor = (float)subsection.Traject.Length / originalLength;

            if (part.Replication.BendPart)
                return ScaleAndBendPolys(part.Polygons, scaleFactor, subsection.Traject);
            else
                return ScalePolys(part.Polygons, scaleFactor);
        }

        private static List<EditorPolygon> ScaleAndBendPolys(List<EditorPolygon> polygons, float scaleFactor, Trajectory bendTrajectory)
        {
            List<EditorPolygon> scaledPolys = new();

            foreach (var poly in polygons)
            {
                var transformedPoly = poly.Copy();

                foreach (var v in transformedPoly.Vertices)
                    v.Position = Transfigurations.BendPoint(v.Position, bendTrajectory, scaleFactor);

                scaledPolys.Add(transformedPoly);                
            }

            return scaledPolys;
        }

        private static List<EditorPolygon> ScalePolys(List<EditorPolygon> polygons, float scaleFactor)
        {            
            List<EditorPolygon> scaledPolys = new();
            
            foreach (var poly in polygons)
            {
                var transformedPoly = poly.Copy();

                foreach (var v in transformedPoly.Vertices)
                    v.Position = new(v.Position.X, v.Position.Y, v.Position.Z * scaleFactor);

                scaledPolys.Add(transformedPoly);
            }            

            return scaledPolys;
        }

        public static List<EditorPolygon> TrimPart(EditorPart part, EditorTrackSection finalSection)
        {
            if (finalSection is null)
                return null;

            var maxZ = finalSection.Traject.Length;

            for (int i = 0; i < part.Polygons.Count; i++)
            {
                var pointsToTrim = part.Polygons[i].Vertices.Select(v => v.Position.Z).Count(z => z > maxZ);

                if (pointsToTrim == 0)
                    continue;

                if (pointsToTrim == part.Polygons[i].Vertices.Count)
                {
                    part.Polygons.RemoveAt(i);
                    continue;
                }    

                foreach(var v in part.Polygons[i].Vertices)
                {
                    if (v.Position.Z > maxZ)
                    {
                        if (!part.Replication.ScaleTexture) 
                            v.UvPosition = new Vector2(v.UvPosition.X, (float)(v.UvPosition.Y * v.Position.Z / maxZ));
                        v.Position = new Vector3(v.Position.X, v.Position.Z, (float)maxZ);
                    }
                }
            }

            return part.Polygons;
        }

        public static EditorPart AssemblePartSections(EditorPart part, 
            List<EditorTrackSection> subsections, EditorTrackSection finalSection, 
            List<EditorPolygon> typicalSegment, List<EditorPolygon> finalSegment)
        {
            var assembly = new EditorPart(part.PartName, PartReplication.NoReplication());

            if (part.Replication.BendPart)
            {
                foreach (var p in typicalSegment)
                    foreach (var s in subsections)
                    assembly.AddPolygon(TransposePoly(p, s.StartDirection));

                if (finalSection is not null)
                    foreach (var p in finalSegment)
                        assembly.AddPolygon(TransposePoly(p, finalSection.StartDirection));
            }
            else
            {
                foreach (var p in typicalSegment)
                    foreach (var s in subsections)
                    {
                        var rotatedDir = new Direction(s.StartDirection.X, s.StartDirection.Y, s.StartDirection.Z,
                            0.5 * (s.StartDirection.A + s.EndDirection.A));
                        assembly.AddPolygon(TransposePoly(p, rotatedDir));
                    }

                if (finalSection is not null)
                    foreach (var p in finalSegment)
                    {
                        var rotatedDir = new Direction(finalSection.StartDirection.X, finalSection.StartDirection.Y, finalSection.StartDirection.Z,
                                0.5 * (finalSection.StartDirection.A + finalSection.EndDirection.A));
                        assembly.AddPolygon(TransposePoly(p, rotatedDir));
                    }
            }

            return assembly;
        }
    }
}
