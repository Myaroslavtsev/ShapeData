/// Data structure containing all data in Kuju tsection.dat file about track sections and track shapes.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ShapeData
{
    public class KujuTsectionDat
    {
        public SortedDictionary<int, KujuTrackSection> TrackSections => trackSections;
        public SortedDictionary<string, KujuTrackShape> TrackShapes => trackShapes;

        private SortedDictionary<int, KujuTrackSection> trackSections { get; }
        private SortedDictionary<string, KujuTrackShape> trackShapes { get; }

        public KujuTsectionDat(SortedDictionary<int, KujuTrackSection> sections, SortedDictionary<string, KujuTrackShape> shapes)
        {
            trackSections = sections;
            trackShapes = shapes;
        }

        public KujuTsectionDat()
        {
            trackSections = new SortedDictionary<int, KujuTrackSection>();
            trackShapes = new SortedDictionary<string, KujuTrackShape>();
        }
    }
}
