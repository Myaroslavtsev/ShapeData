/// Data structure, a part of a kuju TrackShape. Contains track sections, one starting at other's end, that makes them drivable.
/// A multi-track track shape can contain many track paths consisting of many sections each.

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
