using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeData.Geometry;

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
