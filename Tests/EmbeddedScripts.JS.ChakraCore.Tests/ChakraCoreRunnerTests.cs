using System;
using System.Threading.Tasks;
using EmbeddedScripts.JS.Common.Tests;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Xunit;

namespace EmbeddedScripts.JS.ChakraCore.Tests
{
    public class ChakraCoreTests
    {
        private JsCommonTests _tests = new();
        
        [Fact]
        public async void RunValidCode_Succeed()
        {
            using var runner = new ChakraCoreRunner();

            await _tests.RunValidCode_Succeed(runner);
        }
        
        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunWithTwoGlobalVariables_Succeed(runner);
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunWithGlobalFunc_Succeed(runner);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunInvalidCode_ThrowsException(runner);
        }
        
        [Fact]
        public async Task RegisteringNewGlobalVarBetweenRuns_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RegisteringNewGlobalVarBetweenRuns_Success(runner);
        }

        [Fact]
        public async Task RunAsyncWithContinuation_EachRunSharesGlobals_Success()
        {
            var code = "let x = 0;";
            using var runner = new ChakraCoreRunner();
            await _tests.RunAsyncWithContinuation_EachRunSharesGlobals_Success(runner);
        }

        [Fact]
        public async Task RunAsyncWithContinuation_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunAsyncWithContinuation_Success(runner);
        }
        
        [Fact]
        public async Task EvaluateAsync_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateAsync_Success(runner);
        }
        
        [Fact]
        public async Task EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue(runner);
        }
        
        [Fact]
        public async Task CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner(runner);
        }
        
        [Fact]
        public async Task ExposedActionThrowsException_RunnerThrowsSameException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.ExposedFuncThrowingException_RunnerThrowsException(runner);
        }
        
        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.AddConfigTwice_AddsNewConfig_Succeed(runner);
        }

        [Fact]
        public async Task RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException(runner);
        }
        
        [Fact]
        public async Task EvaluateExpressionWithNetAndJsTypes()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateExpressionWithNetAndJsTypes(runner);
        }
        
        [Fact]
        public async Task NetAndJsIntegersEquality()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.NetAndJsIntegersEquality(runner);
        }
        
        [Fact]
        public async Task EvaluateAsyncString()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateAsyncString(runner);
        }

        [Theory]
        [InlineData(3)]
        [InlineData((byte)3)]
        [InlineData((sbyte)3)]
        [InlineData((short)3)]
        [InlineData((ushort)3)]
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
        public async Task CallExposedFuncWithWrongCountOfParameters_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner
                .Register<Func<int, string, int>>((a, b) => a + b.Length, "f");

            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync("f(1)"));
            Assert.Equal("Error: Inappropriate args list", exception?.Message);
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
        public async Task RunWithGlobalVariables_Succeed()
        {
            int t = 0;
            var code = "t++;";

            using var runner = new ChakraCoreRunner();
            runner
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public void TryToRegisterUnsupportedType_RunnerThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            Assert.Throws<ArgumentException>(() => runner.Register(new HelperObject(), "x"));
        }

        [Fact]
        public async Task TryToCallFunctionWithUnsupportedReturnType_RunnerThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner.Register<Func<HelperObject>>(() => new HelperObject(), "f");
            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>  runner.RunAsync("f()"));
        }
    }
}
