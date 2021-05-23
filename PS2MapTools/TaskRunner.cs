using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTools
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskRunner<T> : IDisposable
    {
        //Action<IConsole, CancellationToken>
        protected readonly ConcurrentQueue<Action<T>> _taskQueue;
        protected readonly List<Task> _runningTasks;

        /// <summary>
        /// Gets the number of currently running tasks. Only update this using volatile operations, such as through the <see cref="Interlocked"/> class.
        /// </summary>
        protected int _runningTaskCount;

        protected Task _taskRunner;

        /// <summary>
        /// Gets the number of currently running tasks.
        /// </summary>
        public int RunningTasks => _runningTaskCount;

        /// <summary>
        /// Gets the number of queued tasks (this excludes running tasks).
        /// </summary>
        public int QueuedTasks => _taskQueue.Count;

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="TaskRunner{T}"/> instance is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="TaskRunner{T}"/> instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        public TaskRunner()
        {
            _taskQueue = new ConcurrentQueue<Action<T>>();
            _runningTasks = new List<Task>();
            _taskRunner = new Task(() => throw new InvalidOperationException("This task runner has not been setup."));

            if (typeof(T).IsSubclassOf(typeof(Action)))
                throw new ArgumentException("Generic type must derive from System.Action");
        }

        public virtual void EnqueueTask(Action<T> action) => _taskQueue.Enqueue(action); // Just accept tasks?

        public virtual void Start(T data, CancellationToken ct, int maxParallelTasks = 4)
        {
            if (IsRunning)
                throw new InvalidOperationException("This instance is already running.");

            ct.Register(() => IsRunning = false);

            _taskRunner = new Task(() =>
            {
                while (IsRunning && !ct.IsCancellationRequested)
                {
                    while (_runningTaskCount < maxParallelTasks && _taskQueue.TryDequeue(out Action<T>? a))
                    {
                        Task t = new(() => a.Invoke(data), TaskCreationOptions.LongRunning);
                        t.ContinueWith((t) =>
                        {
                            _runningTasks.Remove(t);
                            Interlocked.Decrement(ref _runningTaskCount);
                        });
                        _runningTasks.Add(t);
                        t.Start();

                        Interlocked.Increment(ref _runningTaskCount);
                        // Cache tasks and wait on them completing before exiting
                    }

                    Task.Delay(100).Wait();
                }
            }, ct, TaskCreationOptions.LongRunning);

            _taskRunner.Start();
        }

        /// <summary>
        /// Stops this <see cref="TaskRunner{T}"/> instance. Any queued tasks will not be processed, and currently active tasks will be finished.
        /// </summary>
        public virtual void Stop() => IsRunning = false;

        /// <summary>
        /// Waits for all tasks (active + queued) to complete.
        /// </summary>
        /// <returns></returns>
        public virtual async Task WaitForAll()
        {
            while (_runningTaskCount != 0 && !_taskQueue.IsEmpty)
                await Task.Delay(100).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _taskRunner.Dispose();
                }

                _taskQueue.Clear();
                IsDisposed = true;
            }
        }
    }
}
