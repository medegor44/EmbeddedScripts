using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
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

            var runner = new ScriptCodeRunner()
                .Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ScriptCodeRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.Length;";

            var runner = new ScriptCodeRunner()
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new ScriptCodeRunner()
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("var c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new ScriptCodeRunner()
                .Register<Action>(() => { x++; }, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new ScriptCodeRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void RunCodeWithRuntimeError_ThrowsAnException()
        {
            var code = "int a = 1; int b = 2 / (a - a);";

            var runner = new ScriptCodeRunner();

            await Assert.ThrowsAsync<DivideByZeroException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async Task ExposedFuncThrowingException_RunnerThrowsSameException()
        {
            var exceptionMessage = "hello from exception";
            var runner = new ScriptCodeRunner()
                .Register<Action>(() => throw new ArgumentException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                runner.RunAsync("f()"));
            
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = @$"throw new System.ArgumentException(""{exceptionMessage}"");";
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                new ScriptCodeRunner().RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
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

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));

            runner.AddEngineOptions(opts =>
                opts.AddImports("System.Text"));

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task ExposeFunc_Succeed()
        {
            await new ScriptCodeRunner()
                .Register<Func<int, int, int>>((a, b) => a + b, "Add")
                .RunAsync("var c = Add(1, 2);");
        }

        [Fact]
        public async Task RunContinueAsync_EachContinueAsyncSharesGlobals_Success()
        {
            var code = @"
var x = 0;
";
            var runner = new ScriptCodeRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.ContinueWithAsync("x = Inc(x);");
            await runner.ContinueWithAsync("x = Inc(x);");
            
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");
            
            await runner.ContinueWithAsync("Check(x);");
        }
        
        [Fact]
        public async Task RunAsync_StartsNewState_AllStatesSharesSameGlobals()
        {
            var code = "var x = 0;";

            var runner = new ScriptCodeRunner();
            
            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.ContinueWithAsync("x = Inc(x);");
            await runner.ContinueWithAsync("x = Inc(x);");
            
            runner.Register<Action<int>>(x => Assert.Equal(1, x), "Check");

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() =>  
                runner.RunAsync("x = Inc(x); Check(x);"));
        }
        
        [Fact]
        public async Task RunContinueAsync_Success()
        {
            var code = @"
var x = 0;
void incr() { 
  x++;
}
void check() {
  if (x != 2)
    throw new Exception(""x is not equal to 2"");
}";

            var runner = new ScriptCodeRunner();
            runner.AddEngineOptions(options => options.AddImports("System"));
            await runner.RunAsync(code);
            await runner.ContinueWithAsync("incr();");
            await runner.ContinueWithAsync("incr();");
            await runner.ContinueWithAsync("check();");
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
