using System;
using System.Threading;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Python.Runtime;

namespace EmbeddedScripts.Python.PythonNet
{
    public class PythonNetRunner : ICodeRunner, IEvaluator, IDisposable
    {
        private readonly PyScope _scope;

        public static string PythonDll
        {
            get => Runtime.PythonDLL;
            set => Runtime.PythonDLL = value;
        }

        static PythonNetRunner()
        {
            var pathToPythonDll = Environment.GetEnvironmentVariable("EMBEDDED_SCRIPTS_PYTHON_DLL");
            PythonEngine.DebugGIL = true;

            if (pathToPythonDll != null && string.IsNullOrEmpty(PythonDll))
                PythonDll = pathToPythonDll;
        }

        public PythonNetRunner()
        {
            using (Py.GIL())
                _scope = Py.CreateScope();
        }

        private void ValidateCurrentThread()
        {
            if (Runtime.MainManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                throw new ScriptEngineErrorException("Cannot perform operation from this thread");
        }

        private void ThrowRunnerException(PythonException exception)
        {
            throw exception.Type.Name switch
            {
                ErrorCodes.SyntaxError or ErrorCodes.IndentationError or ErrorCodes.TabError =>
                    new ScriptSyntaxErrorException(exception.Message, exception),
                ErrorCodes.SystemError or ErrorCodes.OsError or ErrorCodes.SystemExit =>
                    new ScriptEngineErrorException(exception.Message, exception),
                _ => new ScriptRuntimeErrorException(exception.Message, exception)
            };
        }

        public Task RunAsync(string code)
        {
            ValidateCurrentThread();

            using (Py.GIL())
            {
                try
                {
                    _scope.Exec(code);
                }
                catch (PythonException e)
                {
                    ThrowRunnerException(e);
                }
            }

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            ValidateCurrentThread();

            using (Py.GIL())
                _scope.Set(alias, obj);

            return this;
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            ValidateCurrentThread();

            var result = default(T);
            using (Py.GIL())
            {
                try
                {
                    result = _scope.Eval<T>(expression);
                }
                catch (PythonException e)
                {
                    ThrowRunnerException(e);
                }
            }

            return Task.FromResult(result);
        }

        public void Dispose()
        {
            using (Py.GIL())
                _scope?.Dispose();

            PythonEngine.Shutdown();
        }
    }
}