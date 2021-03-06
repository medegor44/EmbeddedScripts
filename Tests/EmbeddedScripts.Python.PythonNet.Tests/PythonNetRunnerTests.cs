 using System;
 using System.Runtime.InteropServices;
 using System.Text;
 using System.Threading.Tasks;
 using EmbeddedScripts.Shared.Exceptions;
 using HelperObjects;
 using Xunit;

namespace EmbeddedScripts.Python.PythonNet.Tests
{
    public class PythonNetRunnerTests
    {
        public PythonNetRunnerTests()
        {
            if (!string.IsNullOrEmpty(PythonNetRunner.PythonDll)) return;
            
            // fallback for local machine tests
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                PythonNetRunner.PythonDll = "C:/Python38/python38.dll";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                PythonNetRunner.PythonDll = "/usr/lib/python3.8/config-3.8-x86_64-linux-gnu/libpython3.8.so";
        }

        [Fact]
        public async void RunAsync_RunValidCode_Succeed()
        {
            var code = @"
a = 1 
b = 2
c = a + b";

            using var runner = new PythonNetRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async void RunAsync_RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x += 1;";

            using var runner = new PythonNetRunner();
            runner.Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }
        
        [Fact]
        public async Task Register_TryToAccessUnregisteredVar_ThrowsException()
        {
            var s = "abc";
            var code = "t.x += len(s)";

            using var runner = new PythonNetRunner();
            runner.Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async Task Register_RegisterUnregisteredLaterVarAfterFail_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += len(s)";

            using var runner = new PythonNetRunner();
            runner.Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunAsync_RunWithTwoGlobalVariables_Succeed()
        {
            using var runner = new PythonNetRunner();
            runner
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("c = a + b;");
        }

        [Fact]
        public async Task RunAsync_CallRegisteredFunction_Succeed()
        {
            int x = 0;
            var code = "t();";

            using var runner = new PythonNetRunner();
            runner.Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunAsync_RunInvalidCode_ThrowsException()
        {
            var code = "1a = 1;";

            using var runner = new PythonNetRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void RunAsync_CodeThrowsAnException_RunnerThrowsSameException()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"raise Exception('{exceptionMessage}');";

            using var runner = new PythonNetRunner();
            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() =>
                runner.RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task RunAsync_CallFunctionReturningValue_Succeed()
        {
            var code = @"
c = add(1, 2)
assert(c == 3)";

            using var runner = new PythonNetRunner();
            await runner
                .Register<Func<int, int, int>>((a, b) => a + b, "add")
                .RunAsync(code);
        }

        [Fact]
        public async Task RunAsync_CallFunctionThrowingException_RunnerThrowsSameException()
        {
            using var runner = new PythonNetRunner();
            var message = "Message from exception";
            runner.Register<Action>(() => throw new ArgumentException(message), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => runner.RunAsync("f()"));
            Assert.Equal(message, exception?.Message);
        }

        public class A
        {
            public int X { get; set; }
        }

        [Fact]
        public async Task Register_RegisterStruct_Succeed()
        {
            using var runner = new PythonNetRunner();
            await runner
                .Register(new A(), "a")
                .RunAsync("a.X += 1;");
        }
        
        [Fact]
        public async Task Evaluate_EvaluateValidExpression_Succeed()
        {
            using var runner = new PythonNetRunner();
            var actual = await runner.EvaluateAsync<int>("1 + 1");
            
            Assert.Equal(2, actual);
        }
        
        [Fact]
        public async Task Evaluate_EvaluateWithContinuation_Succeed()
        {
            using var runner = new PythonNetRunner();
            await runner.RunAsync("a = 1");
            var actual = await runner.EvaluateAsync<int>("a + 1");
            
            Assert.Equal(2, actual);
        }

        [Fact]
        public async Task Evaluate_EvaluatePythonFunctionCall_Success()
        {
            using var runner = new PythonNetRunner();

            await runner.RunAsync(@"
def add(a, b):
  return a + b;
");
            var val = await runner.EvaluateAsync<int>("add(1, 2)");
            
            Assert.Equal(3, val);
        }
        
        [Fact]
        public async Task RunAsync_MultipleTimesEachRunSharesGlobals_Success()
        {
            var code = @"x = 0";
            
            using var runner = new PythonNetRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x)");
            await runner.RunAsync("x = Inc(x)");
            await runner.RunAsync("Check(x)");
        }
        
        [Fact]
        public async Task Register_RegisteringNewGlobalVarBetweenRuns_Success()
        {
            using var runner = new PythonNetRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync("x = 0");
            await runner.RunAsync("x = Inc(x)");
            await runner.RunAsync("x = Inc(x)");
            
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");
            
            await runner.RunAsync("Check(x)");
        }
        
        [Fact]
        public async Task RunAsync_RunWithContinuation_Success()
        {
            using var runner = new PythonNetRunner();
            var code = @"
x = 0
def incr():
  global x 
  x += 1

def check():
  global x
  assert(x == 2) 
";

            await runner.RunAsync(code);
            await runner.RunAsync("incr()");
            await runner.RunAsync("incr()");
            await runner.RunAsync("check()");
        }

        [Fact]
        public async Task Evaluate_EvaluateExpressionWithNetAndJsTypes_Succeed()
        {
            using var runner = new PythonNetRunner();

            runner.Register(1, "x");
            await runner.RunAsync("y = 2;");
            var actual = await runner.EvaluateAsync<int>("x + y");
            
            Assert.Equal(3, actual);
        }

        [Fact]
        public async Task Evaluate_EvaluateRegisteredString_Succeed() 
        {
            using var runner = new PythonNetRunner();

            var str = "abc";
            runner.Register(str, "s");
            var actual = await runner.EvaluateAsync<string>("s");
            
            Assert.Equal(str, actual);
        }

        [Fact]
        public async Task Register_NetAndJsIntegers_AreEqual()
        {
            using var runner = new PythonNetRunner();

            runner.Register(1, "x");
            await runner.RunAsync("y = 1;");
            var actual = await runner.EvaluateAsync<bool>("x == y");

            Assert.True(actual);
        }

        [Fact]
        public async Task Evaluate_RegisteredFunctionCall_Succeed() 
        {
            using var runner = new PythonNetRunner();

            runner.Register<Func<string, string>>(name => "Hello " + name, "GetHello");
            var result = await runner.EvaluateAsync<string>(@"GetHello('John')");
            
            Assert.Equal("Hello John", result);
        }

        [Fact]
        public async Task RunAsync_CodeWithMixedIndentation_ThrowsSyntaxError()
        {
            using var runner = new PythonNetRunner();

            var code = new StringBuilder()
                .AppendLine("def f():")
                .AppendLine("  a = 1")
                .AppendLine("\tb = 2")
                .AppendLine("  return a + b")
                .AppendLine("c = f()")
                .ToString();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }
        
        [Fact]
        public async Task RunAsync_CodeWithInconsistentIndentation_ThrowsSyntaxError()
        {
            using var runner = new PythonNetRunner();

            var code = new StringBuilder()
                .AppendLine("def f():")
                .AppendLine("  a = 1")
                .AppendLine("    b = 2")
                .AppendLine("  return a + b")
                .AppendLine("c = f()")
                .ToString();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }
        
        [Fact]
        public async Task EvaluateAsync_EvaluationInTaskRun_Success()
        {
            var code = @"
def fib(n):
  if n == 0 or n == 1:
    return n
  return fib(n - 1) + fib(n - 2)
";

            using var runner = new PythonNetRunner();
            await runner.RunAsync(code);

            Task.Run(async () =>
            {
                await Assert.ThrowsAsync<ScriptEngineErrorException>(() => runner.EvaluateAsync<int>("fib(20)"));
            }).Wait();
        }

        [Fact]
        public async Task EvaluateAsync_TryToAccessPythonVarFromOtherRunner_Fail()
        {
            using var runner1 = new PythonNetRunner();
            using var runner2 = new PythonNetRunner();

            await runner1.RunAsync("a = 1");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner2.EvaluateAsync<int>("a"));
        }
    }
}