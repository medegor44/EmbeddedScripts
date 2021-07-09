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

            var runner = new JintCodeRunner(code, config =>
                config.Register(t, "t"));

            await runner.RunAsync();

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new JintCodeRunner(code)
                .AddConfig(config =>
                    config.Register(t, "t"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.length;";

            var runner = new JintCodeRunner(code)
                .AddConfig(config =>
                    config.Register(s, "s"));

            await Assert.ThrowsAsync<JavaScriptException>(runner.RunAsync);

            runner.AddConfig(config => config.Register(t, "t"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new JintCodeRunner("let c = a + b;", config =>
                config
                    .Register(1, "a")
                    .Register(2, "b"));

            await runner.RunAsync();
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new JintCodeRunner(code, config =>
                config.Register<Action>(() => x++, "t"));

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
        public async void AddEngineOptionsOnce_SetsOptions_Succeed()
        {
            var code = "x = 1";

            var runner = new JintCodeRunner(code)
                .AddEngineOptions(opts =>
                    opts.Strict());

            await Assert.ThrowsAsync<JavaScriptException>(runner.RunAsync);
        }

        [Fact]
        public async void AddEngineOptionsTwice_AddsNewEngineOptions_SucceedD()
        {
            var code = "x = 1";
            var runner = new JintCodeRunner(code)
                .AddEngineOptions(opts => 
                    opts.Strict());

            await Assert.ThrowsAsync<JavaScriptException>(runner.RunAsync);

            runner.AddEngineOptions(opts =>
                opts.Strict(false));

            await runner.RunAsync();
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw new Error('{exceptionMessage}');";

            var exception = await Assert.ThrowsAsync<JavaScriptException>(new JintCodeRunner(code).RunAsync);
            Assert.Equal(exceptionMessage, exception.Message);
        }
    }
}
