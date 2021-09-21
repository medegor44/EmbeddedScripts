using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
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

        public async Task HandleCustomException(ICodeRunner runner)
        {
            var message = "oops";
            var code = @"
try {
    throws();
}
catch (err) {
    assert(err.message);
}
";

            runner
                .Register<Action>(() => throw new DummyException(message), "throws")
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

        public async Task EvaluateArrayOfSinglePrimitiveTypes<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var actual = await runner.EvaluateAsync<int[]>("[1, 2, 3]");
            Assert.Equal(new[] { 1, 2, 3 }, actual);
        }

        public async Task EvaluateArrayOfDifferentPrimitiveTypes<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var actual = await runner.EvaluateAsync<object[]>("[1, 2, '3']");
            Assert.Equal(new object[] { 1, 2, "3" }, actual);
        }

        public async Task EvaluateRegisteredNetType_ShouldBeEqual<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var obj = new HelperObject();
            runner.Register(obj, "obj");
            var actual = await runner.EvaluateAsync<HelperObject>("obj");

            Assert.Equal(obj, actual);
        }

        public async Task EvaluateRegisteredArray_ShouldBeEqual<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var ints = new[] { 1, 2, 3 };
            runner.Register(ints, "ints");
            var actual = await runner.EvaluateAsync<int[]>("ints");

            Assert.Equal(ints, actual);
        }

        public async Task EvaluateNestedArray<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var expected = new[]
            {
                new[] { 1, 2 },
                new[] { 3 }
            };
            var actual = await runner.EvaluateAsync<int[][]>("[[1, 2], [3]]");

            Assert.Equal(expected, actual);
        }

        public async Task EvaluateRegisteredDateTime_ShouldBeEqual<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var dateTime = DateTime.Now;

            runner.Register(dateTime, "dt");
            var actual = await runner.EvaluateAsync<DateTime>("dt");

            Assert.Equal(dateTime, actual);
        }

        public async Task EvaluateNetObjectField<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var obj = new HelperObject { x = 2 };
            runner.Register(obj, "obj");
            var actual = await runner.EvaluateAsync<int>("obj.x");

            Assert.Equal(obj.x, actual);
        }

        public async Task EvaluateIterator<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var enumerable = await runner.EvaluateAsync<IEnumerable<int>>("[1, 2, 3].entries()");
            var actual = enumerable.ToArray();

            Assert.Equal(new[] { 1, 2, 3 }, actual);
        }
        
        public async Task IEnumerableConvertsToIterator<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            IEnumerable<int> enumerable = new[] { 1, 2, 3 };

            runner.Register(enumerable, "iter");
            Assert.True(await runner.EvaluateAsync<bool>("typeof iter[Symbol.iterator] === 'function'"));
        }
        
        public async Task IEnumerableConvertsToIterator_IteratorYieldsEnumerableItems<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            IEnumerable<int> enumerable = new[] { 1, 2, 3 };

            runner.Register(enumerable, "iter").Register<Action<bool>>(Assert.True, "assert");

            await runner.RunAsync(@"
a = [1, 2, 3];
for (let x of a) { 
    const {done, value: [idx, value]} = iter.next();

    assert(!done);
    assert(x === value);
}

const {done, value} = iter.next();
assert(done);
");
        }

        public async Task EvaluateMap<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var expected = new Dictionary<string, int>
            {
                { "a", 1 },
                { "b", 2 }
            };

            var actual = await runner.EvaluateAsync<Dictionary<string, int>>(@"
let m = new Map();
m.set('a', 1);
m.set('b', 2);
m
");

            Assert.Equal(expected, actual);
        }
        
        public async Task EvaluateRegisteredMap_ShouldBeEqual<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var expected = new Dictionary<string, int>
            {
                { "a", 1 },
                { "b", 2 }
            };

            runner.Register(expected, "dict");
            var actual = await runner.EvaluateAsync<Dictionary<string, int>>("dict");

            Assert.Equal(expected, actual);
        }

        public async Task EvaluateSet<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var expected = new HashSet<int> { 1, 2 };

            var actual = await runner.EvaluateAsync<HashSet<int>>(@"
let s = new Set();
s.add(1);
s.add(2);
s
");

            Assert.Equal(expected, actual);
        }
        
        public async Task EvaluateRegisteredSet_ShouldBeEqual<T>(T runner) where T : ICodeRunner, IEvaluator
        {
            var expected = new HashSet<int> { 1, 2 };

            runner.Register(expected, "set");
            var actual = await runner.EvaluateAsync<HashSet<int>>("set");

            Assert.Equal(expected, actual);
        }
        
        public async Task JsFalsyValuesOnEvaluateConvertsToFalse(IEvaluator runner, string falsyValueExpression)
        {
            var actual = await runner.EvaluateAsync<bool>(falsyValueExpression);
            
            Assert.False(actual);
        }

        public async Task JsNonFalsyValuesOnEvaluateConvertsToTrue(IEvaluator runner, string nonFalsyValueExpression)
        {
            var actual = await runner.EvaluateAsync<bool>(nonFalsyValueExpression);
            
            Assert.True(actual);
        }

        public async Task CallIndexerOnRegisteredNetObject_OverridenIndexerCalls(ICodeRunner runner)
        {
            runner
                .Register(new HelperObject(), "obj")
                .Register<Action<bool>>(Assert.True, "assert");

            await runner.RunAsync(@"
obj[2] = 1;
assert(obj.x === 1);
assert(obj.idx === 2);
");
        }
    }
}