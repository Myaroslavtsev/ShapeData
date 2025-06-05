using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShapeData
{
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
            using System.IO.StreamReader reader = new(fileName);
            return await reader.ReadToEndAsync();
        }

        public static async Task SaveStringToFile(string fileName, string data)
        {
            using System.IO.StreamWriter writer = new(fileName, false);
            await writer.WriteAsync(data);
        }
    }
}
