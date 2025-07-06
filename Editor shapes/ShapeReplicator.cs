/// Replicates all parts in EditorShape instance.

using ShapeData.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ShapeData.Editor_shapes
{
    class ShapeReplicator
    {
        public static async Task<EditorShape> ReplicatePartsInShape(EditorShape editorShape, KujuTrackShape trackShape, KujuTsectionDat tsectionDat)
        {
            var newShape = new EditorShape(trackShape.FileName);

            newShape.ShapeComment = editorShape.ShapeComment;

            foreach (var oldLod in editorShape.Lods)
            {
                var newLod = newShape.AddLod(new EditorLod(oldLod.Distance));

                ReplicatePartsInLod(oldLod, newLod, trackShape, tsectionDat);
            }
            return newShape;
        }

        private static void ReplicatePartsInLod(EditorLod oldLod, EditorLod newLod, KujuTrackShape trackShape, KujuTsectionDat tsectionDat)
        {
            foreach (var part in oldLod.Parts)
            {
                var replicatedParts = ReplicatePart(part, GetSectionsFromShape(trackShape, tsectionDat));
                int counter = 0;

                foreach (var replica in replicatedParts)
                    if (replica != null)
                    {
                        replica.PartName += '_' + counter;
                        newLod.AddPart(replica);
                        counter++;
                    }
            }
        }

        private static List<EditorPart> ReplicatePart(EditorPart part, List<EditorTrackSection> trackSections)
        {
            if (part.Replication.ReplicationMethod == PartReplicationMethod.NoReplication)
                return new List<EditorPart> { part.Copy(true) };

            var replicatedParts = new List<EditorPart>();

            CheckPartReplicationParams(part);

            if (part.Replication.StretchInWidthMethod != PartStretchInWidthMethod.ReplicateAlongAllTracks)
                throw new NotImplementedException(); // ReplicateStretchedPart(part, trackSections);
            
            foreach (var section in trackSections)
            {
                var newSections = SectionTransformer.SplitTrackSectionInSubsections(section, part.Replication);                

                var segments = PartTransformer.MakeTypicalAndFinalSegments(part, newSections);                

                replicatedParts.Add(PartTransformer.AssemblePartSegments(part, newSections, segments));
            }

            return replicatedParts;
        }

        private static void CheckPartReplicationParams(EditorPart part)
        {
            if (part.Replication.ReplicationParams["OriginalLength"] == 0)
                part.Replication.ReplicationParams["OriginalLength"] = 1;
        }

        private static List<EditorTrackSection> GetSectionsFromShape(KujuTrackShape trackShape, KujuTsectionDat tsectionDat)
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
    }
}
