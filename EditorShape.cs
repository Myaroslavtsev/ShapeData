using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    public class EditorShape
    {
        // General properties
        public string ShapeName;

        // Objects
        private readonly List<EditorLod> lods;

        public List<EditorLod> Lods => lods;

        // Methods
        public EditorShape(string shapeName)
        {
            ShapeName = shapeName;
            lods = new List<EditorLod> { new EditorLod(2000) };
        }

        public bool AddLod(EditorLod lod)
        {
            lods.Add(lod);
            return true;
        }

        public bool DeleteLod(int distance) =>
            GeneralMethods.RemoveListItems(lods, lod => lod.Distance == distance);

    }
}
