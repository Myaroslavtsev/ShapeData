using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class KujuTsectionDat
    {
        public Dictionary<int, KujuTrackSection> TrackSections => trackSections;
        public List<KujuTrackShape> TrackShapes => trackShapes;

        private Dictionary<int, KujuTrackSection> trackSections { get; }
        private List<KujuTrackShape> trackShapes { get; }

        public KujuTsectionDat()
        {
            trackSections = new Dictionary<int, KujuTrackSection>();
            trackShapes = new List<KujuTrackShape>();
        }
    }
}
