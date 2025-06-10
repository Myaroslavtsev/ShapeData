/// Data structure. A primary structure proposed to describe shapes in a simple human-readable format.
/// Subdivides by lods, parts, polygons and vertices.

using System.Collections.Generic;
using System.Linq;

namespace ShapeData
{
    public class EditorShape
    {
        // General properties
        public string ShapeName;
        public string ShapeComment;

        // Objects
        public List<EditorLod> Lods { get; private set; }


        public IEnumerable<EditorPart> Parts()
        {
            foreach (var lod in Lods)
                foreach (var part in lod.Parts)
                    yield return part;
        }

        public IEnumerable<EditorPolygon> Polygons()
        {
            foreach (var lod in Lods) 
                foreach (var polygon in lod.Polygons())
                    yield return polygon;
        }

        public IEnumerable<EditorVertex> Vertices()
        {
            foreach (var lod in Lods)
                foreach (var vertex in lod.Vertices())
                    yield return vertex;
        }

        // Methods
        public EditorShape(string shapeName)
        {
            ShapeName = shapeName;
            Lods = new List<EditorLod> { new EditorLod(2000) };
        }

        public EditorLod AddLod(EditorLod lod)
        {
            if (lod == null)
                return null;

            var sameDistanceLod = Lods.Find(l => l.Distance == lod.Distance);

            if (sameDistanceLod == null)
            {
                Lods.Add(lod);
                return lod;
            }
            else
                return sameDistanceLod;
        }

        public bool DeleteLod(int distance) =>
            GeneralMethods.RemoveListItems(Lods, lod => lod.Distance == distance);

        public void OrderLods()
        {
            Lods = Lods.OrderBy(lod => lod.Distance).ToList();
        }

        public void DeleteUnvalidPolys()
        {
            foreach (var part in Parts())
                for (int i = 0; i < part.Polygons.Count; i++)
                    if (part.Polygons[i].Vertices.Count < 3)
                        part.Polygons.RemoveAt(i);
        }
    }
}
