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

            var runner = new ScriptCodeRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ScriptCodeRunner(config => 
                config.Register(t, "t"));

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ScriptCodeRunner()
                .AddConfig(config => 
                    config.Register(t, "t"));

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.Length;";

            var runner = new ScriptCodeRunner()
                .AddConfig(config => 
                    config.Register(s, "s"));

            await Assert.ThrowsAsync<CompilationErrorException>(() => runner.RunAsync(code));

            runner.AddConfig(config => 
                config.Register(t, "t"));

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new ScriptCodeRunner(config => 
                config
                    .Register(1, "a")
                    .Register(2, "b"));

            await runner.RunAsync("var c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new ScriptCodeRunner(config => 
                config.Register<Action>(() => { x++; }, "t"));

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new ScriptCodeRunner();

            await Assert.ThrowsAsync<CompilationErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void RunCodeWithRuntimeError_ThrowsAnException()
        {
            var code = "int a = 1; int b = 2 / (a - a);";

            var runner = new ScriptCodeRunner();

            await Assert.ThrowsAsync<DivideByZeroException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var code = "throw new System.ArgumentException(\"Exception from user code\");";
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                new ScriptCodeRunner().RunAsync(code));

            Assert.Equal("Exception from user code", exception.Message);
        }

        [Fact]
        public async void AddEngineOptionsOnce_SetsOptions_Succeed()
        {
            var code = "Path.Combine(\"a\", \"b\");";

            var runner = new ScriptCodeRunner()
                .AddEngineOptions(opts =>
                    opts.AddImports("System.IO"));

            await runner.RunAsync(code);
        }

        [Fact]
        public async void AddEngineOptionsTwice_AddsNewEngineOptions_Succeed()
        {
            var code = @"
Path.Combine(""a"", ""b""); 
var builder = new StringBuilder();";

            var runner = new ScriptCodeRunner()
                .AddEngineOptions(opts =>
                    opts.AddImports("System.IO"));

            await Assert.ThrowsAsync<CompilationErrorException>(() => runner.RunAsync(code));

            runner.AddEngineOptions(opts =>
                opts.AddImports("System.Text"));

            await runner.RunAsync(code);
        }

        [Fact(Skip = "test is skipped until security question is resolved")]
        public async Task RunWithNotAllowedFunc_Fail()
        {
            var code = "var w = new System.IO.StreamWriter(\"a.txt\"); w.WriteLine(\"Hello\"); w.Close();";

            var runner = new ScriptCodeRunner();

            await Assert.ThrowsAsync<CompilationErrorException>(() => runner.RunAsync(code));
        }
    }
}
