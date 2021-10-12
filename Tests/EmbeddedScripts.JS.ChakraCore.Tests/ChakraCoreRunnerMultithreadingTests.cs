﻿using System;
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

            await Task.WhenAll(firstTask, secondTask);
        }

        [Fact]
        public async Task Evaluate_WhileEvaluateIsRunningInSeparateTask_Success() 
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

            var task = Task.Run(() => runner.EvaluateAsync<int>("start(40, 1)"));

            Task t = runner.EvaluateAsync<int>("start(40, 2)");

            await Task.WhenAll(t, task);
        }

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
                await runner.EvaluateAsync<int>("start(39, 2)");
            });

            await Task.WhenAll(firstTask, secondTask);
        }

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