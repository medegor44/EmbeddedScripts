using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EmbeddedScripts.JS.ChakraCore;
using EmbeddedScripts.JS.ClearScriptV8;
using EmbeddedScripts.JS.Jint;

namespace JsEnginesBenchmarks
{
    public class Md5JsEnginesBenchmark : IDisposable
    {
        private readonly string _code;
        private readonly JintCodeRunner _jint = new();
        private readonly ClearScriptV8Runner _v8 = new();
        private readonly ChakraCoreRunner _chakra = new();

        public Md5JsEnginesBenchmark()
        {
            using var reader = new StreamReader(@"..\..\..\..\..\..\..\TestData\md5.js");
            _code = reader.ReadToEnd();
        }

        [Benchmark]
        public async Task Jint() => await _jint.RunAsync(_code);

        [Benchmark]
        public async Task ChakraCore() => await _chakra.RunAsync(_code);

        [Benchmark]
        public async Task V8() => await _v8.RunAsync(_code);

        public void Dispose()
        {
            _v8?.Dispose();
            _chakra?.Dispose();
        }
    }
}
