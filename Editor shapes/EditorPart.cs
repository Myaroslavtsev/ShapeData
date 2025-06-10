/// Data structure. Describes parts, stored into EditorLod class. Parts can be replicated by different ways. 
/// Note that final .s file contains only polygons withous being partitioned by parts.

using System.Collections.Generic;

namespace ShapeData
{
    public class EditorPart
    {
        // General properties
        public string PartName { get; set; }

        public bool Smoothed { get; set; }

        public PartReplication Replication { get; private set; }

        // Polygon list
        private readonly List<EditorPolygon> polygons;

        public List<EditorPolygon> Polygons => polygons;

        public IEnumerable<EditorVertex> Vertices()
        {
            foreach (var polygon in polygons)
                foreach (var vertex in polygon.Vertices)
                    yield return vertex;
        }

        // Methods
        public EditorPart(
            string name,
            PartReplication replication,
            bool smoothed = false)
        {
            PartName = name;
            Smoothed = smoothed;
            Replication = replication;
            polygons = new List<EditorPolygon>();
        }

        public EditorPolygon AddPolygon(EditorPolygon polygon)
        {
            if (polygon == null)
                return null;

            polygons.Add(polygon);
                return polygon;
        }

        public EditorPart Copy(bool clearReplicationParams)
        {
            var copy = clearReplicationParams ?
                new EditorPart(PartName, PartReplication.NoReplication(), Smoothed) :
                new EditorPart(PartName, Replication, Smoothed);

            foreach (var polygon in polygons)
            {
                copy.AddPolygon(polygon.Copy());
            }

            return copy;
        }

        //public string IsSmoothed() => Smoothed ? "Smoothed" : "Unsmoothed";
    }
}