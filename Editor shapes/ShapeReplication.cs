using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

namespace ShapeData.Editor_shapes
{
    class ShapeReplication
    {
        public static async Task<EditorShape> ReplicatePartsInShape(EditorShape editorShape, KujuTrackShape trackShape, KujuTsectionDat tsectionDat)
        {
            var newShape = new EditorShape(editorShape.ShapeName);

            newShape.ShapeComment = editorShape.ShapeComment;

            foreach(var lod in editorShape.Lods)
            {
                var newLod = newShape.AddLod(new EditorLod(lod.Distance));
                
                foreach(var part in lod.Parts)
                {
                    var replicatedParts = ReplicatePart(part, GetSectionsFromShape(trackShape, tsectionDat));

                    foreach (var replica in replicatedParts)
                        if (replica != null)
                            newLod.AddPart(replica);
                }
            }

            return newShape;
        }

        // private
        public static List<EditorTrackSection> GetSectionsFromShape(KujuTrackShape trackShape, KujuTsectionDat tsectionDat)
        {
            var extractedSections = new List<EditorTrackSection>();

            foreach (var path in trackShape.Paths)
            {
                var simplifiedTrajectories = ExtractAndSimplifyTrajectories(path.TrackSections, tsectionDat);

                var extractedTrackSection = new EditorTrackSection(path.Direction, simplifiedTrajectories[0]);
                extractedSections.Add(extractedTrackSection);

                for (int i = 1; i < simplifiedTrajectories.Count; i++)
                {
                    extractedTrackSection = new EditorTrackSection(extractedTrackSection.EndDirection, simplifiedTrajectories[i]);
                    extractedSections.Add(extractedTrackSection);
                }
            }

            return extractedSections;
        }

        private static List<Trajectory> ExtractAndSimplifyTrajectories(List<int> initialSections, KujuTsectionDat tsectionDat)
        {
            var trajectories = new List<Trajectory>();

            foreach (var id in initialSections)
                trajectories.Add(tsectionDat.TrackSections[id].SectionTrajectory);

            return SimplifyTrajectories(trajectories);
        }

        private static List<Trajectory> SimplifyTrajectories(List<Trajectory> trajectories)
        {
            if (trajectories.Count <= 1)
                return trajectories;

            var simplifiedTrajectories = new List<Trajectory>();

            var currentRadius = trajectories[0].Radius;
            var currentAngleSum = currentRadius == 0 ? trajectories[0].Straight : trajectories[0].Angle;

            for (int i = 1; i < trajectories.Count; i++)
            {
                if (trajectories[i].Radius == currentRadius)
                {
                    currentAngleSum += currentRadius == 0 ? trajectories[i].Straight : trajectories[i].Angle;
                }
                else
                {
                    if (currentRadius == 0)
                        simplifiedTrajectories.Add(new Trajectory(currentAngleSum, 0, 0));
                    else
                        simplifiedTrajectories.Add(new Trajectory(0, currentRadius, currentAngleSum));

                    currentRadius = trajectories[i].Radius;
                    currentAngleSum = currentRadius == 0 ? trajectories[i].Straight : trajectories[i].Angle;
                }
            }

            if (currentRadius == 0)
                simplifiedTrajectories.Add(new Trajectory(currentAngleSum, 0, 0));
            else
                simplifiedTrajectories.Add(new Trajectory(0, currentRadius, currentAngleSum));

            return simplifiedTrajectories;
        }

        private static List<EditorPart> ReplicatePart(EditorPart part, List<EditorTrackSection> sections)
        {
            switch (part.ReplicationParams.ReplicationMethod)
            {
                case PartReplicationMethod.AtFixedPos:
                    return ReplicateAtFixedPos(part, sections);

                case PartReplicationMethod.AtTheEnd:
                    return ReplicateAtTheEnd(part, sections);

                case PartReplicationMethod.ByFixedIntervals:
                    return ReplicateByFixedIntervals(part, sections);

                case PartReplicationMethod.ByEvenIntervals:
                    return ReplicateByEvenIntervals(part, sections);

                case PartReplicationMethod.StretchedByArc:
                    return ReplicateStretchedByArc(part, sections);

                case PartReplicationMethod.StretchedByDeflection:
                    return ReplicateStretchedByDeflection(part, sections);
            }

            return null;
        }

        private static List<EditorPart> ReplicateAtFixedPos(EditorPart part, List<EditorTrackSection> sections)
        {
            var parts = new List<EditorPart>();

            foreach(var section in sections)
            {
                parts.Add(PartTransformer.TransposePart(part.Copy(true), section.StartDirection)); 
            }

            return parts;
        }

        private static List<EditorPart> ReplicateAtTheEnd(EditorPart part, List<EditorTrackSection> sections)
        {
            var parts = new List<EditorPart>();

            foreach (var section in sections)
            {
                parts.Add(PartTransformer.TransposePart(part.Copy(true), section.EndDirection));
            }

            return parts;
        }

        private static List<EditorPart> ReplicateByFixedIntervals(EditorPart part, List<EditorTrackSection> sections)
        {
            var parts = new List<EditorPart>();

            foreach (var section in sections)
            {
                foreach (var direction in PartTransformer.SplitSectionByFixedIntervals(section, part.ReplicationParams))
                    parts.Add(PartTransformer.TransposePart(part.Copy(true), direction));
            }

            return parts;
        }

        private static List<EditorPart> ReplicateByEvenIntervals(EditorPart part, List<EditorTrackSection> sections)
        {
            var parts = new List<EditorPart>();

            foreach (var section in sections)
            {
                foreach (var direction in PartTransformer.SplitSectionByEvenIntervals(section, part.ReplicationParams))
                    parts.Add(PartTransformer.TransposePart(part.Copy(true), direction));
            }

            return parts;
        }

        private static List<EditorPart> ReplicateStretchedByArc(EditorPart part, List<EditorTrackSection> sections)
        {
            var parts = new List<EditorPart>();

            foreach (var section in sections)
            {
                var partialTrajectory = PartTransformer.GetPartialTrajectoryByArc(section, part.ReplicationParams);

                var segment = PartTransformer.BendPart(part.Copy(true), partialTrajectory, part.ReplicationParams); 

                foreach (var direction in PartTransformer.SplitSectionByEvenArcs(section, part.ReplicationParams))
                    parts.Add(PartTransformer.TransposePart(segment.Copy(true), direction));
            }

            return parts;
        }
        
        private static List<EditorPart> ReplicateStretchedByDeflection(EditorPart part, List<EditorTrackSection> sections)
        {
            var parts = new List<EditorPart>();

            foreach (var section in sections)
            {
                var partialTrajectory = PartTransformer.GetPartialTrajectoryByDeflection(section, part.ReplicationParams); 

                var segment = PartTransformer.BendPart(part.Copy(true), partialTrajectory, part.ReplicationParams);

                foreach (var direction in PartTransformer.SplitSectionByEvenDeflection(section, part.ReplicationParams))
                    parts.Add(PartTransformer.TransposePart(segment.Copy(true), direction));
            }

            return parts;
        }
    }
}
