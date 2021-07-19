using System;
using System.Threading.Tasks;
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
            await new ChakraCoreRunner()
                .Register<Func<T>>(() => returnValue, "f")
                .RunAsync("f()");
        }

        [Fact]
        public async Task ExposeFunctionWithArguments_Succeed()
        {
            await new ChakraCoreRunner()
                .Register<Func<int, int, int>>((a, b) => a + b, "add")
                .RunAsync(@"
let a = 1; 
let b = 2; 
let c = add(a, b); 
if (c !== 3) 
    throw new Error();");
        }

        [Fact]
        public async Task ExposeActionThrowingException()
        {
            var runner = new ChakraCoreRunner()
                .Register<Action>(() => throw new Exception(), "f");

            await Assert.ThrowsAsync<Exception>(() => runner.RunAsync("f()"));
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            int t = 0;
            var code = "t++;";

            var runner = new ChakraCoreRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = 1;
            var code = "t++;";

            var runner = new ChakraCoreRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = 1;
            var code = "t += s.length;";

            var runner = new ChakraCoreRunner()
                .Register(s, "s");

            await Assert.ThrowsAsync<Exception>(() => 
                runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new ChakraCoreRunner();
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

            var runner = new ChakraCoreRunner();
            runner
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new ChakraCoreRunner();

            await Assert.ThrowsAsync<Exception>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw Error('{exceptionMessage}');";

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                new ChakraCoreRunner().RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
        }
    }
}
