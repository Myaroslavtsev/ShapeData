using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        public static async Task SaveStringToFile(string fileName, string data, DataFileFormat format = DataFileFormat.PlainText)
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

        public static Task RunTasksInParallel(List<(Task task, string taskName)> tasks, int maxThreadCount)
        {
            var taskFactories = tasks.Select(t => (Func<Task>)(async () =>
            {
                Console.WriteLine("Converting " + t.taskName);
                await t.task;
            }));

            return RunTasksInParallel(taskFactories, maxThreadCount);
        }

        public static Task RunTasksInParallel(List<Task> tasks, int maxThreadCount)
        {
            var taskFactories = tasks.Select(t => (Func<Task>)(() => t));
            return RunTasksInParallel(taskFactories, maxThreadCount);
        }

        private static async Task RunTasksInParallel(IEnumerable<Func<Task>> taskFactories, int maxThreadCount)
        {
            var semaphore = new SemaphoreSlim(maxThreadCount);

            var runningTasks = taskFactories.Select(async taskFactory =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await taskFactory();
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            await Task.WhenAll(runningTasks);
        }
    }
}
