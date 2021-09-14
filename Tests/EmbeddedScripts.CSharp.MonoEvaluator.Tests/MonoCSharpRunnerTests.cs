using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared.Exceptions;
using HelperObjects;
using Xunit;

namespace EmbeddedScripts.CSharp.MonoEvaluator.Tests
{
    public class MonoCSharpRunnerTests
    {
        [Fact]
        public async Task RunAsync_ValidCode_Succeed()
        {
            var code = "var a = 1; var b = 2; var c = a + b;";

            var runner = new MonoCSharpRunner();

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task Register_MutateRegisteredVariableInScript_VariableMutatesInOuterScope()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new MonoCSharpRunner()
                .Register(t, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, t.x);
        }

        [Fact]
        public async Task Register_RegisterVariable_Succeed()
        {
            var t = new HelperObject();
            var code = "t.x++;";

            var runner = new MonoCSharpRunner()
                .Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task Register_Register2ndTimes_RegistersVariable()
        {
            var s = "abc";
            var t = new HelperObject();
            var code = "t.x += s.Length;";

            var runner = new MonoCSharpRunner()
                .Register(s, "s");

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));

            runner.Register(t, "t");

            await runner.RunAsync(code);
        }

        [Fact]
        public async Task RunAsync_WithRegisteredVariables_Succeed()
        {
            var runner = new MonoCSharpRunner()
                .Register(1, "a")
                .Register(2, "b");

            await runner.RunAsync("var c = a + b;");
        }

        [Fact]
        public async Task RunAsync_CallRegisteredFunc_FuncCalled()
        {
            int x = 0;
            var code = "t();";

            var runner = new MonoCSharpRunner()
                .Register<Action>(() => { x++; }, "t");

            await runner.RunAsync(code);

            Assert.Equal(1, x);
        }

        [Fact]
        public async Task RunAsync_RunInvalidCode_ThrowsException()
        {
            var code = "a = 1;";

            var runner = new MonoCSharpRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async void RunAsync_RunCodeWithRuntimeError_ThrowsAnException()
        {
            var code = "int a = 1; int b = 2 / (a - a);";

            var runner = new MonoCSharpRunner();

            await Assert.ThrowsAsync<DivideByZeroException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async Task RunAsync_CallRegisteredFuncThrowingException_RunnerThrowsSameException()
        {
            var exceptionMessage = "hello from exception";
            var runner = new MonoCSharpRunner()
                .Register<Action>(() => throw new ArgumentException(exceptionMessage), "f");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                runner.RunAsync("f()"));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async void RunAsync_CodeThrowsAnException_RunnerThrowsSameException()
        {
            var exceptionMessage = "Exception from user code";
            var code = $"throw new System.ArgumentException(\"{exceptionMessage}\");";

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                new MonoCSharpRunner().RunAsync(code));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task Register_CallRegisteredFunc_Succeed()
        {
            await new MonoCSharpRunner()
                .Register<Func<int, int, int>>((a, b) => a + b, "Add")
                .RunAsync("var c = Add(1, 2);");
        }

        [Fact]
        public async Task RunAsync_RunAsyncSequentially_EachRunSharesState()
        {
            var code = "var x = 0;";
            var runner = new MonoCSharpRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");
            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("Check(x);");
        }

        [Fact]
        public async Task Register_RegisterVarBetweenRuns_VarIsVisibleForNextRuns()
        {
            var code = "var x = 0;";
            var runner = new MonoCSharpRunner();

            runner.Register<Func<int, int>>(x => x + 1, "Inc");

            await runner.RunAsync(code);
            await runner.RunAsync("x = Inc(x);");
            await runner.RunAsync("x = Inc(x);");

            runner.Register<Action<int>>(x => Assert.Equal(2, x), "Check");

            await runner.RunAsync("Check(x);");
        }

        [Fact]
        public async Task EvaluateAsync_ValidExpression_Success()
        {
            var runner = new MonoCSharpRunner();
            var result = await runner.EvaluateAsync<int>("1 + 2");

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task EvaluateAsync_InvalidExpression_ThrowsException()
        {
            var runner = new MonoCSharpRunner();
            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.EvaluateAsync<int>("1 + 2)"));
        }
        
        [Fact]
        public async Task EvaluateAsync_ExpressionWithPartialInput_ThrowsException()
        {
            var runner = new MonoCSharpRunner();
            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.EvaluateAsync<int>("(1 + 2"));
        }
        
        [Fact]
        public async Task EvaluateAsync_EvaluateFunctionCall_EvaluatesFunctionReturnValue()
        {
            var runner = new MonoCSharpRunner();

            await runner.RunAsync(@"
static class Functions
{
    public static string GetHello(string name) { return ""Hello "" + name; }
}");
            var result = await runner.EvaluateAsync<string>(@"Functions.GetHello(""John"")");

            Assert.Equal("Hello John", result);
        }

        [Fact]
        public async Task EvaluateAsync_EvaluateVoidFunctionCall_EvaluatesFunctionReturnValue()
        {
            var runner = new MonoCSharpRunner();

            await runner.RunAsync(@"
static class Functions
{
    public static void GetHello(string name) { return; }
}");

            await Assert.ThrowsAsync<ScriptEngineErrorException>(() =>
                runner.EvaluateAsync<string>(@"Functions.GetHello(""John"")"));
        }

        [Fact]
        public async Task EvaluateAsync_TryToEvaluateInappropriateType_ThrowsInvalidCastException()
        {
            var runner = new MonoCSharpRunner();

            await Assert.ThrowsAsync<InvalidCastException>(() => runner.EvaluateAsync<string>("1 + 2"));
        }

        [Fact]
        public async Task RunAsync_ToplevelFunctionDefinition_Fail()
        {
            var code = "int Add(int a, int b) { return a + b; }";

            var runner = new MonoCSharpRunner();

            await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() => runner.RunAsync(code));
        }

        [Fact]
        public async Task RunAsync_DefineFunctionInStaticClass_Succeed()
        {
            var code = @"
static class Funcs 
{ 
  public static int Add(int a, int b) { return a + b; } 
}";

            var runner = new MonoCSharpRunner();

            await runner.RunAsync(code);
            await runner.RunAsync("var c = Funcs.Add(1, 2);");
        }

        [Fact]
        public async Task RunAsync_RunCodeThrowingSyntaxException2ndTime_ThrowsActualException()
        {
            var runner = new MonoCSharpRunner();

            var ex1 = await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() =>
                runner.RunAsync("var sb = new StringBuilder();"));
            Assert.Equal(
                "(1,15): error CS0246: The type or namespace name `StringBuilder' could not be found. " +
                "Are you missing `System.Text' using directive?",
                ex1.Message.Trim());

            var ex2 = await Assert.ThrowsAsync<ScriptSyntaxErrorException>(() =>
                runner.RunAsync("Console.Write(\"hello\")"));
            Assert.Equal("(1,2): error CS0103: The name `Console' does not exist in the current context",
                ex2.Message.Trim());
        }
    }
}