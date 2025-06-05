using ShapeData.Geometry;

namespace ShapeData
{
    class KujuTrackSection
    {
        public int Id { get; private set; }
        public double Gauge { get; set; }
        public Trajectory SectionTrajectory { get; set; }

        public KujuTrackSection(int id, double gauge = 1.5, double straight = 0, double radius = 0, double angle = 0)
        {
            SectionTrajectory = new Trajectory();

            Id = id;
            Gauge = gauge;
            SectionTrajectory.Straight = straight;
            SectionTrajectory.Radius = radius;
            SectionTrajectory.Angle = angle;
        }
    }
}
