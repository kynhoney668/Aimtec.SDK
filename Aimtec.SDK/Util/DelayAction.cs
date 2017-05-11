using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aimtec.SDK.Util
{
    /// <summary>
    /// Runs actions at a later time.
    /// </summary>
    /// <remarks>
    /// The action will be run in a different thread. Ensure your types are thread safe.
    /// </remarks>
    public class DelayAction
    {
        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        public static TaskFactory Scheduler { get; } = new TaskFactory(TaskScheduler.Current);

        /// <summary>
        ///     Queues the action to be run after a specified time.
        /// </summary>
        /// <param name="milliseconds">The time time to delay in milliseconds.</param>
        /// <param name="action">The action.</param>
        /// <returns>A <see cref="CancellationToken" /> that can be used to cancel the action.</returns>
        public static CancellationTokenSource Queue(int milliseconds, Action action)
        {
            var tokenSource = new CancellationTokenSource();
            Queue(milliseconds, action, tokenSource.Token);

            return tokenSource;
        }

        /// <summary>
        ///     Queues the specified action after a specified delay.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <param name="action">The action.</param>
        /// <param name="token">The token.</param>
        public static void Queue(int milliseconds, Action action, CancellationToken token)
        {
            Task.Delay(milliseconds, token).ContinueWith(task => action(), token);
        }
    }
}