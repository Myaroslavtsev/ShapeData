/// Performs conversion from one EditorShape into many shapes present in tsection.dat

using ShapeData.Editor_shapes;
using ShapeData.Kuju_tsection.dat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ShapeData.Kuju_shape
{
    public class BatchConverter
    {
        public async static Task ConvertShape(
            string shapeFileName,
            string tsectionPath,
            string shapeFileNameMask = "*.*",
            string ffeditLocation = "",
            int limitCount = 0,
            float gauge = 0,
            bool skipRoads = true,
            bool prohibitVisualObstruction = false,
            bool hasWinterTextures = false)
        {
            var (shapesPath, refAndBatPath) = PrepareDirectories();

            var (shape, td) = await LoadInputData(shapeFileName, tsectionPath, skipRoads);

            var (ffeditExists, ffeditPathOnly, ffeditFileName) = BuildFfeditPaths(ffeditLocation);

            Console.WriteLine($"{DateTime.Now:T} Starting shape generation");

            var (refFile, batFile, tasks, numTasks) = await CreateShapeConversionTasks(
                shape, td, shapesPath, shapeFileNameMask, gauge, limitCount,
                ffeditExists, ffeditPathOnly, ffeditFileName,
                prohibitVisualObstruction, hasWinterTextures);

            await Task.WhenAll(tasks);

            await SaveOutputFiles(refAndBatPath, shape.ShapeName, refFile, batFile, ffeditExists, numTasks);
        }

        private static (string, string) PrepareDirectories()
        {
            string shapesPath = Directory.GetCurrentDirectory() + "\\Shapes\\";
            string refAndBatPath = Directory.GetCurrentDirectory() + "\\RefAndBat\\";

            Directory.CreateDirectory(shapesPath);
            Directory.CreateDirectory(refAndBatPath);

            return (shapesPath, refAndBatPath);
        }

        private static async Task<(EditorShape shape, KujuTsectionDat td)> LoadInputData(
            string shapeFileName, string tsectionPath, bool skipRoads = true)
        {
            Console.WriteLine($"{DateTime.Now:T} Loading shape from {shapeFileName}");
            var shapeContent = await GeneralMethods.ReadFileToString(shapeFileName);
            var shape = EditorShapeDeserializer.MakeShapeFromCsv(shapeContent);

            Console.WriteLine($"{DateTime.Now:T} Loading tsection.dat");
            var td = await KujuTsectionParser.LoadTsection(tsectionPath, skipRoads);

            return (shape, td);
        }

        private static (bool exists, string pathOnly, string fileName) BuildFfeditPaths(string ffeditLocation)
        {
            if (File.Exists(ffeditLocation))
            {
                var path = Path.GetDirectoryName(ffeditLocation);
                if (path == "") path = Directory.GetCurrentDirectory();
                return (true, path + '\\', Path.GetFileName(ffeditLocation));
            }

            if (ffeditLocation != "")
                Console.WriteLine($"{DateTime.Now:T} WARNING: ffeditc_unicode.exe not found, please compress shapes manually");

            return (false, "", "");
        }

        private static async Task<(string refFile, string batFile, List<Task> tasks, int taskCount)> CreateShapeConversionTasks(
            EditorShape shape,
            KujuTsectionDat td,
            string shapesPath,
            string shapeFileNameMask,
            float gauge,
            int limitCount,
            bool ffeditExists,
            string ffeditPathOnly,
            string ffeditFileName,
            bool prohibitVisualObstruction,
            bool hasWinterTextures)
        {
            var refFile = "SIMISA@@@@@@@@@@JINX0r1t______\r\n\r\n";
            var batFile = "";

            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
            int convertedShapesCount = 0;

            foreach (var trackShape in td.TrackShapes)
            {
                if (!StringMatchesMask(trackShape.Key, shapeFileNameMask))
                    continue;

                string trackShapeFileName = trackShape.Key;
                var trackShapeSections = trackShape.Value;

                if (gauge > 0 && (trackShapeSections.RoadShape == true ||
                    Math.Abs(td.TrackSections[trackShapeSections.Paths[0].TrackSections[0]].Gauge - gauge) > 1e-3))
                    continue;

                var sb = new StringBuilder();
                KujuShapeBuilder.GetRefFileEntry(trackShapeFileName, shape.ShapeName).PrintBlock(sb);
                refFile += sb.ToString();

                batFile += KujuShapeBuilder.GetFfeditCommandLine(shapesPath, trackShapeFileName);

                await semaphore.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await ReplicateShapeTask(shape, trackShapeSections, td,
                            shapesPath, trackShapeFileName,
                            ffeditExists, ffeditPathOnly, ffeditFileName,
                            prohibitVisualObstruction, hasWinterTextures);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));

                convertedShapesCount++;
                if (limitCount > 0 && convertedShapesCount >= limitCount)
                    break;

                if (convertedShapesCount % 100 == 0)
                    Console.WriteLine($"{DateTime.Now:T} Created {convertedShapesCount} shapes");
            }

            return (refFile, batFile, tasks, convertedShapesCount);
        }

        private static async Task SaveOutputFiles(string refAndBatPath, string shapeName, string refFile, string batFile, bool ffeditExists, int numTasks)
        {
            Console.WriteLine($"{DateTime.Now:T} Saving .ref and .bat files");

            await GeneralMethods.SaveStringToFile(Path.Combine(refAndBatPath, shapeName + ".ref"), refFile, DataFileFormat.UTF16LE);
            await GeneralMethods.SaveStringToFile(Path.Combine(refAndBatPath, shapeName + ".bat"), batFile, DataFileFormat.PlainText);

            Console.WriteLine($"{DateTime.Now:T} {numTasks} shape generation tasks finished.");
            if (!ffeditExists)
                Console.WriteLine($"Execute RefAndBat\\{shapeName}.bat to compress shapes manually.");
        }

        private static async Task ReplicateShapeTask(
            EditorShape shape,
            KujuTrackShape trackShapeSections,
            KujuTsectionDat td,
            string shapesPath,
            string trackShapeFileName,
            bool ffeditExists,
            string ffeditPathOnly,
            string ffeditFileName,
            bool prohibitVisualObstruction,
            bool hasWinterTextures)
        {
            var replica = await ShapeReplicator.ReplicatePartsInShape(shape, trackShapeSections, td);
            if (replica.Polygons().Any())
            {
                var shapeFiles = KujuShapeBuilder.BuildShapeFile(replica, prohibitVisualObstruction, hasWinterTextures);

                await GeneralMethods.SaveStringToFile(shapesPath + trackShapeFileName, shapeFiles.s, DataFileFormat.UTF16LE);
                await GeneralMethods.SaveStringToFile(shapesPath + trackShapeFileName + 'd', shapeFiles.sd, DataFileFormat.UTF16LE);

                if (ffeditExists)
                    await CompressShape(shapesPath, trackShapeFileName, ffeditPathOnly, ffeditFileName);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now:T} WARNING: Empty model generated for {trackShapeFileName}");
            }
        }

        private static async Task CompressShape(string shapesPath, string shapeFileName, string ffeditPath, string ffeditFileName)
        {
            var ffeditExePath = Path.Combine(ffeditPath, ffeditFileName);
            var inputPath = Path.Combine(shapesPath, shapeFileName);
            var outputPath = Path.Combine(ffeditPath, shapeFileName);

            var psi = new ProcessStartInfo
            {
                FileName = ffeditExePath,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
                WorkingDirectory = ffeditPath
            };

            psi.ArgumentList.Add($"{inputPath}");
            psi.ArgumentList.Add($"/o:{shapeFileName}");

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine($"{DateTime.Now:T} WARNING: Error while compressing file {outputPath}");
                return;
            }

            await process.WaitForExitAsync();

            if (File.Exists(outputPath))
                File.Move(outputPath, inputPath, true);
            else
                Console.WriteLine($"{DateTime.Now:T} WARNING: Error while compressing file {outputPath}");
        }

        private static bool StringMatchesMask(string filename, string mask)
        {
            string pattern = "^" + Regex.Escape(mask)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";

            return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
        }
    }
}
