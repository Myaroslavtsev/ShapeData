/// Data structure describing trajectories of multiple tracks modelled by a one .s file

using System.Collections.Generic;

namespace ShapeData
{
    public class KujuTrackShape
    {
        public string FileName { get; private set; }
        public bool RoadShape { get; set; }
        public List<KujuTrackPath> Paths => paths;

        private List<KujuTrackPath> paths { get; }

        public KujuTrackShape(string fileName = "test.s")
        {
            FileName = fileName;

            paths = new List<KujuTrackPath>();
        }

        public void Rename(string fileName)
        {
            FileName = fileName;
        }
    }
}
