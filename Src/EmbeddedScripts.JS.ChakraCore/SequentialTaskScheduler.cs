using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedScripts.JS.ChakraCore
{
    // Since multiple thread couldn't access single chakra core runtime in the same time, there is an implementation of task scheduler
    // to execute task sequentially
    // Taken from msdn https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=net-5.0
    public class SequentialTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        private readonly LinkedList<Task> _tasks = new();
        private int _delegatesQueuedOrRunning;

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                _currentThreadIsProcessingItems = true;
                try
                {
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            if (_tasks.Count != 0)
                            {
                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }
                            else
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }
                        }

                        TryExecuteTask(item);
                    }
                }
                finally
                {
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }
        
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken)
                    return _tasks;
                else
                    throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_tasks);
            }
        }

        protected override void QueueTask(Task task)
        {
            lock (_tasks)
            {
                _tasks.AddLast(task);
                
                if (_delegatesQueuedOrRunning >= 1) 
                    return;
                
                ++_delegatesQueuedOrRunning;
                NotifyThreadPoolOfPendingWork();
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_currentThreadIsProcessingItems)
                return false;

            if (taskWasPreviouslyQueued)
                return TryDequeue(task) && TryExecuteTask(task);
            return TryExecuteTask(task);
        }
        
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) 
                return _tasks.Remove(task);
        }
    }
}