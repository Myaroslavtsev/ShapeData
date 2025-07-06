/// Prepares EditorTrackSection data for replication by ShapeReplicator

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

namespace ShapeData.Editor_shapes
{
    class SectionTransformer
    {
        public static (List<EditorTrackSection>, EditorTrackSection) SplitTrackSectionInSubsections(EditorTrackSection section, PartReplication replicationData)
        {
            var partTraject = ChangeTrajectLength(section.Traject, replicationData.ReplicationParams["OriginalLength"]);

            return replicationData.ReplicationMethod switch
            {
                PartReplicationMethod.ByFixedIntervals or PartReplicationMethod.ByEvenIntervals or PartReplicationMethod.ByDeflection =>
                    MakeSubsectionList2(section, replicationData),

                PartReplicationMethod.AtFixedPos => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.StartDirection, partTraject)}, null),

                PartReplicationMethod.AtTheEnd => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.EndDirection, partTraject) }, null),

                _ => (new List<EditorTrackSection> {
                    new EditorTrackSection(section.StartDirection, partTraject)}, null)
            };
        }

        private static (List<EditorTrackSection>, EditorTrackSection) MakeSubsectionList2(
            EditorTrackSection section, 
            PartReplication replicationData)
        {
            // valid only for Fixed, Even, Deflection replication methods

            replicationData.GetReplicationParam("InitialShift", out var initialShift);
            replicationData.GetReplicationParam("SubdivisionCount", out var subdivisionCount);

            int subdivisionNum = (int)Math.Round(subdivisionCount);
            if (subdivisionNum < 1)
                subdivisionNum = 1;

            var subIntervals = GetSubIntervalCountAndLength(section, replicationData);            

            var mainTraject = ChangeTrajectLength(section.Traject, subIntervals.Length * subdivisionNum);
            var lastTraject = ChangeTrajectLength(section.Traject, subIntervals.Length * (subIntervals.Count % subdivisionNum));

            var startDirection = ShiftStartDirection(section.Traject, section.StartDirection, initialShift);

            var mainSectionCount = subIntervals.Count / subdivisionNum; // integer division is floored automatically
            var mainSections = GenerateSectionList(startDirection, mainTraject, mainSectionCount);

            EditorTrackSection lastSection = null;
            if (mainSectionCount > 0)
                if (lastTraject.Length > 0)
                    lastSection = new EditorTrackSection(mainSections.Last().EndDirection, lastTraject);
            else
                if (lastTraject.Length > 0)
                    lastSection = new EditorTrackSection(startDirection, lastTraject);

            return (mainSections, lastSection);
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

            var angle = Transfigurations.Rad2Deg(shift / traject.Radius);

            return Transfigurations.FindEndDirection(new Trajectory(0, traject.Radius, angle), initDir);
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

            replicationData.GetReplicationParam("SubdivisionCount", out var subdivisionCount);
            int subdivisionNum = (int)Math.Round(subdivisionCount);
            if (subdivisionNum < 1)
                subdivisionNum = 1;

            switch (replicationData.ReplicationMethod)
            {
                case PartReplicationMethod.ByFixedIntervals:
                    replicationData.GetReplicationParam("IntervalLength", out var interval);                    
                    return CalculateSubintervals((float)sectionLength, interval / subdivisionNum, 
                        replicationData.LeaveAtLeastOne);

                case PartReplicationMethod.ByEvenIntervals:
                    replicationData.GetReplicationParam("IntervalLength", out var intervalLength);
                    return CalculateSubintervals(
                        StretchInterval(intervalLength, sectionLength, subdivisionNum), 
                        (float)sectionLength, replicationData.LeaveAtLeastOne);

                case PartReplicationMethod.ByDeflection:
                    replicationData.GetReplicationParam("MaxDeflection", out var deflection);
                    return CalculateSubintervals(
                        StretchInterval(LengthByDeflection(section.Traject, deflection), sectionLength, subdivisionNum),
                        (float)sectionLength, replicationData.LeaveAtLeastOne);

                default:
                    return (0, 0);
            }
        }

        private static (int Count, float Length) CalculateSubintervals(float sectionLength, float intervalLength, bool leaveAtLeastOne)
        {
            int count = (int)Math.Floor(sectionLength / intervalLength);

            if (leaveAtLeastOne && count == 0)
                count = 1;

            return (count, intervalLength);
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

            double angleInterval = 2 * Math.Acos(1 - deflection / trajectory.Radius) * 180 / Math.PI;

            return angleInterval * trajectory.Radius;
        }      
    }
}
