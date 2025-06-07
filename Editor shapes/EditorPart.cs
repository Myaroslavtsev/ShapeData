using System.Collections.Generic;

namespace ShapeData
{
    public class EditorPart
    {
        // General properties
        public string PartName { get; set; }

        public bool Smoothed { get; set; }

        public IPartReplication ReplicationParams { get; private set; }

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
            IPartReplication replicationParams,
            bool smoothed = false)
        {
            PartName = name;
            Smoothed = smoothed;
            ReplicationParams = replicationParams;
            polygons = new List<EditorPolygon>();
        }

        public EditorPolygon AddPolygon(EditorPolygon polygon)
        {
            if (polygon == null)
                return null;

            polygons.Add(polygon);
                return polygon;
        }

        public EditorPart Copy(bool setFixedPosReplication)
        {
            var copy = setFixedPosReplication ?
                new EditorPart(PartName, new ReplicationAtFixedPos(), Smoothed) :
                new EditorPart(PartName, ReplicationParams, Smoothed);

            foreach (var polygon in polygons)
            {
                copy.AddPolygon(polygon.Copy());
            }

            return copy;
        }

        public string IsSmoothed() => Smoothed ? "Smoothed" : "Unsmoothed";
        public string LeaveAtLeastOne() => ReplicationParams.LeaveAtLeastOnePart ? "Leave" : "NotLeave";
    }
}