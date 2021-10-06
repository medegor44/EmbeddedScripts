using System;
using System.Threading.Tasks;
using EmbeddedScripts.JS.Common.Tests;
using EmbeddedScripts.Shared.Exceptions;
using Xunit;
using HelperObjects;
using Xunit.Abstractions;

namespace EmbeddedScripts.JS.ClearScriptV8.Tests
{
    public class ClearScriptV8RunnerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly JsCommonTests _tests = new();

        public ClearScriptV8RunnerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task RunValidCode_Succeed()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunValidCode_Succeed(runner);
        }
        
        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunWithTwoGlobalVariables_Succeed(runner);
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunWithGlobalFunc_Succeed(runner);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunInvalidCode_ThrowsException(runner);
        }
        
        [Fact]
        public async Task RegisteringNewGlobalVarBetweenRuns_Success()
        {
            using var runner = new ClearScriptV8Runner();

            await _tests.RegisteringNewGlobalVarBetweenRuns_Success(runner);
        }
        
        [Fact]
        public async Task RunAsyncWithContinuation_EachRunSharesGlobals_Success()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunAsyncWithContinuation_EachRunSharesGlobals_Success(runner);
        }

        [Fact]
        public async Task RunAsyncWithContinuation_Success()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunAsyncWithContinuation_Success(runner);
        }
        
        [Fact]
        public async Task EvaluateAsync_Success()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.EvaluateAsync_Success(runner);
        }

        [Fact]
        public async Task EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue(runner);
        }
        
        [Fact]
        public async Task ExposedFuncThrowsException_RunnerThrowsSameException()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.ExposedFuncThrowingException_RunnerThrowsException(runner);
        }
        
        [Fact]
        public async void CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner(runner);
        }
        
        [Fact]
        public async Task RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException(runner);
        }

        [Fact]
        public async Task EvaluateExpressionWithNetAndJsTypes()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.EvaluateExpressionWithNetAndJsTypes(runner);
        }

        [Fact]
        public async Task NetAndJsIntegersEquality()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.NetAndJsIntegersEquality(runner);
        }

        [Fact]
        public async Task EvaluateAsyncString()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.EvaluateAsyncString(runner);
        }

        [Fact]
        public async Task RunCodeWithExceptionHandling_Success()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.RunCodeWithExceptionHandling_Success(runner);
        }

        [Fact]
        public async Task HandleExceptionFromExposedFunction()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.HandleExceptionFromExposedFunction(runner);
        }
        
        [Fact]
        public async Task HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage(runner);
        }
        
        [Fact]
        public async Task HandleCustomException()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.HandleCustomException(runner);
        }

        [Fact]
        public async Task EvaluateRegisteredNetType_ShouldBeEqual()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.EvaluateRegisteredNetType_ShouldBeEqual(runner);
        }

        [Fact]
        public async Task EvaluateRegisteredArray_ShouldBeEqual()
        {
            using var runner = new ClearScriptV8Runner();
            await _tests.EvaluateRegisteredArray_ShouldBeEqual(runner);
        }

        [Fact]
        public async void MutateRegisteredVariable_Succeed()
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
            using var runner = new ClearScriptV8Runner();
            await _tests.AddConfigTwice_AddsNewConfig_Succeed(runner);
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
        public async Task EvaluateAsyncScriptObject()
        {
            var runner = new ClearScriptV8Runner();
            dynamic result = await runner.EvaluateAsync<object>("function t() { return {a : 'a'} }; t()");
            
            Assert.Equal("a", result.a);
        }
        
        [Fact]
        public async Task RunnerDispose_DisposesItsGlobalObject()
        {
            using (var runner = new ClearScriptV8Runner())
                runner.Register("hello", "s");

            using var newRunner = new ClearScriptV8Runner();
            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => newRunner.EvaluateAsync<string>("s"));
        }
        
        
    }
}
