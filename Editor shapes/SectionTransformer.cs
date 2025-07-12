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
        public static (List<EditorTrackSection>, EditorTrackSection, float, float) 
            SplitTrackSectionInSubsections(EditorTrackSection section, PartReplication replicationData)
        {
            replicationData.GetReplicationParam("OriginalLength", out var originalLength);
            var partTraject = ChangeTrajectLength(section.Traject, originalLength);

            return replicationData.ReplicationMethod switch
            {
                PartReplicationMethod.ByFixedIntervals or PartReplicationMethod.ByEvenIntervals or PartReplicationMethod.ByDeflection =>
                    MakeSubsectionList(section, replicationData),

                PartReplicationMethod.AtFixedPos => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.StartDirection, partTraject)}, null, 1, 1),

                PartReplicationMethod.AtTheEnd => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.EndDirection, partTraject) }, null, 1, 1),

                _ => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.StartDirection, partTraject)}, null, 1, 1)
            };
        }

        private static (List<EditorTrackSection>, EditorTrackSection, float, float) MakeSubsectionList(
            EditorTrackSection section,
            PartReplication replicationData)
        {
            var allowOneSegmentStraights = replicationData.ReplicationMethod == PartReplicationMethod.ByDeflection;

            var replicationParams = ExtractReplicationParameters(replicationData);

            var subIntervals = GetSubIntervalCountAndLength(section, replicationData);

            var startDirection = ShiftStartDirection(section.Traject, section.StartDirection, replicationParams.InitialShift);

            var mainSections = GenerateMainSections(section.Traject, startDirection, subIntervals, replicationParams,
                allowOneSegmentStraights);

            var lastDirection = startDirection;
            if (mainSections.Count > 0)
                lastDirection = mainSections.Last().EndDirection;

            var lastSection = GenerateLastSection(section.Traject, lastDirection, subIntervals, replicationParams, 
                mainSections.Count, replicationData.ScalingMethod);

            if (mainSections.Count == 0 && lastSection is null && replicationData.LeaveAtLeastOne)
                lastSection = new EditorTrackSection(startDirection, 
                    ChangeTrajectLength(section.Traject, replicationParams.OriginalLength / replicationParams.SubdivisionNum));

            var scaleFactor = subIntervals.Length * replicationParams.SubdivisionNum / replicationParams.OriginalLength;
            var textureScale = 1f;
            if (allowOneSegmentStraights && section.Traject.Radius == 0)
            {
                

                //var subintervalScale = subIntervals.Length / (replicationParams.OriginalLength / replicationParams.SubdivisionNum);
                //scaleFactor = subintervalScale * subIntervals.Count / replicationParams.SubdivisionNum;
                
                //var grossintervalCount = (int)Math.Ceiling((float)subIntervals.Count / replicationParams.SubdivisionNum);
                //scaleFactor = grossintervalCount * subintervalScale;

                textureScale = (float)subIntervals.Count / replicationParams.SubdivisionNum;

                scaleFactor = (float)(section.Traject.Length / replicationParams.OriginalLength);
            }

            return (mainSections, lastSection, scaleFactor, textureScale);
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
            ReplicationParams replicationParams,
            bool allowOneSegmentStraights)
        {
            if (traject.Radius == 0 && allowOneSegmentStraights)
                return new List<EditorTrackSection>();

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
            replicationData.GetReplicationParam("OriginalLength", out var originalLength);

            var subintervalLength = originalLength / subdivisionNum;

            switch (replicationData.ReplicationMethod)
            {
                case PartReplicationMethod.ByFixedIntervals:
                    replicationData.GetReplicationParam("IntervalLength", out var interval);
                    var fixedIntervalLength = interval / subdivisionNum;
                    var fixedCount = CountSubintervals(sectionLength, fixedIntervalLength);
                    return (fixedCount, fixedIntervalLength);

                case PartReplicationMethod.ByEvenIntervals:
                    return StretchInterval(subintervalLength, sectionLength);
                //var stretchedInterval = 
                //var evenCount = CountSubintervals(sectionLength, stretchedInterval);
                //return (evenCount, stretchedInterval);

                case PartReplicationMethod.ByDeflection:
                    replicationData.GetReplicationParam("MaxDeflection", out var deflection);
                    var lengthByDeflection = LengthByDeflection(section.Traject, deflection);
                    var subLen = Math.Min(subintervalLength, lengthByDeflection);
                    return StretchInterval(subLen, sectionLength);
                    //var stretchedSubinterval = 
                    //var deflectionCount = CountSubintervals(sectionLength, stretchedSubinterval);
                    //return (deflectionCount, stretchedSubinterval);

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

        private static int CountSubintervals(double sectionLength, float intervalLength)
        {
            int count = (int)Math.Floor(sectionLength / intervalLength);

            return count;
        }

        private static (int, float) StretchInterval(double subintervalLength, double sectionLength)
        {
            var subIntervalCount = Math.Round(sectionLength / subintervalLength);
            return ((int)subIntervalCount, (float)(sectionLength / subIntervalCount));
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
