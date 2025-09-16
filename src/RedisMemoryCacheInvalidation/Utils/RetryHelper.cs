using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RedisMemoryCacheInvalidation.Utils
{
    /// <summary>
    /// Provides retry functionality for operations that may fail transiently.
    /// </summary>
    internal static class RetryHelper
    {
        /// <summary>
        /// Executes an action with retry logic.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="maxRetries">The maximum number of retry attempts.</param>
        /// <param name="delayMs">The delay between retries in milliseconds.</param>
        /// <param name="exponentialBackoff">Whether to use exponential backoff for delays.</param>
        /// <param name="logger">Optional logger for retry attempts.</param>
        /// <returns>true if the action succeeded; otherwise, false.</returns>
        public static bool ExecuteWithRetry(Action action, int maxRetries = 3, int delayMs = 1000, bool exponentialBackoff = true, ILogger logger = null)
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    action();
                    if (attempt > 0)
                    {
                        logger?.LogInformation("Action succeeded on retry attempt {Attempt}", attempt + 1);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        logger?.LogError(ex, "Action failed after {MaxRetries} attempts", maxRetries + 1);
                        return false;
                    }

                    var delay = exponentialBackoff ? delayMs * (int)Math.Pow(2, attempt) : delayMs;
                    logger?.LogWarning(ex, "Action failed on attempt {Attempt}, retrying in {Delay}ms", attempt + 1, delay);
                    Thread.Sleep(delay);
                }
            }

            return false;
        }

        /// <summary>
        /// Executes an async action with retry logic.
        /// </summary>
        /// <param name="action">The async action to execute.</param>
        /// <param name="maxRetries">The maximum number of retry attempts.</param>
        /// <param name="delayMs">The delay between retries in milliseconds.</param>
        /// <param name="exponentialBackoff">Whether to use exponential backoff for delays.</param>
        /// <param name="logger">Optional logger for retry attempts.</param>
        /// <returns>A task that represents the asynchronous operation and returns true if the action succeeded; otherwise, false.</returns>
        public static async Task<bool> ExecuteWithRetryAsync(Func<Task> action, int maxRetries = 3, int delayMs = 1000, bool exponentialBackoff = true, ILogger logger = null)
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await action();
                    if (attempt > 0)
                    {
                        logger?.LogInformation("Async action succeeded on retry attempt {Attempt}", attempt + 1);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        logger?.LogError(ex, "Async action failed after {MaxRetries} attempts", maxRetries + 1);
                        return false;
                    }

                    var delay = exponentialBackoff ? delayMs * (int)Math.Pow(2, attempt) : delayMs;
                    logger?.LogWarning(ex, "Async action failed on attempt {Attempt}, retrying in {Delay}ms", attempt + 1, delay);
                    await Task.Delay(delay);
                }
            }

            return false;
        }

        /// <summary>
        /// Executes an async function with retry logic and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="func">The async function to execute.</param>
        /// <param name="maxRetries">The maximum number of retry attempts.</param>
        /// <param name="delayMs">The delay between retries in milliseconds.</param>
        /// <param name="exponentialBackoff">Whether to use exponential backoff for delays.</param>
        /// <param name="defaultValue">The default value to return if all retries fail.</param>
        /// <param name="logger">Optional logger for retry attempts.</param>
        /// <returns>A task that represents the asynchronous operation and returns the result or default value.</returns>
        public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> func, int maxRetries = 3, int delayMs = 1000, bool exponentialBackoff = true, T defaultValue = default(T), ILogger logger = null)
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var result = await func();
                    if (attempt > 0)
                    {
                        logger?.LogInformation("Async function succeeded on retry attempt {Attempt}", attempt + 1);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        logger?.LogError(ex, "Async function failed after {MaxRetries} attempts, returning default value", maxRetries + 1);
                        return defaultValue;
                    }

                    var delay = exponentialBackoff ? delayMs * (int)Math.Pow(2, attempt) : delayMs;
                    logger?.LogWarning(ex, "Async function failed on attempt {Attempt}, retrying in {Delay}ms", attempt + 1, delay);
                    await Task.Delay(delay);
                }
            }

            return defaultValue;
        }
    }
}
