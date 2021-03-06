EmbeddedScripts
===============

EmbeddedScripts library provides unified way to run scripts written on C#, JS or Lua using different scripting engines in your .NET 5 application.

Supported engines:
------------------

- C#: [Roslyn's](https://github.com/dotnet/roslyn) CSharp.Scripting and CSharpCompilation and [Mono Evaluator](https://github.com/mono/mono/blob/main/mcs/mcs/eval.cs).
  Mono Evaluator implements some kind of subset of C# with additional language features and limitations. 
  For more informaion refer the official [docs](https://www.mono-project.com/docs/tools+libraries/tools/repl/).
- JS: [Jint](https://github.com/sebastienros/jint), [ChakraCore](https://github.com/chakra-core/ChakraCore), [ClearScriptV8](https://github.com/microsoft/ClearScript)
- Python: [pythonnet](https://github.com/pythonnet/pythonnet)
- Lua: [Moonsharp](https://github.com/moonsharp-devs/moonsharp)

Supported platforms
-------------------

Script engine      | Windows | Linux  | macOS | Android  | iOS
-------------------|---------|--------|-------|----------|----
Roslyn (scripting) | ✔       | ✔     | ?     | ✔¹       | ✔
Roslyn (compiler)  | ✔       | ✔     | ?     | ✔        | ✔
Mono Evaluator⁴    | ✔       | ✔     | ?     | ?        | ?
Jint               | ✔       | ✔     | ?     | ?        | ?
ChakraCore³        | ✔       | ✔     | ?     | ?        | ?
ClearScriptV8³     | ✔       | ✔     | ?     | ?        | ?
Moonsharp          | ✔       | ✔     | ?     | ?        | ?
Pythonnet²         | ✔       | ✔     | ?     | ❌         | ❌

1. Android requires to install additional nuget package `System.Runtime.Loader`.
2. Pythonnet requires installed python in your environment. To setup runner you have to provide path to python library via `PythonNetRunner.PythonDll` static field wich is synonim to PythonDLL static field of pythonnet library. More on pythonnet specifity you can read in [docs](https://github.com/pythonnet/pythonnet/wiki).
3. You have to install native binaries for you operating system. We recommend these binaries for [ChakraCore](https://www.nuget.org/packages?q=JavaScriptEngineSwitcher.ChakraCore.Native) and these for [ClearScriptV8](https://www.nuget.org/packages?q=Microsoft.ClearScript.V8.Native).
4. Mono was added as an experimental engine for comparision with Roslyn scripting. It is not thread safe and doesn't provide all features of C#. We don't recommend it for use in production code.

Basic usage
-----------

Just create one of runners and call `RunCodeAsync` method:

```c#
var code = @"
let a = 1;
let b = 2;
let c = a + b;
";
var runner = new JintCodeRunner();
await runner.RunCodeAsync(code);
```

`ChakraCoreRunner` and `ClearScriptV8Runner` implement `IDisposable` interface, so you need to call `Dispose` method
after usage of them or use them inside `using` scope.

Besides simple script running you can expose some object from .NET to script using `Register` method

```c#
var countOfCalls = 0;
Action func = () => countOfCalls++;

runner.Register<Action>(func, "func");
await runner.RunAsync("func()");
// coutOfCalls will be equal to 1
```

You can chain registration calls

```c#
runner
    .Register<Func<int, int, int>>((a, b) => a + b, "add")
    .Register<Action<string>>(Console.WriteLine, "log");
```

All runners, except `CompiledCodeRunner`, supports expression evaluation using `Evaluate` method.

```c#
var sum = await runner.EvaluateAsync<int>(1 + 2);
// sum will be equal to 3
```

Every runner has its own list of supported .NET types

Script engine      | Supported types
-------------------|------------
Roslyn (scripting) | All
Roslyn (compiler)  | All
Mono Evaluator     | All
Jint               | Check Jint's [readme](https://github.com/sebastienros/jint/blob/main/README.md#net-interoperability)
ChakraCore         | Only primitives (`string`, `bool`, numeric types) and synchronous `Func`/`Action`
ClearScriptV8      | Check ClearScript's [docs](https://microsoft.github.io/ClearScript/Reference/html/M_Microsoft_ClearScript_ScriptEngine_AddHostObject.htm)
Moonsharp          | Check Moonsharp's [docs](https://www.moonsharp.org/objects.html)

and marshalling logic.

Script engine      | Marshaling logic
-------------------|----------------------------------------------
Roslyn (scripting) | One-to-one
Roslyn (compiler)  | One-to-one
Mono Evaluator     | One-to-one
Jint               | Check Jint's [readme](https://github.com/sebastienros/jint/blob/main/README.md#net-interoperability)
ChakraCore         | `string` ↔ `string`, `bool` ↔ `boolean`, numeric types → `number` and `number` → `double` or `int`. You can't marshal JS function to `Action` or `Func` at this moment
ClearScriptV8      | [From .NET to JS](https://microsoft.github.io/ClearScript/Reference/html/M_Microsoft_ClearScript_ScriptEngine_AddHostObject.htm), [From JS to .NET](https://microsoft.github.io/ClearScript/Reference/html/M_Microsoft_ClearScript_ScriptEngine_Evaluate_2.htm)
Moonsharp          | Check Moonsharp's [docs](https://www.moonsharp.org/mapping.html)
Pythonnet          | Check pythonnet [website](http://pythonnet.github.io/)

Not all runners are thread safe, to use some of them with multithreading you have to deal with thread synchronization.

Engine             | Thread safety
-------------------|--------------
Roslyn (scripting) | ✔
Roslyn (compiler)  | ✔
Mono Evaluator     | ❌
Jint               | ❌
ChakraCore         | ✔
ClearScriptV8      | ✔
Moonsharp          | ❌

Performance
-----------

We have written benchmark tests to investigate how fast runners works in some situations such as interoperating calls between .NET and scripting engine, integer and floating point computations. Results of benchmark test you can see [here](Benchmarks/readme.md).
