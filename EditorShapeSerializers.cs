using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    class EditorShapeSerializers
    {
        public string MakeCsvFromEditorShape(EditorShape shape)
        {
            var sb = new StringBuilder();

            AddShapeDataToSb(shape, sb);

            sb.AppendLine("");

            return sb.ToString();
        }

        public EditorShape MakeShapeFromCsv(string csvData) =>
    GetShapeFromCells(SplitCsvToCells(csvData));

        private void AddShapeDataToSb(EditorShape shape, StringBuilder sb)
        {
            sb.AppendLine("Shape" + ";" + shape.ShapeName.Replace(';', ':'));

            foreach (var lod in shape.Lods.OrderBy(l => l.Distance))
            {
                AddLodDataToSb(lod, sb);
            }
        }

        private void AddLodDataToSb(EditorLod lod, StringBuilder sb)
        {
            sb.AppendLine(";" + "Lod" + ";" + lod.Distance);

            foreach (var part in lod.Parts)
            {
                AddPartDataToSb(part, sb);
            }
        }

        private void AddPartDataToSb(EditorPart part, StringBuilder sb)
        {
            var dataString = ";;" + "Part" + ";" +
                part.PartName + ";" +
                part.SayIfSmoothed() + ";" +
                part.ReplicationParams.ReplicationMetod;

            foreach (var p in part.ReplicationParams.GetParams())
            {
                dataString += ";" + p.Name + ";" + p.Value.ToString("0.0000");
            }

            sb.AppendLine(dataString);

            foreach (var poly in part.Polygons)
            {
                AddPolygonDataToSb(poly, sb);
            }
        }

        private void AddPolygonDataToSb(EditorPolygon polygon, StringBuilder sb)
        {
            sb.AppendLine(";;;" + "Polygon" + ";" +
                polygon.PolygonId + ";" +
                polygon.MaterialType + ";" +
                polygon.TextureFilename);

            foreach (var vertex in polygon.Vertices)
            {
                AddVertexDataToSb(vertex, sb);
            }
        }

        private void AddVertexDataToSb(EditorVertex vertex, StringBuilder sb)
        {
            sb.AppendLine(";;;;V;" + 
                vertex.X.ToString("0.0000") + ";" +
                vertex.Y.ToString("0.0000") + ";" +
                vertex.Z.ToString("0.0000") + ";" +
                vertex.U.ToString("0.00000") + ";" +
                vertex.V.ToString("0.00000"));
        }

        private EditorShape GetShapeFromCells(List<(int emptyCellCount, List<string> line)> cells)
        {
            for(var lineCount = 0; lineCount < cells.Count(); lineCount++)
            {
                if (cells[lineCount].emptyCellCount == 0)
                {
                    var shape = new EditorShape(cells[lineCount].line[1]);

                    //EditorShape.

                    return shape;
                }
            }

            return null;
        }

        private List<(int emptyCellCount, List<string> cells)> SplitCsvToCells(string csvData)
        {
            var cells = new List<(int emptyCellCount, List<string> cells)>();
            
            var lines = csvData.Split("\r\n");

            foreach(var line in lines)
            {
                var lineCells = SplitLine(line);

                if (IsValidLine(lineCells))
                {
                    cells.Add(lineCells);
                }
            }

            return cells;
        }

        private (int, List<string>) SplitLine(string line)
        {
            var cells = line.Split(";").ToList();

            int emptyCellCount = 0;
            foreach(var cell in cells)
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

        private bool IsValidLine((int emptyCellCount, List<string> cells) line)
        {
            switch (line.emptyCellCount)
            {
                case 0: return IsValidShapeLine(line.cells);
                case 1: return IsValidLodLine(line.cells);
                case 2: return IsValidPartLine(line.cells);
                case 3: return IsValidPolygonLine(line.cells);
                case 4: return IsValidVertexLine(line.cells);
                default: return false;
            }    
        }

        private bool IsValidShapeLine(List<string> cells)
        {
            if (cells.Count >= 2 && 
                cells[0].ToLower() == "shape") return true;
            return false; 
        }

        private bool IsValidLodLine(List<string> cells)
        {
            if (cells.Count >= 2 &&
                cells[0].ToLower() == "lod") return true;
            return false;
        }

        private bool IsValidPartLine(List<string> cells)
        {
            if (cells.Count >= 5 &&
                cells[0].ToLower() == "part") return true;
            return false;
        }

        private bool IsValidPolygonLine(List<string> cells)
        {
            if (cells.Count >= 4 &&
                cells[0].ToLower() == "polygon") return true;
            return false;
        }

        private bool IsValidVertexLine(List<string> cells)
        {
            if (cells.Count >= 5 &&
                cells[0].ToLower() == "v") return true;
            return false;
        }

    }
}
