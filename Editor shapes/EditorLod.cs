using System.Collections.Generic;

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
