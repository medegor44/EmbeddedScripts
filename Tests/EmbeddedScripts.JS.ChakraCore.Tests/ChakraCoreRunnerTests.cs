using System;
using System.Reflection;
using System.Threading.Tasks;
using EmbeddedScripts.JS.Common.Tests;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace EmbeddedScripts.JS.ChakraCore.Tests
{
    class DisplayTestMethodNameAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            Console.WriteLine("Setup for test '{0}.'", methodUnderTest.Name);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            Console.WriteLine("TearDown for test '{0}.'", methodUnderTest.Name);
        }
    }
    public class ChakraCoreTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly JsCommonTests _tests = new();

        public ChakraCoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async void RunValidCode_Succeed()
        {
            using var runner = new ChakraCoreRunner();

            await _tests.RunValidCode_Succeed(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunWithTwoGlobalVariables_Succeed(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunWithGlobalFunc_Succeed(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunInvalidCode_ThrowsException(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RegisteringNewGlobalVarBetweenRuns_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RegisteringNewGlobalVarBetweenRuns_Success(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunAsyncWithContinuation_EachRunSharesGlobals_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunAsyncWithContinuation_EachRunSharesGlobals_Success(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunAsyncWithContinuation_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunAsyncWithContinuation_Success(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateAsync_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateAsync_Success(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task ExposedActionThrowsException_RunnerThrowsSameException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.ExposedFuncThrowingException_RunnerThrowsException(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.AddConfigTwice_AddsNewConfig_Succeed(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateExpressionWithNetAndJsTypes()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateExpressionWithNetAndJsTypes(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task NetAndJsIntegersEquality()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.NetAndJsIntegersEquality(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateAsyncString()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.EvaluateAsyncString(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunCodeWithExceptionHandling_Success()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.RunCodeWithExceptionHandling_Success(runner);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task HandleExceptionFromExposedFunction()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.HandleExceptionFromExposedFunction(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage(runner);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task HandleCustomException()
        {
            using var runner = new ChakraCoreRunner();
            await _tests.HandleCustomException(runner);
        }

        [DisplayTestMethodNameAttribute]
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

        [DisplayTestMethodNameAttribute]
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

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task CallExposedFuncWithWrongCountOfParameters_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner
                .Register<Func<int, string, int>>((a, b) => a + b.Length, "f");

            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync("f(1)"));
            Assert.Equal("Error: Inappropriate args list", exception?.Message);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task CallExposedFuncWithParameterTypesMismatch_ThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner
                .Register<Func<int, string, int>>((a, b) => a + b.Length, "f");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync("f('1', 1)"));
        }

        [DisplayTestMethodNameAttribute]
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

        [DisplayTestMethodNameAttribute]
        [Fact]
        public void TryToRegisterUnsupportedType_RunnerThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            Assert.Throws<ArgumentException>(() => runner.Register(new HelperObject(), "x"));
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task TryToCallFunctionWithUnsupportedReturnType_RunnerThrowsException()
        {
            using var runner = new ChakraCoreRunner();
            runner.Register<Func<HelperObject>>(() => new HelperObject(), "f");
            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>  runner.RunAsync("f()"));
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task RunnerDispose_DisposesItsGlobalObject()
        {
            using (var runner = new ChakraCoreRunner())
                runner.Register("hello", "s");

            using var newRunner = new ChakraCoreRunner();
            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => newRunner.EvaluateAsync<string>("s"));
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateAsync_RunParallelTasksWithOneRunner_Success()
        {
            var code = @"
function fib(n) {
  if (n === 0 || n === 1)
    return n;
  return fib(n - 1) + fib(n - 2);
}

function start(n, id) {
  log(`start fib ${id}`);
  const res = fib(n);
  log(`end fib ${id}`);

  return res;
}
";

            using var runner = new ChakraCoreRunner();
            runner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");
            
            await runner.RunAsync(code);

            var firstTask = Task.Run(async () =>
                {
                    Task t;
                    lock (runner)
                        t = runner.EvaluateAsync<int>("start(40, 1)");

                    await t;
                }
            );

            var secondTask = Task.Run(async () =>
            {
                Task t;
                lock (runner)
                    t = runner.EvaluateAsync<int>("start(40, 2)");

                await t;
            });

            await Task.WhenAll(firstTask, secondTask);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task Evaluate_WhileEvaluateIsRunningInSeparateTask_Success() // throws Runtime is active on another thread
        {
            var code = @"
function fib(n) {
  if (n === 0 || n === 1)
    return n;
  return fib(n - 1) + fib(n - 2);
}

function start(n, id) {
  log(`start fib ${id}`);
  const res = fib(n);
  log(`end fib ${id}`);

  return res;
}
";

            using var runner = new ChakraCoreRunner();
            await runner.RunAsync(code);
            runner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");

            var task = Task.Run(() =>
            {
                lock (runner)
                    return runner.EvaluateAsync<int>("start(40, 1)");
            });


            Console.WriteLine("before evaluate in same thread");
            lock (runner)
                runner.EvaluateAsync<int>("start(40, 2)");
            Console.WriteLine("after evaluate in same thread");

            //await task;
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateAsync_RunParallelTasksWithEachOnesRunner_Success()
        {
            var code = @"
function fib(n) {
  if (n === 0 || n === 1)
    return n;
  return fib(n - 1) + fib(n - 2);
}

function start(n, id) {
  log(`start fib ${id}`);
  const res = fib(n);
  log(`end fib ${id}`);

  return res;
}
";

            var firstTask = Task.Run(async () =>
            {
                using var runner = new ChakraCoreRunner();
                runner.Register<Action<string>>(s =>
                {
                    _testOutputHelper.WriteLine($"At {DateTime.Now}");
                    _testOutputHelper.WriteLine(s);
                }, "log");
                await runner.RunAsync(code);
                await runner.EvaluateAsync<int>("start(40, 1)");
            });

            var secondTask = Task.Run(async () =>
            {
                using var runner = new ChakraCoreRunner();
                runner.Register<Action<string>>(s =>
                {
                    _testOutputHelper.WriteLine($"At {DateTime.Now}");
                    _testOutputHelper.WriteLine(s);
                }, "log");
                await runner.RunAsync(code);
                await runner.EvaluateAsync<int>("start(40, 2)");
            });
            
            await Task.WhenAll(firstTask, secondTask);
        }
        
        [DisplayTestMethodNameAttribute]
        [Fact]
        public async Task EvaluateAsync_MultipleRunnersInOneThread_Success()
        {
            var code = @"
function fib(n) {
  if (n === 0 || n === 1)
    return n;
  return fib(n - 1) + fib(n - 2);
}

function start(n, id) {
  log(`start fib ${id}`);
  const res = fib(n);
  log(`end fib ${id}`);

  return res;
}
";

            using var firstRunner = new ChakraCoreRunner();
            firstRunner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");
            
            using var secondRunner = new ChakraCoreRunner();
            secondRunner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");

            await firstRunner.RunAsync(code);
            await secondRunner.RunAsync(code);

            await firstRunner.EvaluateAsync<int>("start(40, 1)");
            await secondRunner.EvaluateAsync<int>("start(40, 2)");
        }
    }
}
