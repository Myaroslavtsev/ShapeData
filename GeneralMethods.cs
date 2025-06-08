using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData
{
    public enum DataFileFormat
    {
        PlainText,
        UTF16LE
    }

    static class GeneralMethods
    {
        public static bool RemoveListItems<T>(List<T> list, Predicate<T> condition)
        {
            var oddItems = list.Find(condition);
            if (oddItems != null)
            {
                list.Remove(oddItems);
                return true;
            }
            return false;
        }

        public static async Task<string> ReadFileToString(string fileName)
        {
            using StreamReader reader = new(fileName);
            return await reader.ReadToEndAsync();
        }

        public static async Task SaveStringToFile(string fileName, string data, DataFileFormat format)
        {
            switch (format)
            {
                case DataFileFormat.PlainText:
                    {
                        using StreamWriter writer = new(fileName);
                        await writer.WriteAsync(data);
                        break;
                    }

                case DataFileFormat.UTF16LE:
                    {
                        using StreamWriter writer = 
                            new(fileName, false, new UnicodeEncoding(false, true)); // == Encoding.Unicode
                        await writer.WriteAsync(data);
                        break;
                    }
            }            
        }
    }
}
