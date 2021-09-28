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
        private readonly int _assignedThreadId = Thread.CurrentThread.ManagedThreadId;
        
        public static string PythonDll 
        {
            get => Runtime.PythonDLL;
            set => Runtime.PythonDLL = value;
        }

        static PythonNetRunner()
        {
            PythonEngine.DebugGIL = true;
            var pathToPythonDll = Environment.GetEnvironmentVariable("EMBEDDED_SCRIPTS_PYTHON_DLL");

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
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;

            if (currentThreadId != _assignedThreadId)
                throw new ScriptEngineErrorException("This runner is assigned to different thread, cannot perform  from other thread");
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

            var val = default(T);

            using (Py.GIL())
            {
                try
                {
                    val = _scope.Eval<T>(expression);
                }
                catch (PythonException e)
                {
                    ThrowRunnerException(e);
                }
            }

            return Task.FromResult(val);
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

        public void Dispose()
        {
            ValidateCurrentThread();

            using (Py.GIL())
                _scope.Dispose();
        }
    }
}