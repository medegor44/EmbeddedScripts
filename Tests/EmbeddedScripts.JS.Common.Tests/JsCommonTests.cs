using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Xunit;

namespace EmbeddedScripts.JS.Common.Tests
{
    public class JsCommonTests
    {
        public async Task RunValidCode_Succeed(ICodeRunner runner)
        {
            var code = "let a = 1; let b = 2; let c = a + b;";

            await runner.RunAsync(code);
        }
        
        public async Task RunWithTwoGlobalVariables_Succeed(ICodeRunner runner)
        {
            runner 
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("let c = a + b;");
        }
        
        public async Task RunWithGlobalFunc_Succeed(ICodeRunner runner)
        {
            int x = 0;
            var code = "t();";

            runner
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }
        
        public async Task RunInvalidCode_ThrowsException(ICodeRunner runner)
        {
            var code = "vat a = 1;";

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }
        
        public async Task CodeThrowsAnException_ExceptionWithSameMessageIsThrowingFromRunner(ICodeRunner runner)
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw Error('{exceptionMessage}');";

            var actualMessage = (await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(code))).Message;
            
            var expectedMessage = $"Error: {exceptionMessage}";

            Assert.Equal(expectedMessage, actualMessage);
        }

        public async Task ExposedFuncThrowingException_RunnerThrowsException(ICodeRunner runner)
        {
            var exceptionMessage = "message from exception";
            runner.Register<Action>(() =>
                throw new InvalidOperationException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                runner.RunAsync("f()"));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        public async Task RunCodeWithSyntaxError_ThrowsScriptSyntaxErrorException(ICodeRunner runner)
        {
            var code = "if (true {}";

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        public async Task RunCodeWithExceptionHandling_Success(ICodeRunner runner)
        {
            var code = @"
function f() {
    throw Error('oops');
}

try {
    f();
}
catch (err) {}
";
            await runner.RunAsync(code);
        }

        public async Task HandleExceptionFromExposedFunction(ICodeRunner runner)
        {
            var message = "oops";
            var code = @"
try {
    f();
}
catch(err) {
    throw Error('aa');
}
";

            runner.Register<Action>(() => throw new ArgumentException(message), "f");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));
        }

        public async Task HandleExceptionFromExposedFunc_ErrorMessageIsEqualToExceptionMessage(ICodeRunner runner)
        {
            var message = "oops";
            var code = @"
try {
    f();
}
catch(err) {
    assert(err.message);
}
";

            runner
                .Register<Action>(() => throw new ArgumentException(message), "f")
                .Register<Action<string>>(actual => Assert.Equal(message, actual), "assert");

            await runner.RunAsync(code);
        }

        public async Task AddConfigTwice_AddsNewConfig_Succeed(ICodeRunner runner)
        {
            var s = "abc";
            var t = 1;
            var code = "t += s.length;";

            runner.Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }
        
        public async Task RunAsyncWithContinuation_EachRunSharesGlobals_Success(ICodeRunner runner)
        {
            var code = "let x = 0;";

            runner.Register<Func<int, int>>(x => x + 1, "Inc");
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("Check(x);");
        }
        
        public async Task RegisteringNewGlobalVarBetweenRuns_Success(ICodeRunner runner)
        {
            var code = "let x = 0;";

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");
            
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");
            
            await runner.RunAsync("Check(x);");
        }
        
        public async Task RunAsyncWithContinuation_Success(ICodeRunner runner)
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

            await runner.RunAsync(code);
            await runner.RunAsync("incr()");
            await runner.RunAsync("incr()");
            await runner.RunAsync("check()");
        }
        
        public async Task EvaluateAsync_Success(IEvaluator evaluator)
        {
            var result = await evaluator.EvaluateAsync<int>("1 + 2");
            
            Assert.Equal(3, result);
        }

        public async Task EvaluateExpressionWithNetAndJsTypes<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            runner.Register(1, "x");
            await runner.RunAsync("let y = 2;");
            var actual = await runner.EvaluateAsync<int>("x + y");
            
            Assert.Equal(3, actual);
        }

        public async Task EvaluateAsyncString<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var str = "abc";
            runner.Register(str, "str");
            var actual = await runner.EvaluateAsync<string>("str");
            
            Assert.Equal(str, actual);
        }

        public async Task NetAndJsIntegersEquality<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            runner.Register(1, "x");
            await runner.RunAsync("let y = 1;");
            var actual = await runner.EvaluateAsync<bool>("x === y");

            Assert.True(actual);
        }

        public async Task EvaluateAsyncFunctionCall_ReturnsFunctionReturnValue<T>(T runner) 
            where T : ICodeRunner, IEvaluator
        {
            await runner.RunAsync(@"
function GetHello(name) { 
    return 'Hello ' + name; 
}");
            var result = await runner.EvaluateAsync<string>(@"GetHello(""John"")");
            
            Assert.Equal("Hello John", result);
        }
    }
}