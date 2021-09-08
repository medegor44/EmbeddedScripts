using System;
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

            if (pathToPythonDll != null && string.IsNullOrEmpty(PythonDll))
                PythonDll = pathToPythonDll;
        }

        public PythonNetRunner()
        {
            using (Py.GIL())
                _scope = Py.CreateScope();
        }
        
        public Task RunAsync(string code)
        {
            using (Py.GIL())
            {
                try
                {
                    _scope.Exec(code);
                }
                catch (PythonException e)
                {
                    throw e.Type.Name switch
                    {
                        ErrorCodes.SyntaxError or ErrorCodes.IndentationError or ErrorCodes.TabError =>
                            new ScriptSyntaxErrorException(e.Message, e),
                        ErrorCodes.SystemError or ErrorCodes.OsError or ErrorCodes.SystemExit =>
                            new ScriptEngineErrorException(e.Message, e),
                        _ => new ScriptRuntimeErrorException(e.Message, e)
                    };
                }
            }

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            using (Py.GIL())
                _scope.Set(alias, obj);
            
            return this;
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            using (Py.GIL())
                return Task.FromResult(_scope.Eval<T>(expression));
        }
        
        public void Dispose()
        {
            using (new PythonMultithreadingScope()) // TODO Workaround for System.InvalidOperationException: This property must be set before runtime is initialized at Python.Runtime.Runtime.set_PythonDLL(String value)
            using (Py.GIL())
                _scope?.Dispose();
        }
    }
}