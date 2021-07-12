using System;
using System.Threading.Tasks;
using HelperObjects;
using Microsoft.CodeAnalysis.Scripting;
using Xunit;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting.Tests
{
    public class ScriptCodeRunnerTests
    {
        [Fact]
        public async Task RunValidCode_Succeed()
        {
            var code = "var a = 1; var b = 2; var c = a + b;";

            var runner = new ScriptCodeRunner(code);

            await runner.RunAsync();
        }

        [Fact]
        public async Task RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ScriptCodeRunner(code, options => 
                options
                    .Register(t, "t"));

            await runner.RunAsync();

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task WithConfig_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ScriptCodeRunner(code)
                .AddConfig(config => 
                    config.Register(t, "t"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task AddConfig_AddsConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.Length;";

            var runner = new ScriptCodeRunner(code)
                .AddConfig(config => 
                    config.Register(s, "s"));

            await Assert.ThrowsAsync<CompilationErrorException>(runner.RunAsync);

            runner.AddConfig(options => options.Register(t, "t"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new ScriptCodeRunner("var c = a + b;", options => 
                options
                    .Register(1, "a")
                    .Register(2, "b"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new ScriptCodeRunner(code, options => 
                options.Register<Action>(() => { x++; }, "t"));

            await runner.RunAsync();

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new ScriptCodeRunner(code);

            await Assert.ThrowsAsync<CompilationErrorException>(runner.RunAsync);
        }

        [Fact]
        public async void RunCodeWithRuntimeError_ThrowsAnException()
        {
            var code = "int a = 1; int b = 2 / (a - a);";

            var runner = new ScriptCodeRunner(code);

            await Assert.ThrowsAsync<DivideByZeroException>(runner.RunAsync);
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var code = "throw new System.ArgumentException(\"Exception from user code\");";
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(new ScriptCodeRunner(code).RunAsync);
            Assert.Equal("Exception from user code", exception.Message);
        }

        [Fact]
        public async void WithEngineOptions_SetsNewEngineOptions()
        {
            var code = "Path.Combine(\"a\", \"b\");";

            var runner = new ScriptCodeRunner(code)
                .AddEngineOptions(opts =>
                    opts.AddImports("System.IO"));

            await runner.RunAsync();
        }

        [Fact]
        public async void AddEngineOptions_AddsNewEngineOptions()
        {
            var code = @"
Path.Combine(""a"", ""b""); 
var builder = new StringBuilder();";

            var runner = new ScriptCodeRunner(code)
                .AddEngineOptions(opts =>
                    opts.AddImports("System.IO"));

            await Assert.ThrowsAsync<CompilationErrorException>(runner.RunAsync);

            runner.AddEngineOptions(opts =>
                opts.AddImports("System.Text"));

            await runner.RunAsync();
        }

        [Fact(Skip = "test is skipped until security question is resolved")]
        public async Task RunWithNotAllowedFunc_Fail()
        {
            var code = "var w = new System.IO.StreamWriter(\"a.txt\"); w.WriteLine(\"Hello\"); w.Close();";

            var runner = new ScriptCodeRunner(code);

            await Assert.ThrowsAsync<CompilationErrorException>(runner.RunAsync);
        }
    }
}
