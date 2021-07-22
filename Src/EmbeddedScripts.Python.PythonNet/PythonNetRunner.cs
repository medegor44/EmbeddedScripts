using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Python.Runtime;

namespace EmbeddedScripts.Python.PythonNet
{
    public class PythonNetRunner : ICodeRunner
    {
        private readonly Container _container = new();

        public static string PythonDll 
        {
            get => Runtime.PythonDLL;
            set => Runtime.PythonDLL = value;
        }
        
        public Task RunAsync(string code)
        {
            using (new PythonMultithreadingScope())
            using (Py.GIL())
            using (var scope = Py.CreateScope())
                scope
                    .AddHostObjectsFromContainer(_container)
                    .Exec(code);

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);

            return this;
        }
    }
}