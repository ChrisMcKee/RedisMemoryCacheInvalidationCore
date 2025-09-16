using System;
using System.Collections.Concurrent;
using System.Linq;
using RedisMemoryCacheInvalidation.Utils;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Manager subscriptions and notifications.
    /// </summary>
    internal class NotificationManager : INotificationManager<string>
    {
        internal ConcurrentDictionary<string, SynchronizedCollection<INotificationObserver<string>>> SubscriptionsByTopic { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationManager"/> class.
        /// </summary>
        public NotificationManager()
        {
            SubscriptionsByTopic = new ConcurrentDictionary<string, SynchronizedCollection<INotificationObserver<string>>>();
        }

        /// <summary>
        /// Raises a notification to all observers subscribed to the specified topic.
        /// </summary>
        /// <param name="topicKey">The topic key to notify observers about.</param>
        public void Notify(string topicKey)
        {
            var subscriptions = SubscriptionsByTopic.GetOrAdd(topicKey, new SynchronizedCollection<INotificationObserver<string>>());

            if(subscriptions.Count <= 0) return;

            foreach(INotificationObserver<string> observer in subscriptions.ToList()) //avoid collection modified
            {
                observer.Notify(topicKey);
            }
        }

        /// <summary>
        /// Subscribes an observer to notifications for a specific topic.
        /// </summary>
        /// <param name="topicKey">The topic to subscribe to.</param>
        /// <param name="observer">The observer to receive notifications.</param>
        /// <returns>A disposable object that can be used to unsubscribe from the topic.</returns>
        public IDisposable Subscribe(string topicKey, INotificationObserver<string> observer)
        {
            var subscriptions = SubscriptionsByTopic.GetOrAdd(topicKey, new SynchronizedCollection<INotificationObserver<string>>());

            if(!subscriptions.Contains(observer))
                subscriptions.Add(observer);

            return new Unsubscriber(subscriptions, observer);
        }
    }
}
