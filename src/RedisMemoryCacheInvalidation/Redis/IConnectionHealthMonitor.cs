using System;

namespace RedisMemoryCacheInvalidation.Redis
{
    /// <summary>
    /// Defines the contract for monitoring Redis connection health.
    /// </summary>
    public interface IConnectionHealthMonitor
    {
        /// <summary>
        /// Gets a value indicating whether the connection is healthy.
        /// </summary>
        bool IsHealthy { get; }

        /// <summary>
        /// Event raised when the connection health status changes.
        /// </summary>
        event EventHandler<bool> HealthStatusChanged;

        /// <summary>
        /// Performs a health check on the connection.
        /// </summary>
        /// <returns>true if the connection is healthy; otherwise, false.</returns>
        bool PerformHealthCheck();
    }
}
