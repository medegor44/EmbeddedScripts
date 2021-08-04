using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Xunit;

namespace EmbeddedScripts.JS.ClearScriptV8.Tests
{
    public class ClearScriptV8RunnerTests
    {
        [Fact]
        public async void RunValidCode_Succeed()
        {
            var code = "let a = 1; let b = 2; let c = a + b;";

            using var runner = new ClearScriptV8Runner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            using var runner = new ClearScriptV8Runner();
            runner.Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            using var runner = new ClearScriptV8Runner();
            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.length;";

            using var runner = new ClearScriptV8Runner();
            runner.Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            using var runner = new ClearScriptV8Runner();
            runner.Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("let c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            using var runner = new ClearScriptV8Runner();
            runner
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            using var runner = new ClearScriptV8Runner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw Error('{exceptionMessage}');";

            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(async () =>
            {
                using var runner = new ClearScriptV8Runner();
                await runner.RunAsync(code);
            });

            var expected = $"Error: {exceptionMessage}";

            Assert.Equal(expected, exception.InnerException?.Message);
        }

        public struct A
        {
            public int X;
        }

        [Fact]
        public async Task AddStructAsGlobals_Succeed()
        {
            var inner = 0;
            var a = new A { X = 1 };

            using var runner = new ClearScriptV8Runner();
            await runner
                .Register(a, "a")
                .Register<Action<int>>(x => inner = x, "f")
                .RunAsync("a.X++; a['X']++; f(a.X);");

            Assert.Equal(3, inner);
        }

        [Fact]
        public async Task ExposedFuncThrowsException_RunnerThrowsSameException()
        {
            var exceptionMessage = "Hello from exception";

            using var runner = new ClearScriptV8Runner();
            runner
                .Register<Action>(() => throw new ArgumentException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync("f()"));
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task RunContinueAsync_EachContinueAsyncSharesGlobals_Success()
        {
            var code = @"
let x = 0;
";
            using var runner = new ClearScriptV8Runner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.ContinueWithAsync("x = Inc(x);");
            await runner.ContinueWithAsync("x = Inc(x);");

            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.ContinueWithAsync("Check(x);");
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

            using var runner = new ClearScriptV8Runner();
            await runner.RunAsync(code);
            await runner.ContinueWithAsync("incr()");
            await runner.ContinueWithAsync("incr()");
            await runner.ContinueWithAsync("check()");
        }
        [Fact]
        public async Task RunAsync_StartsNewState_AllStatesSharesSameGlobals()
        {
            var code = "let x = 0";

            using var runner = new ClearScriptV8Runner();
            
            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.ContinueWithAsync("x = Inc(x);");
            await runner.ContinueWithAsync("x = Inc(x);");
            
            runner.Register<Action<int>>(x => Assert.Equal(1, x), "Check");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>  
                runner.RunAsync("x = Inc(x); Check(x)"));
        }
    }
}
