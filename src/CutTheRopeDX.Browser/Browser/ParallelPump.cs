using System;
using System.Threading;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Runs bounded-concurrency work over a fixed list of items.</summary>
    /// <remarks>
    /// The cursor and the completion count are shared by every worker, so both move under
    /// <see cref="Interlocked"/>. Plain increments read, add and store as separate steps,
    /// which lets two workers claim one index and leave another item untouched.
    /// </remarks>
    internal static class ParallelPump
    {
        /// <summary>Hands each item to <paramref name="load"/> exactly once.</summary>
        public static async Task RunAsync(
            string[] work, int concurrency, Func<string, Task> load, Action<int> onProgress)
        {
            int next = 0;
            int done = 0;

            async Task RunWorkerAsync()
            {
                while (true)
                {
                    int index = Interlocked.Increment(ref next) - 1;
                    if (index >= work.Length)
                    {
                        return;
                    }

                    await load(work[index]);
                    onProgress(Interlocked.Increment(ref done));
                }
            }

            Task[] workers = new Task[Math.Min(concurrency, work.Length)];
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = RunWorkerAsync();
            }
            await Task.WhenAll(workers);
        }
    }
}
