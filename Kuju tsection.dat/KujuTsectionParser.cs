using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ShapeData.Kuju_tsection.dat
{
    class TsectionDataBlock
    {
        public string Caption;

        public string Data;

        public int BlockEnd;

        public TsectionDataBlock(string caption, string data, int bEnd)
        {
            Caption = caption;
            Data = data;
            BlockEnd = bEnd;
        }
    }

    class KujuTsectionParser
    {
        public static async Task<KujuTsectionDat> LoadTsection(string fileName, bool skipRoads = true)
        {
            var tsection = new KujuTsectionDat();

            var filedata = SimplifySpaces(await GeneralMethods.ReadFileToString(fileName));

            ParseShapesAndSections(GetAllTrackSectionBlocks(filedata), tsection, skipRoads);

            ParseShapesAndSections(GetAllTrackShapeBlocks(filedata), tsection, skipRoads);

            return tsection;
        }

        private static string SimplifySpaces(string data)
        {
            while (data.IndexOf("  ") > -1)
            {
                data = data.Replace("  ", " ");
            }

            return data;
        }

        private static void ParseShapesAndSections(string trackSections, KujuTsectionDat tsection, bool skipRoads)
        {
            int blockStart = 0;
            var dataBlock = GetDataBlock(trackSections, blockStart);

            while (dataBlock.BlockEnd != -1)
            {
                if (dataBlock.Caption == "TrackSection")
                    ParseOneTrackSection(dataBlock.Data, tsection);

                if (dataBlock.Caption == "TrackShape")
                    ParseOneTrackShape(dataBlock.Data, tsection, skipRoads);

                blockStart = dataBlock.BlockEnd;
                dataBlock = GetDataBlock(trackSections, blockStart);
            }
        }

        private static void ParseOneTrackShape(string data, KujuTsectionDat tsection, bool skipRoads)
        {
            var sectionId = ParseSectionId(data);
            if (sectionId == null) return;

            var shape = new KujuTrackShape();

            ParseShapeBlocks(data, shape);

            if (!tsection.TrackShapes.ContainsKey(shape.FileName) &&
                !(skipRoads && shape.RoadShape))
                tsection.TrackShapes.Add(shape.FileName, shape);
        }

        private static void ParseShapeBlocks(string data, KujuTrackShape shape)
        {
            int blockStart = 0;
            var dataBlock = GetDataBlock(data, blockStart);

            while (dataBlock.BlockEnd != -1)
            {
                switch (dataBlock.Caption.ToLower())
                {
                    case "filename":
                        shape.Rename(dataBlock.Data);
                        break;
                    case "sectionidx":
                        ParseSectionIdx(dataBlock.Data, shape, new CultureInfo("en-US"));
                        break;
                    case "roadshape":
                        shape.RoadShape = true;
                        break;
                }

                blockStart = dataBlock.BlockEnd;
                dataBlock = GetDataBlock(data, blockStart);
            }
        }

        private static void ParseSectionIdx(string data, KujuTrackShape shape, CultureInfo cultureInfo)
        {
            var values = data.Split(" ");
            if (values.Length >= 6)
            {
                var path = new KujuTrackPath();

                if (!double.TryParse(values[1], NumberStyles.Any, cultureInfo, out double dX)) return;
                if (!double.TryParse(values[2], NumberStyles.Any, cultureInfo, out double dY)) return;
                if (!double.TryParse(values[2], NumberStyles.Any, cultureInfo, out double dZ)) return;
                if (!double.TryParse(values[2], NumberStyles.Any, cultureInfo, out double dA)) return;

                for (int i = 5; i < values.Length; i++)
                {
                    if (!int.TryParse(values[i], out int id)) return;
                    path.TrackSections.Add(id);
                }

                path.Direction.X = dX;
                path.Direction.Y = dY;
                path.Direction.Z = dZ;
                path.Direction.A = dA;

                shape.Paths.Add(path);
            }
        }

        private static void ParseOneTrackSection(string data, KujuTsectionDat tsection)
        {
            var sectionId = ParseSectionId(data);
            if (sectionId == null) return;

            var section = new KujuTrackSection(sectionId.Value);

            ParseSectionBlocks(data, section);

            if (!tsection.TrackSections.ContainsKey(sectionId.Value))
                tsection.TrackSections.Add(sectionId.Value, section);
        }

        private static int? ParseSectionId(string data)
        {
            var sectionIdText = data[0..Math.Min(data.IndexOf(" ", 1), data.IndexOf("\r\n", 1))];

            if (int.TryParse(sectionIdText.Trim(), out int sectionId))
                return sectionId;

            return null;
        }

        private static void ParseSectionBlocks(string data, KujuTrackSection section)
        {
            var cultureInfo = new CultureInfo("en-US");
            int blockStart = 0;
            var dataBlock = GetDataBlock(data, blockStart);

            while (dataBlock.BlockEnd != -1)
            {
                switch (dataBlock.Caption.ToLower())
                {
                    case "sectionsize":
                        ParseSectionSize(dataBlock.Data, section, cultureInfo);
                        break;
                    case "sectioncurve":
                        ParseSectionCurve(dataBlock.Data, section, cultureInfo);
                        break;
                }

                blockStart = dataBlock.BlockEnd;
                dataBlock = GetDataBlock(data, blockStart);
            }
        }

        private static void ParseSectionSize(string data, KujuTrackSection section, CultureInfo cultureInfo)
        {
            var values = data.Split(" ");
            if (values.Length >= 2)
            {
                double.TryParse(values[0], NumberStyles.Any, cultureInfo, out double gauge);
                double.TryParse(values[1], NumberStyles.Any, cultureInfo, out double straight);
                section.Gauge = gauge;
                section.SectionTrajectory.Straight = straight;
            }
        }

        private static void ParseSectionCurve(string data, KujuTrackSection section, CultureInfo cultureInfo)
        {
            var values = data.Split(" ");
            if (values.Length >= 2)
            {
                double.TryParse(values[0], NumberStyles.Any, cultureInfo, out double radius);
                double.TryParse(values[1], NumberStyles.Any, cultureInfo, out double angle);
                section.SectionTrajectory.Radius = radius;
                section.SectionTrajectory.Angle = angle;
            }
        }

        private static string GetAllTrackSectionBlocks(string tsectionFile)
        {
            int blockStart = 0;

            while (true)
            {
                var dataBlock = GetDataBlock(tsectionFile, blockStart);

                if (dataBlock.BlockEnd == -1)
                    return "";

                if (dataBlock.Caption == "TrackSections")
                    return dataBlock.Data;

                blockStart = dataBlock.BlockEnd;
            }
        }

        private static string GetAllTrackShapeBlocks(string tsectionFile)
        {
            int blockStart = 0;

            while (true)
            {
                var dataBlock = GetDataBlock(tsectionFile, blockStart);

                if (dataBlock.BlockEnd == -1)
                    return "";

                if (dataBlock.Caption == "TrackShapes")
                    return dataBlock.Data;

                blockStart = dataBlock.BlockEnd;
            }
        }

        private static TsectionDataBlock GetDataBlock(string file, int startPos)
        {
            var (blockName, bracketPos) = FindDataBloskStart(file, startPos);
            if (bracketPos < 0)
                return new TsectionDataBlock("", "", -1);

            var endPos = FindDataBlockEnd(file, bracketPos);

            if (endPos > bracketPos)
                return new TsectionDataBlock(blockName.Trim(),
                    file[(bracketPos + 1)..(endPos - 1)].Trim(),
                    endPos);

            return new TsectionDataBlock("", "", -1);
        }

        private static (string blockName, int bracketPos) FindDataBloskStart(string file, int startPos)
        {
            int lineStartPos = file.IndexOf("\r\n", startPos, StringComparison.OrdinalIgnoreCase);

            while (lineStartPos < file.Length && lineStartPos >= 0)
            {
                var lineEndPos = file.IndexOf("\r\n", lineStartPos + 1, StringComparison.OrdinalIgnoreCase);

                if (lineEndPos < 0)
                    lineEndPos = file.Length;

                var bracketPos = file.IndexOf('(', lineStartPos, lineEndPos - lineStartPos);

                if (bracketPos > lineStartPos)
                    return (file.Substring(lineStartPos + 2, bracketPos - lineStartPos - 2).TrimEnd(), bracketPos);

                lineStartPos = lineEndPos;
            }

            return ("", -1);
        }

        private static int FindDataBlockEnd(string file, int bracketPos)
        {
            var bracketCount = 1;

            while (bracketPos++ < file.Length && bracketCount > 0)
            {
                if (file[bracketPos] == '(')
                    bracketCount++;

                if (file[bracketPos] == ')')
                    bracketCount--;
            }

            return bracketPos;
        }

    }
}
