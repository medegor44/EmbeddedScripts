using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
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

        public PythonNetRunner()
        {
            using (Py.GIL())
                _scope = Py.CreateScope();
        }
        
        public Task RunAsync(string code)
        {
            using (new PythonMultithreadingScope())
            using (Py.GIL())
            {
                _scope.Exec(code);
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
            using (new PythonMultithreadingScope())
            using (Py.GIL())
                return Task.FromResult(_scope.Eval<T>(expression));
        }
        
        public void Dispose()
        {
            using (new PythonMultithreadingScope())
            using (Py.GIL())
                _scope?.Dispose();
        }
    }
}