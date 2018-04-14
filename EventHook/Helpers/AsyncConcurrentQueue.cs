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
        ///     Keeps any pending Dequeue task to wake up once data arrives
        /// </summary>
        private TaskCompletionSource<bool> dequeueTask;

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

            //wake up the dequeue task with result
            dequeueTask?.TrySetResult(true);
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

            dequeueTask = new TaskCompletionSource<bool>();
            taskCancellationToken.Register(() => dequeueTask.TrySetCanceled());
            await dequeueTask.Task;

            queue.TryDequeue(out result);
            return result;
        }
    }
}
