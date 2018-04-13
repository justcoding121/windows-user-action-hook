using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventHook.Helpers
{
    /// <summary>
    /// A concurrent queue facilitating async without locking
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
        ConcurrentQueue<TaskResult> queue = new ConcurrentQueue<TaskResult>();

        /// <summary>
        /// Keeps a list of pending Dequeue tasks in FIFO order
        /// </summary>
        ConcurrentQueue<TaskCompletionSource<TaskResult>> dequeueTasks 
            = new ConcurrentQueue<TaskCompletionSource<TaskResult>>();

        internal void Enqueue(T value)
        {
            queue.Enqueue(new TaskResult() { success = true, Data = value });

            //Set the earlist waiting Dequeue task 
            TaskCompletionSource<TaskResult> task;
            if(dequeueTasks.TryDequeue(out task))
            {
                TaskResult result;
                //if dequeue failed it means another Task picked up the data
                //set the result to false for this Task so that it will be retried
                //otherwise return the result
                if(queue.TryDequeue(out result))
                {
                    task.SetResult(result);
                }
                else
                {
                    task.SetResult(new TaskResult() { success = false });
                }
             
            }

        }

        internal async Task<T> DequeueAsync()
        {
            TaskResult result;
            queue.TryDequeue(out result);

            //try until we get a result
            while (result == null || !result.success)
            {
                var tcs = new TaskCompletionSource<TaskResult>();
                //cancel the task if cancellation token was invoked
                //will throw exception on await below if task was running when cancelled
                taskCancellationToken.Register(() => tcs.TrySetCanceled());

                dequeueTasks.Enqueue(tcs);
                result = await tcs.Task;
            }

            return result.Data;
        }

        /// <summary>
        /// To keep the dequeue result status
        /// </summary>
        internal class TaskResult
        {
            internal bool success { get; set; }
            internal T Data { get; set; }
        }
    }
}
