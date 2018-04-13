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
        /// Keeps a list of pending Dequeue tasks in FIFO order
        /// </summary>
        ConcurrentQueue<TaskCompletionSource<T>> dequeueTasks
            = new ConcurrentQueue<TaskCompletionSource<T>>();

        /// <summary>
        /// Assumes a single threaded producer!
        /// </summary>
        /// <param name="value"></param>
        internal void Enqueue(T value)
        {
            queue.Enqueue(value);

            //Set the earlist waiting Dequeue task 
            TaskCompletionSource<T> task;

            if (dequeueTasks.TryDequeue(out task))
            {
                //return the result
                T result;
                queue.TryDequeue(out result);
                task.SetResult(result);
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

            var tcs = new TaskCompletionSource<T>();
            taskCancellationToken.Register(() => tcs.TrySetCanceled());

            dequeueTasks.Enqueue(tcs);
            result = await tcs.Task;

            return result;
        }

    }
}
