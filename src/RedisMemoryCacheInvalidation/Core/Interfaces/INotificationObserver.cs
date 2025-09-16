
namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Provides a mechanism for receiving push-based notifications.
    /// </summary>
    /// <typeparam name="T">The type of notification data.</typeparam>
    public interface INotificationObserver<T>
    {
        /// <summary>
        /// Called when a notification is received.
        /// </summary>
        /// <param name="value">The notification data.</param>
        void Notify(T value);
    }
}
