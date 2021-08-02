using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Xunit;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation.Tests
{
    public class CompiledCodeRunnerTests
    {
        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new CompiledCodeRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.Length;";

            var runner = new CompiledCodeRunner()
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new CompiledCodeRunner()
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("var c = a + b;");
        }
        [Fact]
        public async Task RunValidCode_Succeed()
        {
            var code = "var a = 1; var b = 2; var c = a + b;";

            var runner = new CompiledCodeRunner();
            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "imt a = 1;";
            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => 
                new CompiledCodeRunner().RunAsync(code));
        }

        [Fact]
        public async void RunCodeWithRuntimeError_ThrowsAnException()
        {
            var code = "int a = 1; int b = 2 / (a - a);";
            await Assert.ThrowsAsync<DivideByZeroException>(() =>
                new CompiledCodeRunner().RunAsync(code));
        }

        [Fact]
        public async Task RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new CompiledCodeRunner()
                .Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new CompiledCodeRunner()
                .Register<Action>(() => { x++; }, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async void RunWithUserDefinedFunction_Success()
        {
            var code = @"
int Add(int a, int b)
{
    return a + b;
}

int x = Add(1, 2);
";

            var runner = new CompiledCodeRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            
            var code = @$"throw new System.ArgumentException(""{exceptionMessage}"");";
            
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                new CompiledCodeRunner().RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task ExposedFuncThrowsException_RunnerThrowsSameException()
        {
            var exceptionMessage = "hello from exception";
            var runner = new CompiledCodeRunner()
                .Register<Action>(() => throw new ArgumentException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync("f();"));
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact(Skip = "test is skipped until security question is resolved")]
        public async Task RunWithNotAllowedFunc_Failed()
        {
            var code = "System.IO.Path.Combine(\"a\",  \"b\");";
            var runner = new CompiledCodeRunner();

            await Assert.ThrowsAsync<Exception>(() => runner.RunAsync(code));
        }
    }
}

