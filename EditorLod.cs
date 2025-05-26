using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    public class EditorLod
    {
        // General properties
        public int Distance;

        // Parts
        private readonly List<EditorPart> parts;

        public List<EditorPart> Parts => parts;

        // Methods
        public EditorLod(int distance)
        {
            Distance = distance;
            parts = new List<EditorPart>();
        }

        public bool AddPart(EditorPart part)
        {
            if (parts.Find(p => p.PartName == part.PartName) == null)
            {
                parts.Add(part);
                return true;
            }
            return false;
        }

        public bool DeletePart(string name) =>
            GeneralMethods.RemoveListItems(parts, p => p.PartName == name);
    }
}
