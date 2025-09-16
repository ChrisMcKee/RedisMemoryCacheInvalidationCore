using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    /// <summary>
    /// A resilient Redis connection wrapper that provides automatic reconnection and health monitoring.
    /// </summary>
    internal class ResilientRedisConnection : IRedisConnection, IConnectionHealthMonitor
    {
        private readonly IRedisConnection _innerConnection;
        private readonly Timer _healthCheckTimer;
        private readonly ILogger _logger;
        private bool _isHealthy = true;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientRedisConnection"/> class.
        /// </summary>
        /// <param name="innerConnection">The inner Redis connection to wrap.</param>
        /// <param name="healthCheckInterval">The interval between health checks in milliseconds.</param>
        /// <param name="logger">Optional logger for diagnostic information.</param>
        public ResilientRedisConnection(IRedisConnection innerConnection, int healthCheckInterval = 30000, ILogger logger = null)
        {
            _innerConnection = innerConnection ?? throw new ArgumentNullException(nameof(innerConnection));
            _logger = logger;
            _healthCheckTimer = new Timer(PerformHealthCheckCallback, null, healthCheckInterval, healthCheckInterval);
        }

        /// <summary>
        /// Gets a value indicating whether the connection is currently connected to Redis.
        /// </summary>
        public bool IsConnected => _innerConnection.IsConnected;

        /// <summary>
        /// Gets a value indicating whether the connection is healthy.
        /// </summary>
        public bool IsHealthy
        {
            get
            {
                lock (_lock)
                {
                    return _isHealthy;
                }
            }
        }

        /// <summary>
        /// Event raised when the connection health status changes.
        /// </summary>
        public event EventHandler<bool> HealthStatusChanged;

        /// <summary>
        /// Establishes a connection to Redis.
        /// </summary>
        /// <returns>true if the connection was established successfully; otherwise, false.</returns>
        public bool Connect()
        {
            var result = _innerConnection.Connect();
            UpdateHealthStatus(result);
            return result;
        }

        /// <summary>
        /// Disconnects from Redis and cleans up resources.
        /// </summary>
        public void Disconnect()
        {
            _innerConnection.Disconnect();
            UpdateHealthStatus(false);
        }

        /// <summary>
        /// Gets the Redis server configuration asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation and returns the configuration key-value pairs.</returns>
        public Task<KeyValuePair<string, string>[]> GetConfigAsync()
        {
            if (!IsHealthy)
            {
                return Task.FromResult(new KeyValuePair<string, string>[] { });
            }

            return _innerConnection.GetConfigAsync();
        }

        /// <summary>
        /// Subscribes to a Redis channel and sets up a message handler.
        /// </summary>
        /// <param name="channel">The Redis channel to subscribe to.</param>
        /// <param name="handler">The action to invoke when a message is received on the channel.</param>
        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            if (IsHealthy)
            {
                _innerConnection.Subscribe(channel, handler);
            }
        }

        /// <summary>
        /// Unsubscribes from all Redis channels.
        /// </summary>
        public void UnsubscribeAll()
        {
            _innerConnection.UnsubscribeAll();
        }

        /// <summary>
        /// Publishes a message to a Redis channel asynchronously.
        /// </summary>
        /// <param name="channel">The Redis channel to publish to.</param>
        /// <param name="value">The message value to publish.</param>
        /// <returns>A task that represents the asynchronous operation and returns the number of subscribers that received the message.</returns>
        public Task<long> PublishAsync(string channel, string value)
        {
            if (!IsHealthy)
            {
                return Task.FromResult(0L);
            }

            return _innerConnection.PublishAsync(channel, value);
        }

        /// <summary>
        /// Performs a health check on the connection.
        /// </summary>
        /// <returns>true if the connection is healthy; otherwise, false.</returns>
        public bool PerformHealthCheck()
        {
            try
            {
                var isHealthy = _innerConnection.IsConnected;
                UpdateHealthStatus(isHealthy);
                return isHealthy;
            }
            catch(Exception ex)
            {
                SafeLogger.LogWarning(_logger, ex, "Health check failed for Redis connection");
                UpdateHealthStatus(false);
                return false;
            }
        }

        private void PerformHealthCheckCallback(object state)
        {
            PerformHealthCheck();
        }

        private void UpdateHealthStatus(bool isHealthy)
        {
            bool statusChanged = false;
            lock (_lock)
            {
                if (_isHealthy != isHealthy)
                {
                    _isHealthy = isHealthy;
                    statusChanged = true;
                }
            }

            if (statusChanged)
            {
                SafeLogger.LogInformation(_logger, "Redis connection health status changed to: {Status}", isHealthy ? "Healthy" : "Unhealthy");
                HealthStatusChanged?.Invoke(this, isHealthy);
            }
        }

        /// <summary>
        /// Disposes the connection and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            _healthCheckTimer?.Dispose();
            _innerConnection.Disconnect();
        }
    }
}
