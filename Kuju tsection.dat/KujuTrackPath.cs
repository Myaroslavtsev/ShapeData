using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class KujuTrackPath
    {
        // initial shift in meters and rotation in angles
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double A { get; set; }
        
        // track section list
        public List<int> TrackSections => trackSections;
        
        private List<int> trackSections { get; }

        public KujuTrackPath()
        {
            trackSections = new List<int>();
        }
    }
}
