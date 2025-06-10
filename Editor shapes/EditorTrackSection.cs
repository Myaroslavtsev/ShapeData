/// Data structure. Describes a line or curve in space with its start and finish vectors.
/// Also corresponds to a one track section (part of a track shape) in tsection.dat file.

using ShapeData.Geometry;

namespace ShapeData.Editor_shapes
{
    class EditorTrackSection
    {
        public Direction StartDirection { get; }
        public Direction EndDirection { get; }

        public Trajectory Traject { get; }

        public EditorTrackSection()
        {
            StartDirection = new Direction();
            EndDirection = new Direction();
            Traject = new Trajectory();
        }

        public EditorTrackSection(Direction startDirection, Trajectory trajectory)
        {
            Traject = trajectory;
            StartDirection = startDirection;
            EndDirection = Transfigurations.FindEndDirection(trajectory, startDirection);
        }
    }
}
