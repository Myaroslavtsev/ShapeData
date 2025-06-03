using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

namespace ShapeData.Editor_shapes
{
    class PartTransformer
    {
        const double accuracy = 1e-5;

        public static EditorPart TransposePart(EditorPart part, Direction direction)
        {
            foreach(var poly in part.Polygons)
            {
                foreach(var v in poly.Vertices)
                    Geometry.Geometry.TransposePoint(v.Position, direction);
            }

            return part;
        }

        public static EditorPart BendPart(EditorPart part, Trajectory trajectory, IPartReplication replicationData)
        {
            var replicationParams = GetReplicationParamDictionary(replicationData);

            if (!replicationParams.ContainsKey("OriginalLength"))
                return part;

            double originalLength = replicationParams["OriginalLength"];

            double scaleFactor = originalLength / trajectory.Length;

            foreach (var poly in part.Polygons)
            {
                foreach (var v in poly.Vertices)
                    Geometry.Geometry.BendPoint(v.Position, trajectory, scaleFactor);
            }

            return part;
        }

        public static List<Direction> SplitSectionByFixedIntervals(EditorTrackSection section, IPartReplication replicationData) =>
            SplitSectionByIntervals(section, replicationData, false, replicationData.LeaveAtLeastOnePart, "Interval");    

        public static List<Direction> SplitSectionByEvenIntervals(EditorTrackSection section, IPartReplication replicationData) =>
            SplitSectionByIntervals(section, replicationData, true, replicationData.LeaveAtLeastOnePart, "MinInterval");

        public static List<Direction> SplitSectionByEvenArcs(EditorTrackSection section, IPartReplication replicationData) =>
            SplitSectionByIntervals(section, replicationData, true, replicationData.LeaveAtLeastOnePart, "MinLength");

        public static List<Direction> SplitSectionByEvenDeflection(
            EditorTrackSection section, 
            IPartReplication replicationData)
        {
            double interval = section.SectionTrajectory.Radius * 
                AngleIntervalByDeflection(section, replicationData) * Math.PI / 180;

            if (section.SectionTrajectory.Radius == 0)
                return new List<Direction> { section.StartDirection };
            else
                return SplitCurvedSection(section, interval, true, replicationData.LeaveAtLeastOnePart);
        }

        private static List<Direction> SplitSectionByIntervals(
            EditorTrackSection section, 
            IPartReplication replicationData, 
            bool arrangeEvenly,
            bool leaveAtLeastOneDirection,
            string replicationParamName)
        {
            var replicationParams = GetReplicationParamDictionary(replicationData);

            if (!replicationParams.ContainsKey(replicationParamName))
                return new List<Direction> { section.StartDirection };

            double interval = replicationParams[replicationParamName];

            if (section.SectionTrajectory.Radius == 0)
                return SplitStraightSection(section, interval, arrangeEvenly, leaveAtLeastOneDirection);               
            else
                return SplitCurvedSection(section, interval, arrangeEvenly, leaveAtLeastOneDirection);
        }

        public static Trajectory GetPartialTrajectoryByArc(EditorTrackSection section, IPartReplication replicationData)
        {
            var replicationParams = GetReplicationParamDictionary(replicationData);

            if (!replicationParams.ContainsKey("MinLength"))
                return section.SectionTrajectory;

            double interval = replicationParams["MinLength"];

            if (section.SectionTrajectory.Radius == 0)
                return new Trajectory(
                    StraightInterval(interval, section.SectionTrajectory.Straight, true), 
                    0, 0);
            else
                return new Trajectory(0, 
                    section.SectionTrajectory.Radius, 
                    AngleInterval(interval, section.SectionTrajectory.Radius, section.SectionTrajectory.Angle, true));
        }

        public static Trajectory GetPartialTrajectoryByDeflection(EditorTrackSection section, IPartReplication replicationData)
        {
            double angleInterval = AngleIntervalByDeflection(section, replicationData);
                
            return new Trajectory(0, section.SectionTrajectory.Radius, angleInterval);
        }

        private static double AngleIntervalByDeflection(EditorTrackSection section, IPartReplication replicationData)
        {
            if (section.SectionTrajectory.Radius == 0)
                return section.SectionTrajectory.Angle;

            var replicationParams = GetReplicationParamDictionary(replicationData);

            if (!replicationParams.ContainsKey("MaxDeflection"))
                return section.SectionTrajectory.Angle;

            double deflection = replicationParams["MaxDeflection"];

            double angleInterval = 2 * Math.Acos(1 - deflection / section.SectionTrajectory.Radius);

            return section.SectionTrajectory.Angle / Math.Floor(section.SectionTrajectory.Angle / angleInterval);
        }

        private static List<Direction> SplitStraightSection(
            EditorTrackSection section, 
            double interval, 
            bool arrangeEvenly,
            bool leaveAtLeastOneDirection)
        {
            var directions = new List<Direction>();
            var partialTrajectory = new Trajectory();

            interval = StraightInterval(interval, section.SectionTrajectory.Straight, arrangeEvenly);

            for (partialTrajectory.Straight = 0;
                 partialTrajectory.Straight < section.SectionTrajectory.Straight - accuracy;
                 partialTrajectory.Straight += interval)
                directions.Add(Geometry.Geometry.FindEndDirection(partialTrajectory, section.StartDirection));

            if (directions.Count == 0 && leaveAtLeastOneDirection )
                return new List<Direction> { section.StartDirection };

            return directions;
        }

        private static double StraightInterval(double interval, double straight, bool arrangeEvenly)
        {
            if (arrangeEvenly)   
                return straight / Math.Floor(straight / interval);

            return interval;
        }

        private static List<Direction> SplitCurvedSection(
            EditorTrackSection section, 
            double interval, 
            bool arrangeEvenly,
            bool leaveAtLeastOneDirection)
        {
            var directions = new List<Direction>();
            var partialTrajectory = new Trajectory
            {
                Radius = section.SectionTrajectory.Radius
            };            

            double angleInterval = AngleInterval(interval, section.SectionTrajectory.Radius, section.SectionTrajectory.Angle, arrangeEvenly);

            for (partialTrajectory.Angle = 0;
                 partialTrajectory.Angle < section.SectionTrajectory.Angle - accuracy;
                 partialTrajectory.Angle += angleInterval)
                directions.Add(Geometry.Geometry.FindEndDirection(partialTrajectory, section.StartDirection));

            if (directions.Count == 0 && leaveAtLeastOneDirection)
                return new List<Direction> { section.StartDirection };

            return directions;
        }

        private static double AngleInterval(double interval, double radius, double angle, bool arrangeEvenly)
        {
            double angleInterval = interval * 180 / Math.PI / radius;

            if (arrangeEvenly)
                angleInterval = angle / Math.Floor(angle / angleInterval);

            return angleInterval;
        }

        private static Dictionary<string, float> GetReplicationParamDictionary(IPartReplication ReplicationData)
        {
            var parameters = new Dictionary<string, float>();

            foreach (var (Name, Value) in ReplicationData.GetParams())            
                parameters.Add(Name, Value);            

            return parameters;
        }
    }
}
