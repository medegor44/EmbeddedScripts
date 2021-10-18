using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EmbeddedScripts.JS.ClearScriptV8.Tests
{
    public class ClearScriptV8RunnerMultithreadingTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private string _code = @"
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

        public ClearScriptV8RunnerMultithreadingTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task EvaluateAsync_RunParallelTasksWithOneRunner_Success()
        {
            using var runner = new ClearScriptV8Runner();
            runner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");

            await runner.RunAsync(_code);

            var firstTask = Task.Run(async () =>
            {
                var t = runner.EvaluateAsync<int>("start(40, 1)");

                await t;
            });

            var secondTask = Task.Run(async () =>
            {
                var t = runner.EvaluateAsync<int>("start(40, 2)");

                await t;
            });

            await Task.WhenAll(firstTask, secondTask);
        }

        [Fact]
        public async Task Evaluate_WhileEvaluateIsRunningInSeparateTask_Success() // throws Runtime is active on another thread
        {
            using var runner = new ClearScriptV8Runner();
            await runner.RunAsync(_code);
            runner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");

            var task = Task.Run(() =>
            {
                return runner.EvaluateAsync<int>("start(40, 1)");
            });

            _testOutputHelper.WriteLine("before evaluate in same thread");
            Task t = runner.EvaluateAsync<int>("start(40, 2)");
            _testOutputHelper.WriteLine("after evaluate in same thread");

            await Task.WhenAll(t, task);
        }

        [Fact]
        public async Task EvaluateAsync_RunParallelTasksWithEachOnesRunner_Success()
        {
            var firstTask = Task.Run(async () =>
            {
                using var runner = new ClearScriptV8Runner();

                runner.Register<Action<string>>(s =>
                {
                    _testOutputHelper.WriteLine($"At {DateTime.Now}");
                    _testOutputHelper.WriteLine(s);
                }, "log");
                await runner.RunAsync(_code);
                await runner.EvaluateAsync<int>("start(40, 1)");
            });

            var secondTask = Task.Run(async () =>
            {
                using var runner = new ClearScriptV8Runner();
                runner.Register<Action<string>>(s =>
                {
                    _testOutputHelper.WriteLine($"At {DateTime.Now}");
                    _testOutputHelper.WriteLine(s);
                }, "log");
                await runner.RunAsync(_code);
                await runner.EvaluateAsync<int>("start(40, 2)");
            });
            
            await Task.WhenAll(firstTask, secondTask);
        }
        
        [Fact]
        public async Task EvaluateAsync_MultipleRunnersInOneThread_Success()
        {
            using var firstRunner = new ClearScriptV8Runner();
            firstRunner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");
            
            using var secondRunner = new ClearScriptV8Runner();
            secondRunner.Register<Action<string>>(s =>
            {
                _testOutputHelper.WriteLine($"At {DateTime.Now}");
                _testOutputHelper.WriteLine(s);
            }, "log");

            await firstRunner.RunAsync(_code);
            await secondRunner.RunAsync(_code);

            await firstRunner.EvaluateAsync<int>("start(40, 1)");
            await secondRunner.EvaluateAsync<int>("start(40, 2)");
        }
    }
}