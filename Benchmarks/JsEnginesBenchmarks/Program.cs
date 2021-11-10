using BenchmarkDotNet.Running;

namespace JsEnginesBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<RecursiveFibonacciBenchmark>();
            BenchmarkRunner.Run<Md5JsEnginesBenchmark>();
            BenchmarkRunner.Run<ThreeDimensionalCubeBenchmark>();
        }
    }
}