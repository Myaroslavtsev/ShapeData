/// Reads tsection.dat file and returns an instance of KujuTsectionDat class

using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

            await Task.WhenAll(
                Task.Run(() => ParseSectionsParallel(GetAllTrackSectionBlocks(filedata), tsection, skipRoads)),
                Task.Run(() => ParseShapesParallel(GetAllTrackShapeBlocks(filedata), tsection, skipRoads))
            );

            return tsection;
        }

        private static string SimplifySpaces(string input)
        {
            var sb = new StringBuilder(input.Length);
            var span = input.AsSpan();

            int lineStart = 0;
            while (lineStart < span.Length)
            {
                int lineEnd = span.Slice(lineStart).IndexOf("\r\n") switch
                {
                    -1 => span.Length,
                    int rel => lineStart + rel
                };

                var line = span.Slice(lineStart, lineEnd - lineStart).TrimStart();

                bool prevSpace = false;
                foreach (var c in line)
                {
                    if (c == ' ')
                    {
                        if (!prevSpace)
                        {
                            sb.Append(' ');
                            prevSpace = true;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                        prevSpace = false;
                    }
                }

                sb.Append("\r\n");

                lineStart = lineEnd + 2; // Skip "\r\n"
            }

            return sb.ToString();
        }

        private static void ParseSectionsParallel(string blockData, KujuTsectionDat tsection, bool skipRoads)
        {
            var cultureInfo = new CultureInfo("en-US");

            var dataBlocks = ExtractDataBlocks(blockData);

            Parallel.ForEach(dataBlocks, dataBlock =>
            {
                if (dataBlock.Caption.AsSpan().Equals("tracksection".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    var sectionId = ParseSectionId(dataBlock.Data);
                    if (sectionId == null) return;

                    var section = new KujuTrackSection(sectionId.Value);
                    ParseSectionBlocks(dataBlock.Data, section, cultureInfo);

                    tsection.TrackSections.TryAdd(sectionId.Value, section);
                }
            });
        }

        private static void ParseShapesParallel(string blockData, KujuTsectionDat tsection, bool skipRoads)
        {
            var cultureInfo = new CultureInfo("en-US");

            var dataBlocks = ExtractDataBlocks(blockData);

            Parallel.ForEach(dataBlocks, dataBlock =>
            {
                if (dataBlock.Caption.AsSpan().Equals("trackshape".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    var sectionId = ParseSectionId(dataBlock.Data);
                    if (sectionId != null)
                    {
                        var shape = new KujuTrackShape();
                        ParseShapeBlocks(dataBlock.Data, shape, cultureInfo);

                        if (!(skipRoads && shape.RoadShape))
                            tsection.TrackShapes.TryAdd(shape.FileName, shape);
                    }                    
                }
            });
        }

        private static List<TsectionDataBlock> ExtractDataBlocks(string blockData)
        {
            var blocks = new List<TsectionDataBlock>();
            int blockStart = 0;
            TsectionDataBlock dataBlock;

            do
            {
                dataBlock = GetDataBlock(blockData, blockStart);
                if (dataBlock.BlockEnd != -1)
                {
                    blocks.Add(dataBlock);
                    blockStart = dataBlock.BlockEnd;
                }
            } while (dataBlock.BlockEnd != -1);

            return blocks;
        }

        private static void ParseShapeBlocks(string data, KujuTrackShape shape, CultureInfo cultureInfo)
        {
            int blockStart = 0;
            TsectionDataBlock dataBlock = GetDataBlock(data, blockStart);

            while (dataBlock.BlockEnd != -1)
            {
                var captionSpan = dataBlock.Caption.AsSpan();

                if (captionSpan.Equals("filename".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    shape.Rename(dataBlock.Data);
                }
                else if (captionSpan.Equals("sectionidx".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    ParseSectionIdx(dataBlock.Data, shape, cultureInfo);
                }
                else if (captionSpan.Equals("roadshape".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    shape.RoadShape = true;
                }

                blockStart = dataBlock.BlockEnd;
                dataBlock = GetDataBlock(data, blockStart);
            }
        }

        private static void ParseSectionIdx(string data, KujuTrackShape shape, CultureInfo cultureInfo)
        {
            var span = data.AsSpan();
            var path = new KujuTrackPath();

            // Пропустить первый элемент (обычно "0")
            int idx = span.IndexOf(' ');
            if (idx == -1 || idx + 1 >= span.Length) return;
            span = span.Slice(idx + 1);

            // Далее: dX, dY, dZ, dA
            if (!TryReadDouble(ref span, out double dX, cultureInfo)) return;
            if (!TryReadDouble(ref span, out double dY, cultureInfo)) return;
            if (!TryReadDouble(ref span, out double dZ, cultureInfo)) return;
            if (!TryReadDouble(ref span, out double dA, cultureInfo)) return;

            // Остальные — int ID
            while (!span.IsEmpty)
            {
                if (!TryReadInt(ref span, out int id)) return;
                path.TrackSections.Add(id);
            }

            path.Direction.X = dX;
            path.Direction.Y = dY;
            path.Direction.Z = dZ;
            path.Direction.A = dA;

            shape.Paths.Add(path);
        }

        private static int? ParseSectionId(string data)
        {
            var span = data.AsSpan();

            int spaceIdx = span.IndexOf(' ');
            int newlineIdx = span.IndexOf("\r\n");

            int endIdx = (spaceIdx, newlineIdx) switch
            {
                (-1, -1) => span.Length,
                (_, -1) => spaceIdx,
                (-1, _) => newlineIdx,
                _ => Math.Min(spaceIdx, newlineIdx)
            };

            var idSpan = span.Slice(0, endIdx).Trim();
            if (int.TryParse(idSpan, out int sectionId))
                return sectionId;

            return null;
        }

        private static void ParseSectionBlocks(string data, KujuTrackSection section, CultureInfo cultureInfo)
        {
            int blockStart = 0;
            TsectionDataBlock dataBlock = GetDataBlock(data, blockStart);

            while (dataBlock.BlockEnd != -1)
            {
                var captionSpan = dataBlock.Caption.AsSpan();

                if (captionSpan.Equals("sectionsize".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    ParseSectionSize(dataBlock.Data, section, cultureInfo);
                }
                else if (captionSpan.Equals("sectioncurve".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    ParseSectionCurve(dataBlock.Data, section, cultureInfo);
                }

                blockStart = dataBlock.BlockEnd;
                dataBlock = GetDataBlock(data, blockStart);
            }
        }

        private static void ParseSectionSize(string data, KujuTrackSection section, CultureInfo cultureInfo)
        {
            var span = data.AsSpan();

            if (!TryReadDouble(ref span, out double gauge, cultureInfo)) return;
            if (!TryReadDouble(ref span, out double straight, cultureInfo)) return;

            section.Gauge = gauge;
            section.SectionTrajectory.Straight = straight;
        }

        private static void ParseSectionCurve(string data, KujuTrackSection section, CultureInfo cultureInfo)
        {
            var span = data.AsSpan();

            if (!TryReadDouble(ref span, out double radius, cultureInfo)) return;
            if (!TryReadDouble(ref span, out double angle, cultureInfo)) return;

            section.SectionTrajectory.Radius = radius;
            section.SectionTrajectory.Angle = angle;
        }

        private static string GetAllTrackSectionBlocks(string tsectionFile)
        {
            int blockStart = 0;

            while (true)
            {
                var dataBlock = GetDataBlock(tsectionFile, blockStart);

                if (dataBlock.BlockEnd == -1)
                    return "";

                if (dataBlock.Caption.AsSpan().Equals("tracksections".AsSpan(), StringComparison.OrdinalIgnoreCase))
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

                if (dataBlock.Caption.AsSpan().Equals("trackshapes".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return dataBlock.Data; // этот блок содержит много TrackShape
                }

                blockStart = dataBlock.BlockEnd;
            }
        }

        private static TsectionDataBlock GetDataBlock(string file, int startPos)
        {
            int cursor = startPos;

            while (cursor < file.Length)
            {
                // 1. Найти начало строки
                int lineEnd = file.IndexOf("\r\n", cursor, StringComparison.OrdinalIgnoreCase);
                if (lineEnd == -1) lineEnd = file.Length;

                var line = file.AsSpan(cursor, lineEnd - cursor);

                // 2. Найти открывающую скобку
                int openBracketPos = line.IndexOf('(');
                if (openBracketPos >= 0)
                {
                    var caption = line[..openBracketPos].Trim().ToString();
                    int globalBracketPos = cursor + openBracketPos;

                    // 3. Найти конец блока начиная с открывающей скобки
                    int relativeEnd = FindDataBlockEnd(file.AsSpan(globalBracketPos));
                    if (relativeEnd <= 0)
                        return new TsectionDataBlock("", "", -1);

                    int globalEnd = globalBracketPos + relativeEnd;
                    var innerData = file.AsSpan(globalBracketPos + 1, relativeEnd - 2).Trim().ToString();

                    return new TsectionDataBlock(caption, innerData, globalEnd);
                }

                // 4. Перейти к следующей строке
                cursor = lineEnd + 2;
            }

            return new TsectionDataBlock("", "", -1);
        }

        private static int FindDataBlockEnd(ReadOnlySpan<char> span)
        {
            int depth = 1;
            for (int i = 1; i < span.Length; i++)
            {
                if (span[i] == '(') depth++;
                else if (span[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                        return i + 1;
                }
            }
            return -1;
        }

        private static bool TryReadDouble(ref ReadOnlySpan<char> span, out double value, CultureInfo cultureInfo)
        {
            span = span.TrimStart();
            int nextSpace = span.IndexOf(' ');
            ReadOnlySpan<char> token;

            if (nextSpace == -1)
            {
                token = span;
                span = ReadOnlySpan<char>.Empty;
            }
            else
            {
                token = span.Slice(0, nextSpace);
                span = span.Slice(nextSpace + 1);
            }

            return double.TryParse(token, NumberStyles.Any, cultureInfo, out value);
        }

        private static bool TryReadInt(ref ReadOnlySpan<char> span, out int value)
        {
            span = span.TrimStart();
            int nextSpace = span.IndexOf(' ');
            ReadOnlySpan<char> token;

            if (nextSpace == -1)
            {
                token = span;
                span = ReadOnlySpan<char>.Empty;
            }
            else
            {
                token = span.Slice(0, nextSpace);
                span = span.Slice(nextSpace + 1);
            }

            return int.TryParse(token, out value);
        }
    }
}
