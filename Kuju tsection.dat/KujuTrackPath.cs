using ShapeData.Geometry;
using System.Collections.Generic;

namespace ShapeData
{
    class KujuTrackPath
    {
        // initial shift in meters and rotation in angles
        public Direction Direction { get; set; }

        // track section list
        public List<int> TrackSections => trackSections;

        private List<int> trackSections { get; }

        public KujuTrackPath()
        {
            trackSections = new List<int>();
            Direction = new Direction();
        }
    }
}
