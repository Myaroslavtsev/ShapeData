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
        const float Accuracy = 1e-4f;

        public static EditorPart AssemblePartSegments(EditorPart oldPart,
            (List<EditorTrackSection> subsections, EditorTrackSection finalSection) newSections,
            (List<EditorPolygon> typicalSegment, List<EditorPolygon> finalSegment) segments)
        {
            var assembledPart = new EditorPart(oldPart.PartName, PartReplication.NoReplication())
            {
                Smoothed = oldPart.Smoothed
            };

            double typicalRotation = 0;
            double endRotation = 0;

            if (!oldPart.Replication.BendPart)
            {
                if (newSections.subsections is not null && newSections.subsections.Count > 0)
                    typicalRotation = newSections.subsections[0].Traject.Angle / 2;
                if (newSections.finalSection is not null)
                    endRotation = newSections.finalSection.Traject.Angle / 2;
            }

            if (newSections.subsections is not null)
                foreach (var section in newSections.subsections)
                    AddPolysAtDirection(assembledPart, segments.typicalSegment, section, typicalRotation);

            AddPolysAtDirection(assembledPart, segments.finalSegment, newSections.finalSection, endRotation);

            return assembledPart;
        }

        private static void AddPolysAtDirection(
            EditorPart part, 
            List<EditorPolygon> segment, 
            EditorTrackSection section, 
            double additionalRotation)
        {
            if (segment is null || section is null)
                return;

            var newDirection = new Direction(section.StartDirection.X, section.StartDirection.Y, section.StartDirection.Z,
                section.StartDirection.A + additionalRotation);

            foreach (var poly in segment)
            {
                var newPoly = poly.Copy();
                part.AddPolygon(TransposePoly(newPoly, newDirection));
            }
                
        }

        public static (List<EditorPolygon>, List<EditorPolygon>) MakeTypicalAndFinalSegments(
            EditorPart part, 
            (List<EditorTrackSection> subsections, EditorTrackSection finalSection) newSections)
        {
            (List<EditorPolygon> typicalSegment, List<EditorPolygon> finalSegment) segments = 
                (new List<EditorPolygon>(), new List<EditorPolygon>());

            if (newSections.subsections is not null && newSections.subsections.Count > 0)
                segments.typicalSegment = ScaleAndBendPart(part.Copy(false), newSections.subsections.First());
            if (newSections.finalSection is not null)
                segments.finalSegment = TrimPart(part.Copy(false), newSections.finalSection);

            return segments;
        }

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
            {
                if (originalLength != 0)
                    scaleFactor = (float)subsection.Traject.Length / originalLength;
                else
                    scaleFactor = 1;
            }
                
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

            var newPolygons = new List<EditorPolygon>();

            for (int i = 0; i < part.Polygons.Count; i++)
            {
                var pointsToTrim = part.Polygons[i].Vertices.Select(v => v.Position.Z).Count(z => z + Accuracy > maxZ);

                if (pointsToTrim == 0)
                {
                    newPolygons.Add(part.Polygons[i]);
                    continue;
                }
                    
                if (pointsToTrim == part.Polygons[i].Vertices.Count)
                {
                    //part.Polygons.RemoveAt(i);
                    continue;
                }    

                foreach(var v in part.Polygons[i].Vertices)
                {
                    if (v.Position.Z > maxZ)
                    {
                        if (!part.Replication.ScaleTexture) 
                            v.UvPosition = new Vector2(v.UvPosition.X, (float)(v.UvPosition.Y * v.Position.Z / maxZ));
                        v.Position = new Vector3(v.Position.X, v.Position.Y, (float)maxZ);
                    }
                }

                newPolygons.Add(part.Polygons[i]);
            }

            return newPolygons;
        }
    }
}
