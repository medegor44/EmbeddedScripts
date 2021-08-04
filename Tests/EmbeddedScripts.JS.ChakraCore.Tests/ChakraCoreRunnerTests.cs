using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
using Xunit;

namespace EmbeddedScripts.JS.ChakraCore.Tests
{
    public class ChakraCoreTests
    {
        [Fact]
        public async void RunValidCode_Succeed()
        {
            var code = "let a = 1; let b = 2; let c = a + b;";

            var runner = new ChakraCoreRunner();

            await runner.RunAsync(code);
        }

        [Theory]
        [InlineData(3)]
        [InlineData("3.14")]
        [InlineData(3.14)]
        [InlineData(true)]
        [InlineData(1L)]
        [InlineData(1UL)]
        [InlineData(3.14f)]
        public async Task ExposeHostFunctions_Succeed<T>(T returnValue)
        {
            using var runner = new ChakraCoreRunner();
            await runner
                .Register<Func<T>>(() => returnValue, "f")
                .RunAsync("f()");
        }

        [Fact]
        public async Task ExposeFunctionWithArguments_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await runner
                .Register<Func<int, int, int>>((a, b) => a + b, "add")
                .RunAsync(@"
let a = 1; 
let b = 2; 
let c = add(a, b); 
if (c !== 3) 
    throw new Error();");
        }

        [Fact]
        public async Task ExposedActionThrowsException_RunnerThrowsSameException()
        {
            var exceptionMessage = "hello from exception";

            using var runner = new ChakraCoreRunner();
            runner
                .Register<Action>(() => throw new ArgumentException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync("f()"));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task CallExposedFuncWithWrongCountOfParameters_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner
                .Register<Func<int, string, int>>((a, b) => a + b.Length, "f");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync("f(1)"));
        }

        [Fact]
        public async Task CallExposedFuncWithParameterTypesMismatch_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner
                .Register<Func<int, string, int>>((a, b) => a + b.Length, "f");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync("f('1', 1)"));
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            int t = 0;
            var code = "t++;";

            using var runner = new ChakraCoreRunner();
            runner
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = 1;
            var code = "t++;";

            using var runner = new ChakraCoreRunner();
            runner
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = 1;
            var code = "t += s.length;";

            using var runner = new ChakraCoreRunner();
            runner
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            runner
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("let x = a + b;");

            runner.Register(3, "c");

            await runner.RunAsync("let y = a + b + c;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            using var runner = new ChakraCoreRunner();
            runner
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1";

            using var runner = new ChakraCoreRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw Error('{exceptionMessage}');";

            using var runner = new ChakraCoreRunner();
            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task RunContinueAsync_EachContinueAsyncSharesGlobals_Success()
        {
            var code = @"
let x = 0;
";
            using var runner = new ChakraCoreRunner();

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

            using var runner = new ChakraCoreRunner();
            await runner.RunAsync(code);
            await runner.ContinueWithAsync("incr()");
            await runner.ContinueWithAsync("incr()");
            await runner.ContinueWithAsync("check()");
        }
        
        [Fact]
        public async Task RunAsync_StartsNewState_AllStatesSharesSameGlobals()
        {
            var code = "let x = 0";

            using var runner = new ChakraCoreRunner();
            
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
