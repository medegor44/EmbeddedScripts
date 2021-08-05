using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Jint.Runtime;
using Xunit;

namespace EmbeddedScripts.JS.Jint.Tests
{
    public class JintCodeRunnerTests
    {
        [Fact]
        public async void RunValidCode_Succeed()
        {
            var code = "let a = 1; let b = 2; let c = a + b;";

            var runner = new JintCodeRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new JintCodeRunner()
                .Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new JintCodeRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.length;";

            var runner = new JintCodeRunner()
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new JintCodeRunner()
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("let c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new JintCodeRunner()
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new JintCodeRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void AddEngineOptionsOnce_SetsOptions_Succeed()
        {
            var code = "x = 1";

            var runner = new JintCodeRunner()
                .AddEngineOptions(opts =>
                    opts.Strict());

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_RunnerThrowsSameException()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw new Error('{exceptionMessage}');";

            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => 
                new JintCodeRunner().RunAsync(code));

            Assert.IsType<JavaScriptException>(exception.InnerException);
            Assert.Equal(exceptionMessage, exception.InnerException?.Message);
        }

        [Fact]
        public async Task ExposedFuncThrowingException_RunnerThrowsException()
        {
            var exceptionMessage = "message from exception";
            var runner = new JintCodeRunner()
                .Register<Action>(() => 
                    throw new InvalidOperationException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                runner.RunAsync("f()"));
            
            Assert.Equal(exceptionMessage, exception.Message);
        }

        struct A
        {
            public int X { get; set; }
        }
        [Fact]
        public async void AddStructAsGlobals_Succeed()
        {
            await new JintCodeRunner()
                .Register(new A(), "a")
                .RunAsync("a.X++;");
        }
        
        [Fact]
        public async Task RunContinueAsync_EachContinueAsyncSharesGlobals_Success()
        {
            var code = "let x = 0;";
            var runner = new JintCodeRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("Check(x);");
        }
        
        [Fact]
        public async Task RegisteringNewGlobalVarBetweenRuns_Success()
        {
            var code = "let x = 0;";
            var runner = new JintCodeRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");
            
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");
            
            await runner.RunAsync("Check(x);");
        }

        [Fact]
        public async Task RunContinueAsync_Success()
        {
            var code = @"
var x = 0;
function incr() { 
  x++;
}
function check() {
  if (x !== 2)
    throw new Error('x is not equal to 2');
}";

            var runner = new JintCodeRunner();
            await runner.RunAsync(code);
            await runner.RunAsync("incr()");
            await runner.RunAsync("incr()");
            await runner.RunAsync("check()");
        }
        
        [Fact]
        public async Task CodeFromContinueAsyncThrowsException_RunnerThrowsException()
        {
            var runner = new JintCodeRunner();

            await runner.RunAsync("var x = 0;");
            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(@"throw new Error('Hello')"));
        }
    }
}
