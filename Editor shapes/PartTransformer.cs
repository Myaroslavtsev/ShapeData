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
            (List<EditorTrackSection> subsections, EditorTrackSection finalSection, float, float) newSections,
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
                var copiedPoly = poly.Copy();
                var transformedPoly = TransposePoly(copiedPoly, newDirection);
                part.AddPolygon(transformedPoly);
            }
                
        }
        private static EditorPolygon TransposePoly(EditorPolygon polygon, Direction direction)
        {
            if (polygon.Vertices.Count < 3)
                return polygon;

            foreach (var v in polygon.Vertices)
                v.Position = Transfigurations.TransposePoint(v.Position, direction);

            return polygon;
        }

        public static (List<EditorPolygon>, List<EditorPolygon>) MakeTypicalAndFinalSegments(
            EditorPart part, 
            (List<EditorTrackSection> subsections, EditorTrackSection finalSection, float scaleFactor, float textureScale) newSections)
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
                    
                    segments.finalSegment = TrimPolys(part.Polygons, newSections.finalSection);
                    break;

                case PartScalingMethod.Stretch:
                    segments.typicalSegment = StretchPolys(part.Polygons, newSections.scaleFactor, part.Replication.PreserveTextureDimension);
                    var isStraigtSectionByDeflection = newSections.finalSection is not null &&
                        part.Replication.ReplicationMethod == PartReplicationMethod.ByDeflection && newSections.finalSection.Traject.Radius == 0;
                    
                    segments.finalSegment = TrimPolys(StretchPolys(part.Polygons, newSections.scaleFactor, 
                        part.Replication.PreserveTextureDimension), newSections.finalSection);
                    if (isStraigtSectionByDeflection)
                        StretchTexture(segments.finalSegment, newSections.textureScale);                    
                    break;
            }

            return (BendPolys(segments.typicalSegment, mainSection, part.Replication.BendPart),
                BendPolys(segments.finalSegment, newSections.finalSection, part.Replication.BendPart));
        }

        private static void StretchTexture(List<EditorPolygon> polygons, float scaleFactor)
        {
            foreach (var poly in polygons)
                foreach (var v in poly.Vertices)
                    v.UvPosition = new(v.UvPosition.X, v.UvPosition.Y * scaleFactor);
        }

        private static List<EditorPolygon> StretchPolys(List<EditorPolygon> polygons, float scaleFactor, bool preserveTextureDimension)
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
                {
                    v.Position = new(v.Position.X, v.Position.Y, v.Position.Z * scaleFactor);
                    if (preserveTextureDimension)
                        v.UvPosition = new(v.UvPosition.X, v.UvPosition.Y * scaleFactor);
                }    
                    
                scaledPolys.Add(transformedPoly);
            }

            return scaledPolys;
        }

        private static List<EditorPolygon> BendPolys(List<EditorPolygon> polygons, EditorTrackSection section, bool shouldBend)
        {
            if (!shouldBend || section is null || polygons is null || section.Traject.Radius == 0)
                return polygons;

            List<EditorPolygon> scaledPolys = new();

            foreach (var poly in polygons)
            {
                var transformedPoly = poly.Copy();

                foreach (var v in transformedPoly.Vertices)
                    v.Position = Transfigurations.BendPoint(v.Position, section.Traject, 1);

                if (section.Traject.Angle > 0)
                    transformedPoly.Flip();

                scaledPolys.Add(transformedPoly);
            }

            return scaledPolys;
        }

        private static List<EditorPolygon> TrimPolys(List<EditorPolygon> polygons, EditorTrackSection finalSection)
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
                        v.UvPosition = new Vector2(v.UvPosition.X, (float)(v.UvPosition.Y * maxZ / v.Position.Z));
                        v.Position = new Vector3(v.Position.X, v.Position.Y, (float)maxZ);
                    }
                }

                newPolygons.Add(newPoly);
            }

            return newPolygons;
        }

        public static void ShiftPart(EditorPart part, float dX, float dY, float dZ)
        {
            foreach (var v in part.Vertices())
                v.Position = new Vector3(v.Position.X + dX, v.Position.Y + dY, v.Position.Z + dZ);            
        }

        public static void FlipPart(EditorPart part, bool flipX, bool flipY, bool flipZ)
        {
            foreach (var v in part.Vertices())
            {
                var newX = flipX ? -v.Position.X : v.Position.X;
                var newY = flipY ? -v.Position.Y : v.Position.Y;
                var newZ = flipZ ? -v.Position.Z : v.Position.Z;
                
                v.Position = new Vector3(newX, newY, newZ);
            }

            var count = 0;
            if (flipX) count++;
            if (flipY) count++;
            if (flipZ) count++;

            if (count % 2 == 1)
                foreach (var poly in part.Polygons)
                    poly.Flip();
        }
    }
}
