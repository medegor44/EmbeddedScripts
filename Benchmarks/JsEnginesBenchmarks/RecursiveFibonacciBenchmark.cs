using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EmbeddedScripts.JS.ChakraCore;
using EmbeddedScripts.JS.ClearScriptV8;
using EmbeddedScripts.JS.Jint;

namespace JsEnginesBenchmarks
{
    public class RecursiveFibonacciBenchmark : IDisposable
    {
        private readonly JintCodeRunner _jint = new();
        private readonly ClearScriptV8Runner _v8 = new();
        private readonly ChakraCoreRunner _chakra = new();

        private readonly string _code = @"
function fib(n) {
  if (n === 0 || n === 1)
    return n;
  return fib(n - 1) + fib(n - 2);
}
";

        public RecursiveFibonacciBenchmark()
        {
            _jint.RunAsync(_code);
            _v8.RunAsync(_code);
            _chakra.RunAsync(_code);
        }

        [Benchmark]
        public async Task<int> Jint() => await _jint.EvaluateAsync<int>("fib(30)");
        [Benchmark]
        public async Task<int> V8() => await _v8.EvaluateAsync<int>("fib(30)");
        [Benchmark]
        public async Task<int> ChakraCore() => await _chakra.EvaluateAsync<int>("fib(30)");

        public void Dispose()
        {
            _v8?.Dispose();
            _chakra?.Dispose();
        }
    }
}

