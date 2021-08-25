 using System;
 using System.Runtime.InteropServices;
 using System.Threading.Tasks;
 using HelperObjects;
 using Xunit;
 using Python.Runtime;

namespace EmbeddedScripts.Python.PythonNet.Tests
{
    public class PythonNetRunnerTests
    {
        public PythonNetRunnerTests()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                PythonNetRunner.PythonDll = "/usr/lib/python3.8/config-3.8-x86_64-linux-gnu/libpython3.8.so";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                PythonNetRunner.PythonDll = "python38.dll";
        }

        [Fact]
        public async void RunValidCode_Succeed()
        {
            var code = @"
a = 1 
b = 2
c = a + b";

            var runner = new PythonNetRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x += 1;";

            var runner = new PythonNetRunner();
            runner.Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x += 1;";

            var runner = new PythonNetRunner();
            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += len(s)";

            var runner = new PythonNetRunner();
            runner.Register(s, "s");

            await Assert.ThrowsAsync<PythonException>(() => runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new PythonNetRunner();
            runner
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new PythonNetRunner();
            runner.Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "1a = 1;";

            var runner = new PythonNetRunner();

            await Assert.ThrowsAsync<PythonException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"raise Exception('{exceptionMessage}');";

            var runner = new PythonNetRunner();
            var exception = await Assert.ThrowsAsync<PythonException>(() =>
                runner.RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task ExposeFuncWithReturnValue_Succeed()
        {
            var code = @"
c = add(1, 2)
assert(c == 3)";

            var runner = new PythonNetRunner();
            await runner
                .Register<Func<int, int, int>>((a, b) => a + b, "add")
                .RunAsync(code);
        }

        [Fact]
        public async Task CallFunctionThrowingException_RunnerThrowsSameException()
        {
            var runner = new PythonNetRunner();
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
        public async Task AddStructAsGlobals_Succeed()
        {
            var runner = new PythonNetRunner();
            await runner
                .Register(new A(), "a")
                .RunAsync("a.X += 1;");
        }
        
        [Fact]
        public async Task Evaluate()
        {
            var runner = new PythonNetRunner();
            var actual = await runner.EvaluateAsync<int>("1 + 1");
            
            Assert.Equal(2, actual);
        }
        
        [Fact]
        public async Task Evaluate1()
        {
            var runner = new PythonNetRunner();
            await runner.RunAsync("a = 1");
            var actual = await runner.EvaluateAsync<int>("a + 1");
            
            Assert.Equal(2, actual);
        }
    }
}