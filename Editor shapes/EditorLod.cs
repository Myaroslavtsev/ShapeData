using System.Collections.Generic;

namespace ShapeData
{
    public class EditorLod
    {

        public int Distance;

        private readonly List<EditorPart> parts;

        public List<EditorPart> Parts => parts;

        public IEnumerable<EditorPolygon> Polygons()
        {
            foreach (var part in parts)
                foreach (var poly in part.Polygons)
                    yield return poly;
        }

        public IEnumerable<EditorVertex> Vertices()
        {
            foreach (var part in parts)
                foreach (var vertex in part.Vertices())
                    yield return vertex;
        }

        public List<(int pointId, int normalId, int uvPointId)> KujuVerticeList { get; set; }

        public EditorLod(int distance)
        {
            Distance = distance;
            parts = new List<EditorPart>();
        }

        public EditorPart AddPart(EditorPart part)
        {
            if (part == null)
                return null;

            if (parts.Find(p => p.PartName == part.PartName) == null)
            {
                parts.Add(part);
                return part;
            }

            return null;
        }

        public int TriangleCount()
        {
            int count = 0;

            foreach (var poly in Polygons())
                if (poly.Vertices.Count >= 3)
                    count += poly.Vertices.Count - 2;

            return count;
        }

        public HashSet<int> GetPrimStateIdSet()
        {
            var primStateIds = new HashSet<int>();

            foreach (var poly in Polygons())
                primStateIds.Add(poly.KujuPrimStateId);

            return primStateIds;
        }

        public bool DeletePart(string name) =>
            GeneralMethods.RemoveListItems(parts, p => p.PartName == name);
    }
}
