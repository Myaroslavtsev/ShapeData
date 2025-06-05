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

        public bool DeletePart(string name) =>
            GeneralMethods.RemoveListItems(parts, p => p.PartName == name);
    }
}
