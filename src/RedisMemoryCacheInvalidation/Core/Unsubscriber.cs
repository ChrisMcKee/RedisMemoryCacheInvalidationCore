using System;
using RedisMemoryCacheInvalidation.Utils;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Provides a mechanism to unsubscribe from notifications.
    /// </summary>
    internal class Unsubscriber : IDisposable
    {
        private readonly SynchronizedCollection<INotificationObserver<string>> _observers;
        private readonly INotificationObserver<string> _observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Unsubscriber"/> class.
        /// </summary>
        /// <param name="observers">The collection of observers.</param>
        /// <param name="observer">The observer to unsubscribe.</param>
        public Unsubscriber(SynchronizedCollection<INotificationObserver<string>> observers, INotificationObserver<string> observer)
        {
            Guard.NotNull(observers, nameof(observers));
            Guard.NotNull(observer, nameof(observer));

            this._observers = observers;
            this._observer = observer;
        }

        /// <summary>
        /// Unsubscribes the observer from the collection.
        /// </summary>
        public void Dispose()
        {
            if(_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
