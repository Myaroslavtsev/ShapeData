/// Converts a string data received from .csv file into EditorShape instance.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeData
{
    public class EditorShapeDeserializer
    {
        public static EditorShape MakeShapeFromCsv(string csvData) =>
            GetShapeFromCells(SplitCsvToCells(csvData));

        private static EditorShape GetShapeFromCells(List<(int emptyCellCount, List<string> line)> cells)
        {
            int lineNumber = SkipLinesWithIncorrectIndents(cells, 0, 0);

            var shape = new EditorShape(cells[lineNumber].line[1]);

            if (cells[lineNumber].line.Count > 2)
                shape.ShapeComment = cells[lineNumber].line[2];

            AddEditorObjects(cells, ++lineNumber, shape, null, null, null);

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
                        part.AddPolygon(GetPolygonFromLine(cells[lineNumber++].line));
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

        private static EditorPolygon GetPolygonFromLine(List<string> line)
        {
            return new EditorPolygon(new List<EditorVertex>(), ParseMaterialName(line[1]), line[2], IsTrackbedPolygon(line[3]));
        }

        private static bool IsTrackbedPolygon(string data) =>
            data.ToLower() == "trackbed";

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
            var part = new EditorPart(line[1],
                ParseReplicationParameters(line),
                line[2].ToLower() == "smoothed"
                );           

            return part;
        }

        private static PartReplication ParseReplicationParameters(List<string> line)
        {
            if (line.Count < 9)
                return PartReplication.NoReplication();

            if (!Enum.TryParse<PartReplicationMethod>(line[3], ignoreCase: true, out var replicationMethod))
                replicationMethod = PartReplicationMethod.NoReplication;

            if (!Enum.TryParse<PartStretchInWidthMethod>(line[4], ignoreCase: true, out var stretchInWidthMethod))
                stretchInWidthMethod = PartStretchInWidthMethod.ReplicateAlongAllTracks;

            if (!Enum.TryParse<PartScalingMethod>(line[5], ignoreCase: true, out var scalingMethod))
                scalingMethod = PartScalingMethod.FixLength;

            var scaleTexture = line[6].ToLower() == "scaletexture";
            var bendPart = line[7].ToLower() == "bendpart";
            var leaveAtLeastOne = line[8].ToLower() == "leaveatleastone";

            var replicationParams = new Dictionary<string, float>();

            for (int i = 9; i + 1 < line.Count; i+=2)
            {
                if (!float.TryParse(line[i + 1], out var paramValue))
                    paramValue = 0;

                replicationParams.Add(line[i], paramValue);
            }            

            return new PartReplication(replicationMethod, scalingMethod, stretchInWidthMethod, 
                scaleTexture, bendPart, leaveAtLeastOne, replicationParams);
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
            if (cells.Count >= 5 &&
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
