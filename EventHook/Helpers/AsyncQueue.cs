using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventHook.Helpers
{
    /// <summary>
    /// A concurrent queue facilitating async dequeue
    /// Since our consumer is always single threaded no locking is needed
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
        TaskCompletionSource<T> dequeueTask;

        /// <summary>
        /// Assumes a single threaded producer!
        /// </summary>
        /// <param name="value"></param>
        internal void Enqueue(T value)
        {
            queue.Enqueue(value);

            //wake up the dequeue task with result
            if (dequeueTask != null 
                && !dequeueTask.Task.IsCompleted)
            {
                T result;
                queue.TryDequeue(out result);
                dequeueTask.SetResult(result);
            }

        }

        /// <summary>
        /// Assumes a single threaded consumer!
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

            dequeueTask = new TaskCompletionSource<T>();
            taskCancellationToken.Register(() => dequeueTask.TrySetCanceled());
            result = await dequeueTask.Task;
            dequeueTask = null;

            return result;
        }

    }
}
