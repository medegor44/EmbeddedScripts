using System;
using Python.Runtime;

namespace EmbeddedScripts.Python.PythonNet
{
    public class PythonMultithreadingScope : IDisposable
    {
        private readonly IntPtr _threadState;

        public PythonMultithreadingScope()
        {
            PythonEngine.Initialize();
            _threadState = PythonEngine.BeginAllowThreads();
        }
        
        public void Dispose()
        {
            PythonEngine.EndAllowThreads(_threadState);
            PythonEngine.Shutdown();
        }
    }
}