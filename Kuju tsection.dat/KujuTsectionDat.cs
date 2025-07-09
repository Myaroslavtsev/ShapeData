/// Data structure containing all data in Kuju tsection.dat file about track sections and track shapes.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ShapeData
{
    public class KujuTsectionDat
    {
        public ConcurrentDictionary<int, KujuTrackSection> TrackSections => trackSections;
        public ConcurrentDictionary<string, KujuTrackShape> TrackShapes => trackShapes;

        private ConcurrentDictionary<int, KujuTrackSection> trackSections { get; }
        private ConcurrentDictionary<string, KujuTrackShape> trackShapes { get; }

        public KujuTsectionDat()
        {
            trackSections = new ConcurrentDictionary<int, KujuTrackSection>();
            trackShapes = new ConcurrentDictionary<string, KujuTrackShape>();
        }
    }
}
