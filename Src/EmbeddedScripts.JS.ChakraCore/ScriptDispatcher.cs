using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;

// Taken from https://github.com/Taritsyn/JavaScriptEngineSwitcher/blob/master/src/JavaScriptEngineSwitcher.ChakraCore/ScriptDispatcher.cs
namespace EmbeddedScripts.JS.ChakraCore
{
    public struct InterlockedStatedFlag
    {
        private int _counter;

        public bool IsSet()
        {
            return _counter != 0;
        }

        public bool Set()
        {
            return Interlocked.Exchange(ref _counter, 1) == 0;
        }
    }
    
    public struct StatedFlag
    {
        private bool _isSet;

        public bool IsSet()
        {
            return _isSet;
        }

        public bool Set()
        {
            if (_isSet) return false;
            
            _isSet = true;
            return true;
        }
    }
    
    public class ScriptDispatcher : IDisposable
    {
        private Thread _thread;
        private AutoResetEvent _waitHandle = new(false);
        private Queue<ScriptTask> _taskQueue = new();
        private readonly object _queueSync = new();
        private InterlockedStatedFlag _disposedFlag;

        public ScriptDispatcher()
        {
            _thread = new Thread(StartThread) { IsBackground = true };

            _thread.Start();
        }

        public void VerifyNotDisposed()
        {
            if (_disposedFlag.IsSet())
                throw new ObjectDisposedException(ToString());
        }

        private void StartThread()
        {
            while (true)
            {
                ScriptTask task = null;

                lock (_queueSync)
                {
                    if (_taskQueue.Count > 0)
                    {
                        task = _taskQueue.Dequeue();
                        if (task == null)
                        {
                            _taskQueue.Clear();
                            return;
                        }
                    }
                }

                if (task != null)
                    task.Run();
                else
                    _waitHandle.WaitOne();
            }
        }

        private void EnqueueTask(ScriptTask task)
        {
            lock (_queueSync)
                _taskQueue.Enqueue(task);

            _waitHandle.Set();
        }

        private void ExecuteTask(ScriptTask task)
        {
            EnqueueTask(task);
            task.Wait();

            var ex = task.Exception;

            if (ex != null)
                ExceptionDispatchInfo.Capture(ex).Throw();
        }

        public T Invoke<T>(Func<T> func)
        {
            VerifyNotDisposed();

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (Thread.CurrentThread == _thread)
                return func();

            using var task = new ScriptTaskWithResult<T>(func);
            
            ExecuteTask(task);
            return task.Result;
        }

        public void Invoke(Action action)
        {
            VerifyNotDisposed();

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (Thread.CurrentThread == _thread)
                action();

            using var task = new ScriptTaskWithoutResult(action);
            
            ExecuteTask(task);
            
        }

        public void Dispose()
        {
            if (!_disposedFlag.Set()) return;
            
            EnqueueTask(null);

            _thread?.Join();
            _thread = null;

            _waitHandle?.Dispose();
            _waitHandle = null;

            _taskQueue = null;
        }
    }

    internal abstract class ScriptTask : IDisposable
    {
        protected ManualResetEvent _waitHandle = new(false);
        protected Exception _exception;
        protected StatedFlag _disposedFlag = new();

        public Exception Exception => _exception;

        protected void VerifyNotDisposed()
        {
            if (_disposedFlag.IsSet())
                throw new ObjectDisposedException(ToString());
        }

        public abstract void Run();

        public void Wait()
        {
            VerifyNotDisposed();

            _waitHandle.WaitOne();
        }

        public virtual void Dispose()
        {
            _waitHandle?.Dispose();
            _waitHandle = null;
            _exception = null;
        }
    }

    internal class ScriptTaskWithResult<TResult> : ScriptTask
    {
        private Func<TResult> _func;
        private TResult _result;

        public TResult Result => _result;

        public ScriptTaskWithResult(Func<TResult> func)
        {
            _func = func;
        }

        public override void Run()
        {
            VerifyNotDisposed();

            try
            {
                _result = _func();
            }
            catch (Exception e)
            {
                _exception = e;
            }

            _waitHandle.Set();
        }

        public override void Dispose()
        {
            if (!_disposedFlag.Set()) return;
            
            base.Dispose();
            _result = default;
            _func = null;
        }
    }
    
    internal class ScriptTaskWithoutResult : ScriptTask
    {
        private Action _action;

        public ScriptTaskWithoutResult(Action action)
        {
            _action = action;
        }

        public override void Run()
        {
            VerifyNotDisposed();

            try
            {
                _action();
            }
            catch (Exception e)
            {
                _exception = e;
            }

            _waitHandle.Set();
        }

        public override void Dispose()
        {
            if (!_disposedFlag.Set()) return;
            
            base.Dispose();
            _action = null;
        }
    }
}