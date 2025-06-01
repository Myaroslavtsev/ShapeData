using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class KujuTrackSection
    {
        public int Id { get; private set; }
        public double Gauge { get; set; }
        public double Straight { get; set; }
        public double Radius { get; set; }
        public double Angle { get; set; }

        public KujuTrackSection(int id, double gauge = 1.5, double straight = 0, double radius = 0, double angle = 0)
        {
            Id = id;
            Gauge = gauge;
            Straight = straight;
            Radius = radius;
            Angle = angle;
        }
    }
}
