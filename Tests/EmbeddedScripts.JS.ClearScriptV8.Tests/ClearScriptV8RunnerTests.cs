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

            var runner = new ClearScriptV8Runner();

            await runner.RunAsync(code);
        }
        
        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ClearScriptV8Runner()
                .Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new ClearScriptV8Runner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.length;";

            var runner = new ClearScriptV8Runner()
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => 
                runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new ClearScriptV8Runner()
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("let c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new ClearScriptV8Runner()
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new ClearScriptV8Runner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw Error('{exceptionMessage}');";

            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                new ClearScriptV8Runner().RunAsync(code));

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
            var a = new A {X = 1};
            
            await new ClearScriptV8Runner()
                .Register(a, "a")
                .Register<Action<int>>(x => inner = x, "f")
                .RunAsync("a.X++; a['X']++; f(a.X);");

            Assert.Equal(3, inner);
        }

        [Fact]
        public async Task ExposedFuncThrowsException_RunnerThrowsSameException()
        {
            var exceptionMessage = "Hello from exception";
            
            var runner = new ClearScriptV8Runner()
                .Register<Action>(() => throw new ArgumentException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>  runner.RunAsync("f()"));
            Assert.Equal(exceptionMessage, exception.Message);
        }
    }
}
