/// Performs conversion from one EditorShape into many shapes present in tsection.dat

using ShapeData.Editor_shapes;
using ShapeData.Geometry;
using ShapeData.Kuju_tsection.dat;
using ShapeData.Kuju_shape;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ShapeData.Kuju_shape
{
    class BatchConverter
    {
        public async static Task ConvertShape(
            string shapeFileName,
            string tsectionPath,
            string shapeFileNameMask = "*.*",
            int limitCount = 0)
        {
            string shapesPath = Directory.GetCurrentDirectory() + "\\Shapes\\";
            Directory.CreateDirectory(shapesPath);

            Console.WriteLine($"{DateTime.Now:T} Loading shape from {shapeFileName}");
            var shape = EditorShapeDeserializer.MakeShapeFromCsv(await GeneralMethods.ReadFileToString(shapeFileName));

            Console.WriteLine($"{DateTime.Now:T} Loading tsection.dat");
            var td = await KujuTsectionParser.LoadTsection(tsectionPath);

            var refFile = "SIMISA@@@@@@@@@@JINX0r1t______\r\n\r\n";
            var batFile = "";

            Console.WriteLine($"{DateTime.Now:T} Starting shape generation");

            int convertedShapesCount = 0;
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);

            foreach (var trackShape in td.TrackShapes)
            {
                if (StringMatchesMask(trackShape.Key, shapeFileNameMask))
                {
                    var trackKey = trackShape.Key;
                    var trackValue = trackShape.Value;

                    var sb = new StringBuilder();
                    KujuShapeBuilder.GetRefFileEntry(trackKey, shape.ShapeName).PrintBlock(sb);
                    refFile += sb.ToString();

                    batFile += KujuShapeBuilder.GetFfeditCommandLine(shapesPath, trackKey);

                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var replica = await ShapeReplicator.ReplicatePartsInShape(shape, trackValue, td);
                            var shapeFiles = KujuShapeBuilder.BuildShapeFile(replica);

                            await GeneralMethods.SaveStringToFile(shapesPath + trackKey, shapeFiles.s, DataFileFormat.UTF16LE);
                            await GeneralMethods.SaveStringToFile(shapesPath + trackKey + 'd', shapeFiles.sd, DataFileFormat.UTF16LE);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));

                    convertedShapesCount++;
                    if (limitCount > 0 && convertedShapesCount >= limitCount) break;

                    if (convertedShapesCount % 100 == 0)
                        Console.WriteLine($"{DateTime.Now:T} Converted {convertedShapesCount} shapes");
                }
            }

            await Task.WhenAll(tasks);

            Console.WriteLine($"{DateTime.Now:T} Saving .ref and .bat files");
            await GeneralMethods.SaveStringToFile(shape.ShapeName + ".ref", refFile, DataFileFormat.UTF16LE);
            await GeneralMethods.SaveStringToFile(shape.ShapeName + ".bat", batFile, DataFileFormat.PlainText);

            Console.WriteLine($"{DateTime.Now:T} All shape generation tasks finished. Execute .bat to compress shapes.");
        }

        private static bool StringMatchesMask(string filename, string mask)
        {
            string pattern = Regex.Escape(mask)
            .Replace(@"\*", ".*")   // * → любое количество любых символов
            .Replace(@"\?", ".");   // ? → один любой символ

            pattern = "^" + pattern + "$";

            return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
        }
    }
}
