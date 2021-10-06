using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting.Tests
{
    public class ScriptCodeRunnerMultithreadingTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private string _code = @"
long fib(int n)
{
    if (n == 0 || n == 1)
      return n;
    return fib(n - 1) + fib(n - 2);
}

long start(int n, int id)
{
    log($""start fib {id}"");
    long res = fib(n);
    log($""end fib {id}"");
    return res;
}
";
        public ScriptCodeRunnerMultithreadingTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task EvaluateAsync_RunParallelTasksWithOneRunner_Success()
        {
            var runner = new ScriptCodeRunner();
            runner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");
            
            await runner.RunAsync(_code);

            var firstTask = Task.Run(async () =>
            {
                var t = runner.EvaluateAsync<long>("start(40, 1)");
                await t;
            });

            var secondTask = Task.Run(async () =>
            {
                var t = runner.EvaluateAsync<long>("start(40, 2)");
                await t;
            });

            await Task.WhenAll(firstTask, secondTask);
        }

        [Fact]
        public async Task Evaluate_WhileEvaluateIsRunningInSeparateTask_Success() 
        {
            var runner = new ScriptCodeRunner();
            runner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");
            
            await runner.RunAsync(_code);

            var task = Task.Run(() => runner.EvaluateAsync<long>("start(40, 1)"));

            _testOutputHelper.WriteLine("before evaluate in same thread");
            
            Task t = runner.EvaluateAsync<long>("start(40, 2)");
            
            _testOutputHelper.WriteLine("after evaluate in same thread");

            await Task.WhenAll(t, task);
        }

        [Fact]
        public async Task EvaluateAsync_RunParallelTasksWithEachOnesRunner_Success()
        {
            var firstTask = Task.Run(async () =>
            {
                var runner = new ScriptCodeRunner();
                runner.Register<Action<string>>(s =>
                {
                    _testOutputHelper.WriteLine($"At {DateTime.Now}");
                    _testOutputHelper.WriteLine(s);
                }, "log");
                
                await runner.RunAsync(_code);
                await runner.EvaluateAsync<long>("start(40, 1)");
            });

            var secondTask = Task.Run(async () =>
            {
                var runner = new ScriptCodeRunner();
                runner.Register<Action<string>>(s =>
                {
                    _testOutputHelper.WriteLine($"At {DateTime.Now}");
                    _testOutputHelper.WriteLine(s);
                }, "log");
                
                await runner.RunAsync(_code);
                await runner.EvaluateAsync<long>("start(40, 2)");
            });

            await Task.WhenAll(firstTask, secondTask);
        }

        [Fact]
        public async Task EvaluateAsync_MultipleRunnersInOneThread_Success()
        {
            var firstRunner = new ScriptCodeRunner();
            firstRunner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");
            
            var secondRunner = new ScriptCodeRunner();
            secondRunner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");

            await firstRunner.RunAsync(_code);
            await secondRunner.RunAsync(_code);

            await firstRunner.EvaluateAsync<long>("start(40, 1)");
            await secondRunner.EvaluateAsync<long>("start(40, 2)");
        }
    }
}