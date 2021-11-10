using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using EmbeddedScripts.CSharp.Roslyn.Scripting;
using EmbeddedScripts.JS.ChakraCore;
using EmbeddedScripts.JS.ClearScriptV8;
using EmbeddedScripts.JS.Jint;
using EmbeddedScripts.Lua.Moonsharp;
using EmbeddedScripts.Python.PythonNet;
using EmbeddedScripts.Shared;

namespace InteropBenchmark
{
    public class OneFunctionInteropBenchmark : IDisposable
    {
        private readonly JintCodeRunner _jint = new();
        private readonly ClearScriptV8Runner _v8 = new();
        private readonly ChakraCoreRunner _chakra = new();
        private readonly ScriptCodeRunner _roslynScripting = new();
        private readonly MoonsharpRunner _moonsharp = new();
        private readonly PythonNetRunner _pythonnet = new();
        private const int IterationsCount = 1_000_000;

        private readonly string _jsCode = $@"
for (let i = 0; i < {IterationsCount}; i++)
  add(i, i);
";

        private readonly string _csharpCode = $@"
for (int i = 0; i < {IterationsCount}; i++)
  add(i, i);
";

        private readonly string _luaCode = $@"
for i = 0, {IterationsCount} do
  add(i, i)
end
";

        private readonly string _pyCode = $@"
for i in range({IterationsCount}):
  add(i, i)
";

        public OneFunctionInteropBenchmark()
        {
            var runners = new List<ICodeRunner> { _jint, _v8, _chakra, _roslynScripting, _moonsharp, _pythonnet };
            foreach (var runner in runners)
                runner.Register<Func<int, int, int>>((a, b) => a + b, "add");
        }

        [Benchmark]
        public async Task Jint() => await _jint.RunAsync(_jsCode);
        [Benchmark]
        public async Task V8() => await _v8.RunAsync(_jsCode);
        [Benchmark]
        public async Task ChakraCore() => await _chakra.RunAsync(_jsCode);
        [Benchmark]
        public async Task Roslyn() => await _roslynScripting.RunAsync(_csharpCode);
        [Benchmark]
        public async Task Moonsharp() => await _moonsharp.RunAsync(_luaCode);
        [Benchmark]
        public async Task Pythonnet() => await _pythonnet.RunAsync(_pyCode);

        public void Dispose()
        {
            _v8?.Dispose();
            _chakra?.Dispose();
        }
    }
}
