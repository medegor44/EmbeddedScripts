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
        private readonly InterpreterLockManager _manager;
        
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
            _manager = new();
            
            using (_manager.Lock())
                _scope = Py.CreateScope();
        }

        public Task RunAsync(string code)
        {
            var scope = _manager.Lock();

            try
            {
                _scope.Exec(code);
            }
            catch (PythonException e)
            {
                ThrowRunnerException(e);
            }
            finally
            {
                scope.Dispose();
            }

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            using (_manager.Lock())
                _scope.Set(alias, obj);
            return this;
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            var scope = _manager.Lock();

            var val = default(T);
            
            try
            {
                val = _scope.Eval<T>(expression);
            }
            catch (PythonException e)
            {
                ThrowRunnerException(e);
            }
            finally
            {
                scope.Dispose();
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
            using (_manager.Lock())
                _scope.Dispose();

            _manager.Dispose();
        }
    }
}