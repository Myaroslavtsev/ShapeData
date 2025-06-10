/// Data structure containing all data in Kuju tsection.dat file about track sections and track shapes.

using System.Collections.Generic;

namespace ShapeData
{
    class KujuTsectionDat
    {
        public Dictionary<int, KujuTrackSection> TrackSections => trackSections;
        public Dictionary<string, KujuTrackShape> TrackShapes => trackShapes;

        private Dictionary<int, KujuTrackSection> trackSections { get; }
        private Dictionary<string, KujuTrackShape> trackShapes { get; }

        public KujuTsectionDat()
        {
            trackSections = new Dictionary<int, KujuTrackSection>();
            trackShapes = new Dictionary<string, KujuTrackShape>();
        }
    }
}
