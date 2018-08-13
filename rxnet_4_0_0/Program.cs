using BenchmarkDotNet.Running;
using System;

namespace rxnet_3
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ShakespearePlaysScrabbleRxNET>();
        }
    }
}
