using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PS2MapTool
{
    /// <summary>
    /// Runs multiple tasks in parallel, with support for a parallelism limit.
    /// </summary>
    public class ParallelTaskRunner : IDisposable
    {
        /// <summary>
        /// The delay in milliseoncds that waiting threads will pause for.
        /// </summary>
        private const int WAIT_DELAY = 100;

        protected readonly ConcurrentQueue<Task> _taskQueue;
        protected readonly List<Task> _runningTasks;
        protected readonly Action<AggregateException>? _onTaskException;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelTaskRunner"/> object.
        /// </summary>
        public ParallelTaskRunner(Action<AggregateException>? onTaskException = default)
        {
            _onTaskException = onTaskException;

            _taskQueue = new ConcurrentQueue<Task>();
            _runningTasks = new List<Task>();
            _taskRunner = new Task(() => throw new InvalidOperationException("This task runner has not been setup."));
        }

        /// <summary>
        /// Adds a tasks to the processing queue.
        /// </summary>
        /// <param name="task">The task to run.</param>
        public virtual void EnqueueTask(Task task)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ParallelTaskRunner));

            _taskQueue.Enqueue(task);
        }

        /// <summary>
        /// Begins running tasks.
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="maxParallelTasks">The maximum number of tasks that may be run in parallel.</param>
        public virtual void Start(CancellationToken ct, int maxParallelTasks = 4)
        {
            if (IsRunning)
                throw new InvalidOperationException("This instance is already running.");
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ParallelTaskRunner));

            ct.Register(() => IsRunning = false);

            _taskRunner = new Task(() =>
            {
                while (IsRunning && !ct.IsCancellationRequested)
                {
                    while (_runningTaskCount < maxParallelTasks && _taskQueue.TryDequeue(out Task? t))
                    {
                        t.ContinueWith((t) =>
                        {
                            Monitor.Enter(_runningTasks);
                            _runningTasks.Remove(t);
                            Monitor.Exit(_runningTasks);

                            Interlocked.Decrement(ref _runningTaskCount);

                            if (t.Exception is not null && _onTaskException is not null)
                                _onTaskException(t.Exception);
                        }, ct);

                        Interlocked.Increment(ref _runningTaskCount);

                        Monitor.Enter(_runningTasks);
                        _runningTasks.Add(t);
                        Monitor.Exit(_runningTasks);

                        t.Start();
                    }

                    Task.Delay(WAIT_DELAY).Wait();
                }
            }, ct, TaskCreationOptions.LongRunning);

            IsRunning = true;
            _taskRunner.Start();
        }

        /// <summary>
        /// Clears any queued tasks that are yet to be run.
        /// </summary>
        public void ClearTaskQueue() => _taskQueue.Clear();

        /// <summary>
        /// Stops this <see cref="TaskRunner{T}"/> instance. Currently active tasks will be finished.
        /// </summary>
        public virtual void Stop() => IsRunning = false;

        /// <summary>
        /// Waits for all remaining tasks to complete.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the wait operation.</returns>
        public virtual async Task WaitForAll()
        {
            while (_runningTaskCount != 0 || (!_taskQueue.IsEmpty && IsRunning))
                await Task.Delay(WAIT_DELAY).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_runningTaskCount != 0)
                throw new InvalidOperationException("Ensure that this " + nameof(ParallelTaskRunner) + " is stopped, and all remaining tasks have been processed, before disposing of it.");

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
