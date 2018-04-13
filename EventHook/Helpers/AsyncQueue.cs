using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventHook.Helpers
{
    /// <summary>
    /// A concurrent queue facilitating async dequeue with minimal locking
    /// Assumes single/multi-threaded producer and a single-threaded consumer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AsyncQueue<T>
    {
        private CancellationToken taskCancellationToken;
        internal AsyncQueue(CancellationToken taskCancellationToken)
        {
            this.taskCancellationToken = taskCancellationToken;
        }

        /// <summary>
        /// Backing queue
        /// </summary>
        ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        /// <summary>
        /// Keeps any pending Dequeue task to wake up once data arrives
        /// </summary>
        TaskCompletionSource<bool> dequeueTask;

        /// <summary>
        /// Supports multi-threaded producers
        /// </summary>
        /// <param name="value"></param>
        internal void Enqueue(T value)
        {
            queue.Enqueue(value);

            //wake up the dequeue task with result
             dequeueTask?.TrySetResult(true);
            
        }

        /// <summary>
        /// Assumes a single-threaded consumer!
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
            dequeueTask = null;

            queue.TryDequeue(out result);
            return result;
        }

    }
}
