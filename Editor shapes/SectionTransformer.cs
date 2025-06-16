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
        const float accuracy = 1e-5f;

        public static (List<EditorTrackSection>, EditorTrackSection) SplitTrackSectionInSubsections(EditorTrackSection section, PartReplication replicationData)
        {
            return replicationData.ReplicationMethod switch
            {
                PartReplicationMethod.ByFixedIntervals or PartReplicationMethod.ByEvenIntervals or PartReplicationMethod.ByDeflection =>
                    MakeSubsectionList(GetIntervalSubsection(section, replicationData), section, replicationData),

                PartReplicationMethod.AtFixedPos => (new List<EditorTrackSection> { section }, null),

                PartReplicationMethod.AtTheEnd => (new List<EditorTrackSection> {
                        new EditorTrackSection(section.EndDirection, section.Traject) }, null),

                _ => (new List<EditorTrackSection> { section }, null)
            };
        }

        private static (List<EditorTrackSection>, EditorTrackSection) MakeSubsectionList(
            EditorTrackSection subsection, 
            EditorTrackSection fullSection, 
            PartReplication replicationData)
        {
            List<EditorTrackSection> subsections = new();
            EditorTrackSection finalSection = null;

            if (subsection.Traject.Straight == 0 && subsection.Traject.Angle == 0)
            {
                if (replicationData.ScalingMethod == PartScalingMethod.FixLengthAndCut)
                    finalSection = fullSection;

                return (subsections, finalSection);
            }

            var initialTraject = subsection.Traject;
            var startDirection = subsection.EndDirection;

            if (replicationData.GetReplicationParam("InitialShift", out float initialShift))
            {
                initialTraject = subsection.Traject * (1 + initialShift / subsection.Traject.Length);

                startDirection = Transfigurations.FindEndDirection(initialTraject, new Direction());
                startDirection = Transfigurations.FindEndDirection(subsection.Traject * -1, startDirection);
            }

            for (var partialTrajectory = initialTraject;
                partialTrajectory.Length < fullSection.Traject.Length - accuracy;
                partialTrajectory += subsection.Traject)
            {
                var newSubsection = new EditorTrackSection(startDirection, subsection.Traject);
                subsections.Add(newSubsection);
                startDirection = newSubsection.EndDirection;
            }

            if (subsections.Count == 0 && (replicationData.LeaveAtLeastOne || replicationData.ScalingMethod == PartScalingMethod.FixLengthAndCut))
                subsections.Add(subsection); // too much added when A1t10mstrt splitted by 9.5 parts

            if (replicationData.ScalingMethod == PartScalingMethod.FixLengthAndCut)
                finalSection = new EditorTrackSection(subsections.Last().EndDirection,
                    fullSection.Traject - subsection.Traject * subsections.Count);

            return (subsections, finalSection);
        }

        private static EditorTrackSection GetIntervalSubsection(EditorTrackSection section, PartReplication replicationData)
        {
            // valid only for Fixed, Even, Deflection replication methods

            float splitParameter = 0;
            EditorTrackSection result;

            if (replicationData.ReplicationMethod == PartReplicationMethod.ByDeflection)
            {
                if (!replicationData.GetReplicationParam("MaxDeflection", out splitParameter))
                    return section;
            }

            if (replicationData.ReplicationMethod == PartReplicationMethod.ByEvenIntervals ||
                replicationData.ReplicationMethod == PartReplicationMethod.ByFixedIntervals)
            {
                if (!replicationData.GetReplicationParam("MinLength", out splitParameter))
                    return section;
            }            

            if (section.Traject.Radius == 0)
                result = new EditorTrackSection(section.StartDirection,
                    new Trajectory(
                        GetStraightInterval(section, splitParameter, replicationData.ReplicationMethod), 0, 0));
            else
                result = new EditorTrackSection(section.StartDirection, 
                    new Trajectory(0, section.Traject.Radius, 
                        GetAngleInterval(section, splitParameter, replicationData.ReplicationMethod)));

            //if (replicationData.LeaveAtLeastOne && result.Traject.Angle == 0 && result.Traject.Length == 0)
            //    return section;

            return result;
        }

        private static double GetStraightInterval(EditorTrackSection section, float splitParameter, PartReplicationMethod method)
        {
            return method switch
            {
                PartReplicationMethod.ByFixedIntervals =>
                    StraightInterval(splitParameter, section.Traject.Straight, false),
                PartReplicationMethod.ByEvenIntervals =>
                    StraightInterval(splitParameter, section.Traject.Straight, true),
                PartReplicationMethod.ByDeflection =>
                    section.Traject.Straight,
                _ => 0,
            };
        }

        private static double GetAngleInterval(EditorTrackSection section, float splitParameter, PartReplicationMethod method)
        {
            return method switch
            {
                PartReplicationMethod.ByFixedIntervals => 
                    AngleInterval(splitParameter, section.Traject.Radius, section.Traject.Angle, false),
                PartReplicationMethod.ByEvenIntervals => 
                    AngleInterval(splitParameter, section.Traject.Radius, section.Traject.Angle, true),
                PartReplicationMethod.ByDeflection => 
                    AngleIntervalByDeflection(section.Traject.Radius, section.Traject.Angle, splitParameter),
                _ => 0,
            };
        }

        private static double StraightInterval(double interval, double straight, bool arrangeEvenly)
        {
            if (arrangeEvenly)
                return straight / Math.Floor(straight / interval);

            return interval;
        }

        private static double AngleIntervalByDeflection(double radius, double angle, double deflection)
        {
            double angleInterval = 2 * Math.Acos(1 - deflection / radius) * 180 / Math.PI;

            angleInterval = Math.Abs(angle) / Math.Floor(Math.Abs(angle / angleInterval));

            return angleInterval * Math.Sign(angle);
        }

        private static double AngleInterval(double interval, double radius, double angle, bool arrangeEvenly)
        {
            double angleInterval = interval * 180 / Math.PI / radius;

            if (arrangeEvenly)
                angleInterval = Math.Abs(angle) / Math.Floor(Math.Abs(angle / angleInterval));

            return angleInterval * Math.Sign(angle);
        }        
    }
}
