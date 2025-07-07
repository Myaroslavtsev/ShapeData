/// Prepares EditorTrackSection data for replication by ShapeReplicator

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

namespace ShapeData.Editor_shapes
{
    struct ReplicationParams
    {
        public int SubdivisionNum;
        public float OriginalLength;
        public float InitialShift;

        public ReplicationParams (int subdivisionNum, float originalLength, float initialShift)
        {
            SubdivisionNum = subdivisionNum;
            OriginalLength = originalLength;
            InitialShift = initialShift;
        }
    }

    class SectionTransformer
    {
        public static (List<EditorTrackSection>, EditorTrackSection, float) SplitTrackSectionInSubsections(EditorTrackSection section, PartReplication replicationData)
        {
            var partTraject = ChangeTrajectLength(section.Traject, replicationData.ReplicationParams["OriginalLength"]);

            return replicationData.ReplicationMethod switch
            {
                PartReplicationMethod.ByFixedIntervals or PartReplicationMethod.ByEvenIntervals or PartReplicationMethod.ByDeflection =>
                    MakeSubsectionList4(section, replicationData),

                PartReplicationMethod.AtFixedPos => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.StartDirection, partTraject)}, null, 1),

                PartReplicationMethod.AtTheEnd => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.EndDirection, partTraject) }, null, 1),

                _ => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.StartDirection, partTraject)}, null, 1)
            };
        }

        private static (List<EditorTrackSection>, EditorTrackSection, float) MakeSubsectionList4(
            EditorTrackSection section,
            PartReplication replicationData)
        {
            var replicationParams = ExtractReplicationParameters(replicationData);

            var subIntervals = GetSubIntervalCountAndLength(section, replicationData);

            var startDirection = ShiftStartDirection(section.Traject, section.StartDirection, replicationParams.InitialShift);

            var mainSections = GenerateMainSections(section.Traject, startDirection, subIntervals, replicationParams);

            var lastDirection = startDirection;
            if (mainSections.Count > 0)
                lastDirection = mainSections.Last().EndDirection;

            var lastSection = GenerateLastSection(section.Traject, lastDirection, subIntervals, replicationParams, 
                mainSections.Count, replicationData.ScalingMethod);

            if (mainSections.Count == 0 && lastSection is null && replicationData.LeaveAtLeastOne)
                lastSection = new EditorTrackSection(startDirection, 
                    ChangeTrajectLength(section.Traject, replicationParams.OriginalLength / replicationParams.SubdivisionNum));

            var scaleFactor = subIntervals.Length * replicationParams.SubdivisionNum / replicationParams.OriginalLength;

            return (mainSections, lastSection, scaleFactor);
        }

        private static EditorTrackSection GenerateLastSection(
            Trajectory traject,
            Direction lastDirection,
            (int Count, float Length) subIntervals,
            ReplicationParams replicationParams,
            int mainSectionsCount,
            PartScalingMethod scalingMethod)
        {
            var lastSectionLength = traject.Length -
                subIntervals.Length * replicationParams.SubdivisionNum * mainSectionsCount;

            if (scalingMethod == PartScalingMethod.FixLength)
            {
                var subIntervalOriginalLength = replicationParams.OriginalLength / replicationParams.SubdivisionNum;
                lastSectionLength = Math.Floor(lastSectionLength / subIntervalOriginalLength) * subIntervalOriginalLength;
            }

            var lastTraject = ChangeTrajectLength(traject, lastSectionLength);

            EditorTrackSection lastSection = null;
            if (lastTraject.Length > 0)
                lastSection = new EditorTrackSection(lastDirection, lastTraject);

            return lastSection;
        }

        private static List<EditorTrackSection> GenerateMainSections(
            Trajectory traject,
            Direction startDirection,
            (int Count, float Length) subIntervals,
            ReplicationParams replicationParams)
        {
            var mainTraject = ChangeTrajectLength(traject, subIntervals.Length * replicationParams.SubdivisionNum);

            var mainSectionCount = subIntervals.Count / replicationParams.SubdivisionNum; // integer division is floored automatically

            return GenerateSectionList(startDirection, mainTraject, mainSectionCount);
        }

        private static ReplicationParams ExtractReplicationParameters(PartReplication replicationData)
        {
            replicationData.GetReplicationParam("InitialShift", out var initialShift);
            replicationData.GetReplicationParam("OriginalLength", out var originalLength);

            var subdivisionNum = GetSubdivisionNum(replicationData);

            return new ReplicationParams(subdivisionNum, originalLength, initialShift);
        }

        private static List<EditorTrackSection> GenerateSectionList(
            Direction startDirection, Trajectory trajectory, int sectionCount)
        {
            var sections = new List<EditorTrackSection>();

            for(int n = 0; n < sectionCount; n++)
            {
                var newSection = new EditorTrackSection(startDirection, trajectory);
                sections.Add(newSection);
                startDirection = newSection.EndDirection;
            }

            return sections;
        }

        private static Direction ShiftStartDirection(Trajectory traject, Direction initDir, float shift)
        {
            if (traject.Radius == 0)
                return new Direction(initDir.X, initDir.Y, initDir.Z + shift, initDir.A);

            var angle = Math.Sign(traject.Angle) * Transfigurations.Rad2Deg(Math.Abs(shift) / traject.Radius);

            if (shift > 0) // forward shift
                return Transfigurations.FindEndDirection(new Trajectory(0, traject.Radius, angle), initDir);

            // backward shift
            var reversedDir = Transfigurations.FindEndDirection(new Trajectory(0, traject.Radius, -angle),
                new Direction(initDir.X, initDir.Y, initDir.Z, initDir.A - 180));
            return new Direction(reversedDir.X, reversedDir.Y, reversedDir.Z, reversedDir.A - 180);
        }

        private static Trajectory ChangeTrajectLength(Trajectory originalTraject, double newLength)
        {
            if (originalTraject.Radius == 0)
                return new Trajectory(newLength, 0, 0);

            var angle = Transfigurations.Rad2Deg(newLength / originalTraject.Radius) * Math.Sign(originalTraject.Angle);

            return new Trajectory(0, originalTraject.Radius, angle);
        }        

        private static (int Count, float Length) GetSubIntervalCountAndLength(EditorTrackSection section,
            PartReplication replicationData)
        {            
            var sectionLength = section.Traject.Length;

            var subdivisionNum = GetSubdivisionNum(replicationData);

            switch (replicationData.ReplicationMethod)
            {
                case PartReplicationMethod.ByFixedIntervals:
                    replicationData.GetReplicationParam("IntervalLength", out var interval);
                    var fixedIntervalLength = interval / subdivisionNum;
                    var fixedCount = CountSubintervals((float)sectionLength, fixedIntervalLength, 
                        replicationData.LeaveAtLeastOne);
                    return (fixedCount, fixedIntervalLength);

                case PartReplicationMethod.ByEvenIntervals:
                    replicationData.GetReplicationParam("IntervalLength", out var intervalLength);
                    var stretchedInterval = StretchInterval(intervalLength, sectionLength, subdivisionNum);
                    var evenCount = CountSubintervals((float)sectionLength, stretchedInterval,
                        replicationData.LeaveAtLeastOne);
                    return (evenCount, stretchedInterval);

                case PartReplicationMethod.ByDeflection:
                    replicationData.GetReplicationParam("MaxDeflection", out var deflection);
                    var deflectionCount = CountSubintervals((float)sectionLength,
                        StretchInterval(LengthByDeflection(section.Traject, deflection), sectionLength, subdivisionNum),
                        replicationData.LeaveAtLeastOne);
                    return (deflectionCount, (float)(sectionLength / deflectionCount));

                default:
                    return (0, 0);
            }
        }

        private static int GetSubdivisionNum(PartReplication replicationData)
        {
            replicationData.GetReplicationParam("SubdivisionCount", out var subdivisionCount);
            
            var subdivisionNum = (int)Math.Round(subdivisionCount);

            if (subdivisionNum < 1)
                subdivisionNum = 1;

            return subdivisionNum;
        }

        private static int CountSubintervals(float sectionLength, float intervalLength, bool leaveAtLeastOne)
        {
            int count = (int)Math.Floor(sectionLength / intervalLength);

            return count;
        }

        private static float StretchInterval(double intervalLength, double sectionLength, int subdivisionCount)
        {
            var desiredSunibtervalLength = intervalLength / subdivisionCount;
            var subIntervalCount = Math.Round(sectionLength / desiredSunibtervalLength);
            return (float)(sectionLength / subIntervalCount);
        }

        private static double LengthByDeflection(Trajectory trajectory, float deflection)
        {
            if (trajectory.Radius == 0)
                return trajectory.Length;

            double angleInterval = 2 * Math.Acos(1 - Math.Abs(deflection / trajectory.Radius));

            return Math.Min(angleInterval * trajectory.Radius, trajectory.Length);
        }      
    }
}
