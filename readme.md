EmbeddedScripts
===============
EmbeddedScripts library provides unified way to run scripts written on C#, JS or Lua using different scripting engines in your .NET 5 application.

Supported engines:
------------------
- C#: [Roslyn's](https://github.com/dotnet/roslyn) CSharp.Scripting and CSharpCompilation and [Mono Evaluator](https://github.com/mono/mono/blob/main/mcs/mcs/eval.cs). 
  Mono Evaluator implements some kind of subset of C# with additional language features and limitations. 
  For more informaion refer the official [docs](https://www.mono-project.com/docs/tools+libraries/tools/repl/).
- JS: [Jint](https://github.com/sebastienros/jint), [ChakraCore](https://github.com/chakra-core/ChakraCore), [ClearScriptV8](https://github.com/microsoft/ClearScript)
- Lua: [Moonsharp](https://github.com/moonsharp-devs/moonsharp)

Supported platforms
-------------------
Script engine      | Windows | Linux  | macOS | Android | iOS
-------------------|---------|--------|-------|---------|----
Roslyn (scripting) | ✔       | ✔     | ?     | ?       | ?
Roslyn (compiler)  | ✔       | ✔     | ?     | ?       | ?
Mono Evaluator     | ✔       | ✔     | ?     | ?       | ?
Jint               | ✔       | ✔     | ?     | ?       | ?
ChakraCore         | ✔       | ✔     | ?     | ?       | ?
ClearScriptV8      | ✔       | ✔     | ?     | ?       | ?
Moonsharp          | ✔       | ✔     | ?     | ?       | ?

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
-------------------|------------
Roslyn (scripting) | One-to-one
Roslyn (compiler)  | One-to-one
Mono Evaluator     | One-to-one
Jint               | Check Jint's [readme](https://github.com/sebastienros/jint/blob/main/README.md#net-interoperability)
ChakraCore         | `string` <-> `string`, `bool` <-> `boolean`, numeric types -> `number` and `number` -> `double` or `int`. You can't marshal JS function to `Action` or `Func` at this moment
ClearScriptV8      | [From .NET to JS](https://microsoft.github.io/ClearScript/Reference/html/M_Microsoft_ClearScript_ScriptEngine_AddHostObject.htm), [From JS to .NET](https://microsoft.github.io/ClearScript/Reference/html/M_Microsoft_ClearScript_ScriptEngine_Evaluate_2.htm)
Moonsharp          | Check Moonsharp's [docs](https://www.moonsharp.org/mapping.html)

