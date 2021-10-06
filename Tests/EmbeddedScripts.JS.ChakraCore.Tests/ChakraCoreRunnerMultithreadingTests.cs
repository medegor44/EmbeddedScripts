using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EmbeddedScripts.JS.ChakraCore.Tests
{
    public class ChakraCoreRunnerMultithreadingTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ChakraCoreRunnerMultithreadingTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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
                var t = runner.EvaluateAsync<int>("start(40, 1)");

                await t;
            });

            var secondTask = Task.Run(async () =>
            {
                var t = runner.EvaluateAsync<int>("start(40, 2)");

                await t;
            });

            Task.WaitAll(firstTask, secondTask);
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
                return runner.EvaluateAsync<int>("start(40, 1)");
            });

            Console.WriteLine("before evaluate in same thread");
            Task t = runner.EvaluateAsync<int>("start(40, 2)");
            Console.WriteLine("after evaluate in same thread");

            Task.WaitAll(t, task);
        }

        [DisplayTestMethodNameAttribute]
        [Fact]
        public void EvaluateAsync_RunParallelTasksWithEachOnesRunner_Success()
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
                    Console.WriteLine($"At {DateTime.Now}");
                    Console.WriteLine(s);
                }, "log");
                await runner.RunAsync(code);
                await runner.EvaluateAsync<int>("start(40, 1)");
            });

            var secondTask = Task.Run(async () =>
            {
                using var runner = new ChakraCoreRunner();
                runner.Register<Action<string>>(s =>
                {
                    Console.WriteLine($"At {DateTime.Now}");
                    Console.WriteLine(s);
                }, "log");
                await runner.RunAsync(code);
                await runner.EvaluateAsync<int>("start(39, 2)");
            });
            
            Task.WaitAll(firstTask, secondTask);
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