using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SqliteCustomFunctionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var tempPath = Path.Combine(exePath, "temp");

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            var testDbFilename = Path.Combine(tempPath, "test.db");

            if (File.Exists(testDbFilename))
                File.Delete(testDbFilename);

            while (true)
            {
                var conn = new SqliteConnection($"Filename={testDbFilename}");

                conn.Open();

                using (new CustomFunction1(conn))
                {
                    using (var comm = conn.CreateCommand())
                    {
                        comm.CommandText = $"SELECT {CustomFunction1.Name}()";

                        var value = comm.ExecuteScalar();

                        if (value == null)
                            Console.WriteLine("Return: (null)");
                        else
                            Console.WriteLine($"Return: {value}");
                    }

                }
                
                conn.Dispose();
            }
        }
    }
}
