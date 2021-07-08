using System;
using System.Threading.Tasks;
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

            var runner = new JintCodeRunner(code);

            await runner.RunAsync();
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new JintCodeRunner(code, options =>
                options
                    .Register(t, "t"));

            await runner.RunAsync();

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task WithOptions_SetsOptions_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new JintCodeRunner(code)
                .WithOptions(options =>
                    options.Register(t, "t"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task AddOptions_AddsNewOptions_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.length;";

            var runner = new JintCodeRunner(code)
                .WithOptions(options =>
                    options.Register(s, "s"));

            await Assert.ThrowsAsync<JavaScriptException>(runner.RunAsync);

            runner.AddOptions(options => options.Register(t, "t"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new JintCodeRunner("let c = a + b;", options =>
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

            var runner = new JintCodeRunner(code, options =>
                options.Register<Action>(() => 
                    { x++; }, "t"));

            await runner.RunAsync();

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "vat a = 1;";

            var runner = new JintCodeRunner(code);

            await Assert.ThrowsAsync<Esprima.ParserException>(runner.RunAsync);
        }
        
        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var code = "throw new Error('Exception from user code');";

            var exception = await Assert.ThrowsAsync<JavaScriptException>(new JintCodeRunner(code).RunAsync);
            Assert.Equal("Exception from user code", exception.Message);
        }
    }
}
