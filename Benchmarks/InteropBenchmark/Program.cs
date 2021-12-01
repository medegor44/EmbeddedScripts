using BenchmarkDotNet.Running;

namespace InteropBenchmark
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<OneFunctionInteropBenchmark>();
            BenchmarkRunner.Run<MultipleFunctionsInteropBenchmark>();
        }
    }
}