using System;
using Python.Runtime;

namespace EmbeddedScripts.Python.PythonNet
{
    public class InterpreterLockManager : IDisposable
    {
        private IntPtr ThreadState { get; set; }

        public InterpreterLockManager()
        {
            if (!PythonEngine.IsInitialized)
                PythonEngine.Initialize();

            ThreadState = PythonEngine.BeginAllowThreads();
        }

        public InterpreterLockScope Lock() =>
            new(this);
        
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        public class InterpreterLockScope : IDisposable
        {
            private readonly InterpreterLockManager _manager;
            internal InterpreterLockScope(InterpreterLockManager manager)
            {
                _manager = manager;
                PythonEngine.EndAllowThreads(manager.ThreadState);
            }
            
            public void Dispose()
            {
                _manager.ThreadState = PythonEngine.BeginAllowThreads();
            }
        }
    }
}