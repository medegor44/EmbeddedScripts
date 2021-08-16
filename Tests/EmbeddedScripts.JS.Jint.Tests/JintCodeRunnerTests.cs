using System.Reflection;
using System.Threading.Tasks;
using EmbeddedScripts.JS.Common.Tests;
using EmbeddedScripts.Shared.Exceptions;
using Xunit;
using HelperObject = HelperObjects.HelperObject;

namespace EmbeddedScripts.JS.Jint.Tests
{
    public class JintCodeRunnerTests
    {
        private JsCommonTests _tests = new();

        [Fact]
        public async void RunValidCode_Succeed() => 
            await _tests.RunValidCode_Succeed(new JintCodeRunner()); 
        
        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed() =>
            await _tests.RunWithTwoGlobalVariables_Succeed(new JintCodeRunner());

        [Fact]
        public async Task RunWithGlobalFunc_Succeed() =>
            await _tests.RunWithGlobalFunc_Succeed(new JintCodeRunner());

        [Fact]
        public async Task RunInvalidCode_ThrowsException() =>
            await _tests.RunInvalidCode_ThrowsException(new JintCodeRunner());
        
        [Fact]
        public async Task RunAsyncWithContinuation_EachRunSharesGlobals_Success() => 
            await _tests.RunAsyncWithContinuation_EachRunSharesGlobals_Success(new JintCodeRunner());

        [Fact]
        public async Task RegisteringNewGlobalVarBetweenRuns_Success() => 
            await _tests.RegisteringNewGlobalVarBetweenRuns_Success(new JintCodeRunner());

        [Fact]
        public async Task RunAsyncWithContinuation_Success() =>
            await _tests.RunAsyncWithContinuation_Success(new JintCodeRunner());

        [Fact]
        public async Task EvaluateAsync_Success() =>
            await _tests.EvaluateAsync_Success(new JintCodeRunner());

        [Fact]
        public async Task EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue() =>
            await _tests.EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue(new JintCodeRunner());
        
        [Fact]
        public async Task ExposedFuncThrowingException_RunnerThrowsException() =>
            await _tests.ExposedFuncThrowingException_RunnerThrowsException(new JintCodeRunner());
        
        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed() =>
            await _tests.AddConfigTwice_AddsNewConfig_Succeed(new JintCodeRunner());

        [Fact]
        public async Task CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner() =>
            await _tests.CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner(new JintCodeRunner());
        
        [Fact]
        public async Task RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException() =>
            await _tests.RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException(new JintCodeRunner());

        [Fact]
        public async Task EvaluateExpressionWithNetAndJsTypes() =>
            await _tests.EvaluateExpressionWithNetAndJsTypes(new JintCodeRunner());

        [Fact]
        public async Task NetAndJsIntegersEquality() =>
            await _tests.NetAndJsIntegersEquality(new JintCodeRunner());

        [Fact]
        public async Task RunCodeWithExceptionHandling_Success() =>
            await _tests.RunCodeWithExceptionHandling_Success(new JintCodeRunner());

        [Fact]
        public async Task EvaluateAsyncString() => 
            await _tests.EvaluateAsyncString(new JintCodeRunner());

        [Fact]
        public async Task HandleExceptionFromExposedFunction() =>
            await _tests.HandleExceptionFromExposedFunction(
                new JintCodeRunner().AddEngineOptions(options => options.CatchClrExceptions()));

        [Fact]
        public async Task HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage() =>
            await _tests.HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage(
                new JintCodeRunner().AddEngineOptions(options => options.CatchClrExceptions()));
        
        [Fact]
        public async void MutateRegisteredVariable_Succeed()
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
        public async Task AddEngineOptionsOnce_SetsOptions_Succeed()
        {
            var code = "x = 1";

            var runner = new JintCodeRunner()
                .AddEngineOptions(opts =>
                    opts.Strict());

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));
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
        public async Task EvaluateAsyncScriptObject()
        {
            var runner = new JintCodeRunner();
            dynamic result = await runner.EvaluateAsync<object>("function t() { return {a : 'a'} }; t()");
            
            Assert.Equal("a", result.a);
        }

        [Fact]
        public async Task EndlessRecursion_ThrowsEngineErrorException()
        {
            var runner = new JintCodeRunner();
            runner.AddEngineOptions(opt => opt.LimitRecursion(10));
            await Assert.ThrowsAsync<ScriptEngineErrorException>(() => runner.RunAsync("(function f() { f(); })()"));
        }
    }
}
