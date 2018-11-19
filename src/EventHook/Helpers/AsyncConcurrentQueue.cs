using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventHook.Helpers
{
    /// <summary>
    ///     A concurrent queue facilitating async dequeue with minimal locking
    ///     Assumes single/multi-threaded producer and a single-threaded consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AsyncConcurrentQueue<T>
    {
        /// <summary>
        ///     Backing queue
        /// </summary>
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        /// <summary>
        ///     Wake up any pending dequeue task
        /// </summary>
        private TaskCompletionSource<bool> dequeueTask;
        private SemaphoreSlim @dequeueTaskLock = new SemaphoreSlim(1);
        private CancellationToken taskCancellationToken;

        internal AsyncConcurrentQueue(CancellationToken taskCancellationToken)
        {
            this.taskCancellationToken = taskCancellationToken;
        }

        /// <summary>
        ///     Supports multi-threaded producers
        /// </summary>
        /// <param name="value"></param>
        internal void Enqueue(T value)
        {
            queue.Enqueue(value);

            //signal 
            dequeueTaskLock.Wait();
            dequeueTask?.TrySetResult(true);
            dequeueTaskLock.Release();

        }

        /// <summary>
        ///     Assumes a single-threaded consumer!
        /// </summary>
        /// <returns></returns>
        internal async Task<T> DequeueAsync()
        {
            T result;
            queue.TryDequeue(out result);

            if (result != null)
            {
                return result;
            }

            await dequeueTaskLock.WaitAsync();
            dequeueTask = new TaskCompletionSource<bool>();
            dequeueTaskLock.Release();

            taskCancellationToken.Register(() => dequeueTask.TrySetCanceled());
            await dequeueTask.Task;
            
            queue.TryDequeue(out result);
            return result;
        }
    }
}
