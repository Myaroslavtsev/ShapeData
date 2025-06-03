using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

namespace ShapeData.Editor_shapes
{
    class EditorTrackSection
    {
        public Direction StartDirection { get; }
        public Direction EndDirection { get; }

        public Trajectory SectionTrajectory { get; }

        public EditorTrackSection()
        {           
            StartDirection = new Direction();
            EndDirection = new Direction();
            SectionTrajectory = new Trajectory();
        }

        public EditorTrackSection(Direction startDirection, Trajectory trajectory)
        {
            SectionTrajectory = trajectory;
            StartDirection = startDirection;
            EndDirection = Geometry.Geometry.FindEndDirection(trajectory, startDirection);            
        }
    }
}
