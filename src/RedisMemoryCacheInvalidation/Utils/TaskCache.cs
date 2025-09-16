using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation.Utils
{
    /// <summary>
    /// Provides cached task instances for common return values.
    /// </summary>
    internal class TaskCache
    {
        /// <summary>
        /// A cached task that returns true.
        /// </summary>
        public static readonly Task<bool> AlwaysTrue = MakeTask(true);
        
        /// <summary>
        /// A cached task that returns false.
        /// </summary>
        public static readonly Task<bool> AlwaysFalse = MakeTask(false);
        
        /// <summary>
        /// A cached task that returns null.
        /// </summary>
        public static readonly Task<object> Empty = MakeTask<object>(null);

        /// <summary>
        /// Creates a completed task with the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <param name="value">The value to set as the task result.</param>
        /// <returns>A completed task with the specified value.</returns>
        private static Task<T> MakeTask<T>(T value)
        {
            return FromResult<T>(value);
        }

        /// <summary>
        /// Creates a completed task with the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <param name="value">The value to set as the task result.</param>
        /// <returns>A completed task with the specified value.</returns>
        public static Task<T> FromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}
