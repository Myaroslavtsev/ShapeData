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
            (List<EditorTrackSection> subsections, EditorTrackSection finalSection, float) newSections,
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
        private static EditorPolygon TransposePoly(EditorPolygon polygon, Direction direction)
        {
            foreach (var v in polygon.Vertices)
                v.Position = Transfigurations.TransposePoint(v.Position, direction);

            return polygon;
        }

        public static (List<EditorPolygon>, List<EditorPolygon>) MakeTypicalAndFinalSegments(
            EditorPart part, 
            (List<EditorTrackSection> subsections, EditorTrackSection finalSection, float scaleFactor) newSections)
        {
            (List<EditorPolygon> typicalSegment, List<EditorPolygon> finalSegment) segments = 
                (new List<EditorPolygon>(), new List<EditorPolygon>());

            EditorTrackSection mainSection = null;
            if (newSections.subsections is not null && newSections.subsections.Count > 0)
                mainSection = newSections.subsections.First();

            switch (part.Replication.ScalingMethod)
            {
                case PartScalingMethod.FixLength:
                case PartScalingMethod.FixLengthAndTrim:
                    segments.typicalSegment = part.Polygons;
                    segments.finalSegment = TrimPolys(part.Polygons, newSections.finalSection, part.Replication.ScaleTexture);
                    break;

                case PartScalingMethod.Stretch:
                    /*double scaleFactor = 1;
                    if (mainSection is not null)
                    {
                        part.Replication.GetReplicationParam("OriginalLength", out var originalLength);
                        scaleFactor = mainSection.Traject.Length / originalLength;
                    }*/

                    segments.typicalSegment = StretchPolys(part.Polygons, newSections.scaleFactor);
                    segments.finalSegment = TrimPolys(StretchPolys(part.Polygons, newSections.scaleFactor), 
                        newSections.finalSection, part.Replication.ScaleTexture);
                    break;
            }

            return (BendPolys(segments.typicalSegment, mainSection, part.Replication.BendPart),
                BendPolys(segments.finalSegment, newSections.finalSection, part.Replication.BendPart));
        }

        private static List<EditorPolygon> StretchPolys(List<EditorPolygon> polygons, float scaleFactor)
        {
            if (polygons is null)
                return null;

            if (scaleFactor == 0)
                scaleFactor = 1;

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

        private static List<EditorPolygon> BendPolys(List<EditorPolygon> polygons, EditorTrackSection section, bool shouldBend)
        {
            if (!shouldBend || section is null || polygons is null)
                return polygons;

            List<EditorPolygon> scaledPolys = new();

            foreach (var poly in polygons)
            {
                var transformedPoly = poly.Copy();

                foreach (var v in transformedPoly.Vertices)
                    v.Position = Transfigurations.BendPoint(v.Position, section.Traject, 1);

                scaledPolys.Add(transformedPoly);
            }

            return scaledPolys;
        }

        private static List<EditorPolygon> TrimPolys(List<EditorPolygon> polygons, EditorTrackSection finalSection, bool scaleTexture)
        {
            if (finalSection is null)
                return null;

            var maxZ = finalSection.Traject.Length;

            var newPolygons = new List<EditorPolygon>();

            foreach(var poly in polygons)
            {
                var pointsToTrim = poly.Vertices.Select(v => v.Position.Z).Count(z => z + Accuracy > maxZ);

                if (pointsToTrim == 0)
                {
                    newPolygons.Add(poly);
                    continue;
                }

                if (pointsToTrim == poly.Vertices.Count)
                {
                    continue;
                }

                var newPoly = poly.Copy();

                foreach (var v in newPoly.Vertices)
                {
                    if (v.Position.Z > maxZ)
                    {
                        if (!scaleTexture)
                            v.UvPosition = new Vector2(v.UvPosition.X, (float)(v.UvPosition.Y * v.Position.Z / maxZ));
                        v.Position = new Vector3(v.Position.X, v.Position.Y, (float)maxZ);
                    }
                }

                newPolygons.Add(newPoly);
            }

            return newPolygons;
        }
    }
}
