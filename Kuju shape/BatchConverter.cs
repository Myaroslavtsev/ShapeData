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
        public async static Task ConvertShape(string shapeFileName,
            string tsectionPath, 
            string shapeFileNameMask = "*.*",            
            int limitCount = 0)
        {            
            string shapesPath = Directory.GetCurrentDirectory() + "\\Shapes\\";
            Directory.CreateDirectory(shapesPath);

            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + "Loading shape from " + shapeFileName);
            var shape = EditorShapeDeserializer.MakeShapeFromCsv(await GeneralMethods.ReadFileToString(shapeFileName));

            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + "Loading tsection.dat");
            var td = await KujuTsectionParser.LoadTsection(tsectionPath);

            var refFile = "SIMISA@@@@@@@@@@JINX0r1t______" + "\r\n" + "\r\n";
            var batFile = "";

            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + "Generating conversion tasks");
            var conversionTasks = new List<(Task task, string taskName)>();

            int convertedShapesCount = 0;
            foreach(var trackShape in td.TrackShapes)
            {
                if (StringMatchesMask(trackShape.Key, shapeFileNameMask))
                {
                    var sb = new StringBuilder();
                    KujuShapeBuilder.GetRefFileEntry(trackShape.Key, shape.ShapeName).PrintBlock(sb);
                    refFile += sb.ToString();

                    batFile += KujuShapeBuilder.GetFfeditCommandLine(shapesPath, trackShape.Key);

                    conversionTasks.Add ((Task.Run(async () =>
                    {
                        var replica = await ShapeReplicator.ReplicatePartsInShape(shape, trackShape.Value, td);

                        var shapeFiles = KujuShapeBuilder.BuildShapeFile(replica);

                        await GeneralMethods.SaveStringToFile(shapesPath + trackShape.Key, shapeFiles.s, DataFileFormat.UTF16LE);
                        await GeneralMethods.SaveStringToFile(shapesPath + trackShape.Key + 'd', shapeFiles.sd, DataFileFormat.UTF16LE);
                    }), 
                    trackShape.Key));

                    convertedShapesCount++;
                    if (limitCount > 0 && convertedShapesCount >= limitCount) break;
                }                
            }

            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + "Saving .ref and .bat files");
            await GeneralMethods.SaveStringToFile(shape.ShapeName + ".ref", refFile, DataFileFormat.UTF16LE);
            await GeneralMethods.SaveStringToFile(shape.ShapeName + ".bat", batFile, DataFileFormat.PlainText);

            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + "Starting shape generation");
            await GeneralMethods.RunTasksInParallel(conversionTasks, 16);

            Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + "All shape generation tasks finished. Execute .bat to compress shapes.");
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
