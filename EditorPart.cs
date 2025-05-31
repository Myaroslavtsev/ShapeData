using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Methods
        public EditorPart(
            string name,
            IPartReplication replicationParams,
            bool smoothed = false
            )
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

            if (polygons.Find(p => p.PolygonId == polygon.PolygonId) == null)
            {
                polygons.Add(polygon);
                return polygon;
            }
            else
                return null;
        }

        public bool DeletePolygon(uint polygonId) =>
            GeneralMethods.RemoveListItems(polygons, poly => poly.PolygonId == polygonId);

        public string SayIfSmoothed() => Smoothed ? "Smoothed" : "Unsmoothed";
    }
}