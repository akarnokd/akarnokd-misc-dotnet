using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace akarnokd_misc_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.Version);
            Console.WriteLine(GetNetCoreVersion());

            BenchmarkRunner.Run<ShakespearePlaysScrabbleAsyncEnumerableDotNet>();

            //BenchmarkRunner.Run<ShakespearePlaysScrabbleRxNET>();
            /*
            ScrabbleBenchmarks();

            Console.WriteLine("Done... Press ENTER to quit");
            Console.ReadLine();
            */
        }

        public static string GetNetCoreVersion()
        {
            var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
                return assemblyPath[netCoreAppIndex + 1];
            return null;
        }
    }
}
