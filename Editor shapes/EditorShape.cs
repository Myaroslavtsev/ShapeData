using System.Collections.Generic;

namespace ShapeData
{
    public class EditorShape
    {
        // General properties
        public string ShapeName;
        public string ShapeComment;

        // Objects
        private readonly List<EditorLod> lods;

        public List<EditorLod> Lods => lods;

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
            lods = new List<EditorLod> { new EditorLod(2000) };
        }

        public EditorLod AddLod(EditorLod lod)
        {
            if (lod == null)
                return null;

            var sameDistanceLod = lods.Find(l => l.Distance == lod.Distance);

            if (sameDistanceLod == null)
            {
                lods.Add(lod);
                return lod;
            }
            else
                return sameDistanceLod;
        }

        public bool DeleteLod(int distance) =>
            GeneralMethods.RemoveListItems(lods, lod => lod.Distance == distance);
    }
}
