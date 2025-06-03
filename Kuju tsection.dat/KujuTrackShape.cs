using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class KujuTrackShape
    {
        public string FileName { get; private set; }
        public bool RoadShape { get; set; }
        public List<KujuTrackPath> Paths => paths;

        private List<KujuTrackPath> paths { get; }

        public KujuTrackShape(string fileName = "test.s" )
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
