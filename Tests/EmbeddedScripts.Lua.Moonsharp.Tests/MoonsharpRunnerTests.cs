using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Xunit;

namespace EmbeddedScripts.Lua.Moonsharp.Tests
{
    public class MoonsharpRunnerTests
    {
        [Fact]
        public async void RunValidCode_Succeed()
        {
            var code = "a = 1; b = 2; c = a + b;";

            var runner = new MoonsharpRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async void RunWithGlobalVariables_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x = t.x + 1;";

            var runner = new MoonsharpRunner()
                .Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task AddConfigOnce_SetsConfig_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x = t.x + 1;";

            var runner = new MoonsharpRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task AddConfigTwice_AddsNewConfig_Succeed()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x = t.x + string.len(s);";

            var runner = new MoonsharpRunner()
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => runner.RunAsync(code));

            runner.Register(t, "t"); 

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RegisterArray_Succeed()
        {
            var code = @"
sum = 0
for entry in arr do
    sum  = sum + entry
end
";
            await new MoonsharpRunner().Register(new[] {1, 2, 3}, "arr").RunAsync(code);
        }

        [Fact]
        public async Task RunWithTwoGlobalVariables_Succeed()
        {
            var runner = new MoonsharpRunner()
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("c = a + b;");
        }

        [Fact]
        public async Task RunWithGlobalFunc_Succeed()
        {
            int x = 0;
            var code = "t();";

            var runner = new MoonsharpRunner()
                .Register<Action>(() => x++, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RegisterFunctionReturningValue_Succeed()
        {
            await new MoonsharpRunner()
                .Register<Func<int, int, int>>((a, b) => a + b, "add")
                .RunAsync("c = add(1, 2)");
        }

        [Fact]
        public async Task RegisteredFuncThrowsExceptions_RunnerThrowsSameException()
        {
            var exceptionMessage = "hello from exception";
            
            var runner = new MoonsharpRunner()
                .Register<Action>(() => 
                    throw new InvalidOperationException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync("f()"));
            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task RunInvalidCode_ThrowsException()
        {
            var code = "1a = 1;";

            var runner = new MoonsharpRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void CodeThrowsAnException_SameExceptionIsThrowingFromRunner()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"error('{exceptionMessage}');";

            var exception = await Assert.ThrowsAsync<ScriptRuntimeErrorException>(() => 
                new MoonsharpRunner().RunAsync(code));

            Assert.Equal(exceptionMessage, exception.InnerException?.Message);
        }

        struct A
        {
            public int X { get; set; }
        }
        [Fact]
        public async void AddStructAsGlobals_Succeed()
        {
            await new MoonsharpRunner()
                .Register(new A(), "a")
                .RunAsync("a.X = a.X + 1");
        }

        [Fact]
        public async Task ExposeList_Success()
        {
            var list = new List<int> {1, 2, 3};
            var code = @"
assert(a[0] == 1)
assert(a[1] == 2)
assert(a[2] == 3)
";

            var runner = new MoonsharpRunner().Register(list, "a");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task ExposeDictionary_Success()
        {
            var code = @"
assert(d['a'] == 1)
assert(d['b'] == 2)
";
            var dict = new Dictionary<string, int> {{"a", 1}, {"b", 2}};
            var runner = new MoonsharpRunner().Register(dict, "d");

            await runner.RunAsync(code);
        }
        
        [Fact]
        public async Task RegisteringNewGlobalVarBetweenRuns_Success()
        {
            var code = "x = 0;";
            var runner = new MoonsharpRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");
            
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");
            
            await runner.RunAsync("Check(x);");
        }
        
        [Fact]
        public async Task RunAsyncWithContinuation_EachRunSharesGlobals_Success()
        {
            var code = "x = 0";
            var runner = new MoonsharpRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x)");
            await runner.RunAsync("x = Inc(x)");
            await runner.RunAsync("Check(x)");
        }

        [Fact]
        public async Task RunAsyncWithContinuation_Success()
        {
            var code = @"
x = 0;
function incr()  
  x = x + 1
end
function check() 
  assert(x == 2)
end";

            var runner = new MoonsharpRunner();
            await runner.RunAsync(code);
            await runner.RunAsync("incr()");
            await runner.RunAsync("incr()");
            await runner.RunAsync("check()");
        }
    }
}