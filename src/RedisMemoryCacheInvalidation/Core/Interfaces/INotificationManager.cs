using System;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Manage a list of subscription. Basically a custom IObservable to support topic-based subscriptions.
    /// </summary>
    /// <typeparam name="T">The type of notification data.</typeparam>
    public interface INotificationManager<T>
    {
        /// <summary>
        /// Subscribes an observer to notifications for a specific topic.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="observer">The observer to receive notifications.</param>
        /// <returns>A disposable object that can be used to unsubscribe from the topic.</returns>
        IDisposable Subscribe(string topic, INotificationObserver<T> observer);

        /// <summary>
        /// Notifies all observers subscribed to the specified topic.
        /// </summary>
        /// <param name="topicKey">The topic key to notify observers about.</param>
        void Notify(string topicKey);
    }
}
