using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class EditorShapeDeserializer
    {
        public static EditorShape MakeShapeFromCsv(string csvData) =>
            GetShapeFromCells(SplitCsvToCells(csvData));

        private static EditorShape GetShapeFromCells(List<(int emptyCellCount, List<string> line)> cells)
        {
            int lineNumber = SkipLinesWithIncorrectIndents(cells, 0, 0);

            var shape = new EditorShape(cells[lineNumber++].line[1]);

            AddEditorObjects(cells, lineNumber, shape, null, null, null);

            return shape;
        }

        private static void AddEditorObjects(List<(int emptyCellCount, List<string> line)> cells,
            int lineNumber,
            EditorShape shape,
            EditorLod lod,
            EditorPart part,
            EditorPolygon polygon)
        {
            if (lineNumber >= cells.Count)
                return;

            switch (cells[lineNumber].emptyCellCount)
            {
                case 1:
                    if (shape != null)
                    {
                        var newLod = shape.AddLod(GetLodFromLine(cells[lineNumber++].line));
                        AddEditorObjects(cells, lineNumber, shape, newLod, null, null);
                    }
                    break;

                case 2:
                    if (lod != null)
                        lod.AddPart(GetPartFromLine(cells[lineNumber++].line));
                    AddEditorObjects(cells, lineNumber, shape, lod, lod.Parts.Last(), null);
                    break;

                case 3:
                    if (part != null)
                        part.AddPolygon(GetPolygonFromLine(cells[lineNumber++].line, part.Polygons.Count));
                    AddEditorObjects(cells, lineNumber, shape, lod, part, part.Polygons.Last());
                    break;

                case 4:
                    if (polygon != null)
                        polygon.AddVertex(GetVertexFromLine(cells[lineNumber++].line));
                    AddEditorObjects(cells, lineNumber, shape, lod, part, polygon);
                    break;

                default: return;
            }
        }

        private static EditorVertex GetVertexFromLine(List<string> line)
        {
            if (!float.TryParse(line[1], out float x)) return null;
            if (!float.TryParse(line[2], out float y)) return null;
            if (!float.TryParse(line[3], out float z)) return null;
            if (!float.TryParse(line[4], out float u)) return null;
            if (!float.TryParse(line[5], out float v)) return null;

            return new EditorVertex(x, y, z, u, v);
        }

        private static EditorPolygon GetPolygonFromLine(List<string> line, int id)
        {
            return new EditorPolygon((uint)id, new List<EditorVertex>(), ParseMaterialName(line[2]), line[3]);
        }

        private static Material ParseMaterialName(string materialName)
        {
            foreach (Material material in Enum.GetValues(typeof(Material)))
            {
                if (material.ToString() == materialName.ToLower())
                    return material;
            }

            return Material.SolidNorm;
        }

        private static EditorPart GetPartFromLine(List<string> line)
        {
            return new EditorPart(line[1],
                ParseReplicationParameters(line),
                line[2].ToLower() == "smoothed");
        }

        private static IPartReplication ParseReplicationParameters(List<string> line)
        {
            switch (line[3].ToLower())
            {
                case "attheend":
                    return new ReplicationAtTheEnd();

                case "byfixedintervals":
                    if (!float.TryParse(line[5], out var interval))
                        return new ReplicationAtFixedPos();

                    return new ReplicationByFixedIntervals(interval);

                case "stretchedbyhorde":
                    if (!float.TryParse(line[5], out var originalLength))
                        return new ReplicationAtFixedPos();

                    if (!float.TryParse(line[7], out var minimalLength))
                        return new ReplicationAtFixedPos();

                    return new ReplicationStretchedByHorde(originalLength, minimalLength);

                case "stretchedbydeflection":
                    if (!float.TryParse(line[5], out var maxDeflection))
                        return new ReplicationAtFixedPos();

                    return new ReplicationStretchedByDeflection(maxDeflection);
            }

            return new ReplicationAtFixedPos();
        }

        private static EditorLod GetLodFromLine(List<string> line)
        {
            if (!int.TryParse(line[1], out int distance))
                return null;

            return new EditorLod(distance);
        }

        private static int SkipLinesWithIncorrectIndents(List<(int emptyCellCount, List<string> line)> cells, int indentCount, int lineNumber)
        {
            while (cells[lineNumber].emptyCellCount != indentCount)
            {
                if (lineNumber == cells.Count)
                    return cells.Count;

                lineNumber++;
            }

            return lineNumber;
        }

        private static List<(int emptyCellCount, List<string> cells)> SplitCsvToCells(string csvData)
        {
            var cells = new List<(int emptyCellCount, List<string> cells)>();

            var lines = csvData.Split("\r\n");

            foreach (var line in lines)
            {
                var lineCells = SplitLine(line);

                if (IsValidLine(lineCells))
                {
                    cells.Add(lineCells);
                }
            }

            return cells;
        }

        private static (int, List<string>) SplitLine(string line)
        {
            var cells = line.Split(";").ToList();

            int emptyCellCount = 0;
            foreach (var cell in cells)
            {
                if (cell.Replace(" ", "") == "")
                {
                    emptyCellCount++;
                }
                else break;
            }

            cells.RemoveRange(0, emptyCellCount);

            return (emptyCellCount, cells);
        }

        private static bool IsValidLine((int emptyCellCount, List<string> cells) line)
        {
            return line.emptyCellCount switch
            {
                0 => IsValidShapeLine(line.cells),
                1 => IsValidLodLine(line.cells),
                2 => IsValidPartLine(line.cells),
                3 => IsValidPolygonLine(line.cells),
                4 => IsValidVertexLine(line.cells),
                _ => false,
            };
        }

        private static bool IsValidShapeLine(List<string> cells)
        {
            if (cells.Count >= 2 &&
                cells[0].ToLower() == "shape") return true;
            return false;
        }

        private static bool IsValidLodLine(List<string> cells)
        {
            if (cells.Count >= 2 &&
                cells[0].ToLower() == "lod") return true;
            return false;
        }

        private static bool IsValidPartLine(List<string> cells)
        {
            if (cells.Count >= 4 &&
                cells[0].ToLower() == "part") return true;
            return false;
        }

        private static bool IsValidPolygonLine(List<string> cells)
        {
            if (cells.Count >= 4 &&
                cells[0].ToLower() == "polygon") return true;
            return false;
        }

        private static bool IsValidVertexLine(List<string> cells)
        {
            if (cells.Count >= 5 &&
                cells[0].ToLower() == "v") return true;
            return false;
        }
    }
}
