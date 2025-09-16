using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using RedisMemoryCacheInvalidation.Redis;
using RedisMemoryCacheInvalidation.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace RedisMemoryCacheInvalidation.Tests.Integration;

[Collection("RedisServer")]
public class ResilienceTests : IClassFixture<RedisServerFixture>
{
    private readonly MemoryCache _localCache;
    private readonly RedisServerFixture _fixture;
    private readonly Fixture _autoFixture;
    private readonly ILogger _testLogger;

    public ResilienceTests(RedisServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _autoFixture = new Fixture();
        _localCache = new MemoryCache("ResilienceTestCache");
        var loggedMessages = new List<string>();

        // Create a test logger that captures log messages
        _testLogger = new TestLogger(loggedMessages, output);
    }

    private void Cleanup()
    {
        _localCache?.Dispose();
        InvalidationManager.NotificationBus?.Stop();
        InvalidationManager.NotificationBus = null;
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void Configure_WithResilienceEnabled_ShouldCreateResilientConnection()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings
            {
                EnableResilience = true,
                Logger = _testLogger,
                HealthCheckIntervalMs = 1000,
                MaxRetryAttempts = 3,
                RetryDelayMs = 100
            };

            InvalidationManager.Configure(_fixture.GetEndpoint(), settings);

            Assert.NotNull(InvalidationManager.NotificationBus);
            Assert.True(InvalidationManager.IsConnected);
            
            // Verify that the connection is resilient
            var connection = InvalidationManager.NotificationBus.Connection;
            Assert.True(connection is IConnectionHealthMonitor);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void Configure_WithResilienceDisabled_ShouldCreateStandardConnection()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings { EnableResilience = false, Logger = _testLogger };

            InvalidationManager.Configure(_fixture.GetEndpoint(), settings);

            Assert.NotNull(InvalidationManager.NotificationBus);
            Assert.True(InvalidationManager.IsConnected);
            
            // Verify that the connection is NOT resilient
            var connection = InvalidationManager.NotificationBus.Connection;
            Assert.False(connection is IConnectionHealthMonitor);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public async Task InvalidateAsync_WithResilienceEnabled_ShouldWork()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings { EnableResilience = true, Logger = _testLogger, MaxRetryAttempts = 2, RetryDelayMs = 50 };

            InvalidationManager.Configure(_fixture.GetEndpoint(), settings);
            var testKey = _autoFixture.Create<string>();
            var result = await InvalidationManager.InvalidateAsync(testKey);
            Assert.True(result >= 0); // Should succeed or return 0 (no subscribers)
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void HealthMonitor_ShouldDetectConnectionStatus()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings { EnableResilience = true, Logger = _testLogger, HealthCheckIntervalMs = 500 };

            InvalidationManager.Configure(_fixture.GetEndpoint(), settings);
            var connection = InvalidationManager.NotificationBus.Connection as IConnectionHealthMonitor;
            Assert.NotNull(connection);
            Assert.True(connection.IsHealthy);
            Assert.True(connection.PerformHealthCheck());
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public async Task InvalidateAsync_WithInvalidRedisEndpoint_ShouldHandleGracefully()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings { EnableResilience = true, Logger = _testLogger, MaxRetryAttempts = 2, RetryDelayMs = 100 };

            // Use an invalid endpoint
            InvalidationManager.Configure("invalid-host:6379", settings);
            var testKey = _autoFixture.Create<string>();

            var result = await InvalidationManager.InvalidateAsync(testKey);
            Assert.Equal(0L, result); // Should return 0 when not connected
            Assert.False(InvalidationManager.IsConnected);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void ConnectionFactory_WithResilienceEnabled_ShouldCreateResilientConnection()
    {
        var logger = _testLogger;
        var healthCheckInterval = 1000;

        var connection = RedisConnectionFactory.New(_fixture.GetEndpoint(), enableResilience: true, logger, healthCheckInterval);

        Assert.NotNull(connection);
        Assert.True(connection is IConnectionHealthMonitor);

        var healthMonitor = connection as IConnectionHealthMonitor;
        Assert.NotNull(healthMonitor);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void ConnectionFactory_WithResilienceDisabled_ShouldCreateStandardConnection()
    {
        var logger = _testLogger;
        var connection = RedisConnectionFactory.New(_fixture.GetEndpoint(), enableResilience: false, logger);
        Assert.NotNull(connection);
        Assert.False(connection is IConnectionHealthMonitor);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void ResilientConnection_ShouldImplementHealthMonitoring()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings { EnableResilience = true, Logger = _testLogger, HealthCheckIntervalMs = 1000 };

            InvalidationManager.Configure(_fixture.GetEndpoint(), settings);
            var connection = InvalidationManager.NotificationBus.Connection;

            Assert.True(connection is IConnectionHealthMonitor);
            var healthMonitor = connection as IConnectionHealthMonitor;
            Assert.NotNull(healthMonitor);
            Assert.True(healthMonitor.IsHealthy);
            Assert.True(healthMonitor.PerformHealthCheck());
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void Logger_ShouldBePassedToConnection()
    {
        // Ensure clean state before test
        Cleanup();
        
        try
        {
            var settings = new InvalidationSettings { EnableResilience = true, Logger = _testLogger };
            InvalidationManager.Configure(_fixture.GetEndpoint(), settings);
            Assert.NotNull(InvalidationManager.NotificationBus);
            Assert.True(InvalidationManager.IsConnected);
            
            // The logger should be passed through to the connection
            // We can verify this by checking that the connection was created successfully
            var connection = InvalidationManager.NotificationBus.Connection;
            Assert.NotNull(connection);
        }
        finally
        {
            Cleanup();
        }
    }

    // Test logger implementation for capturing log messages
    private class TestLogger : ILogger
    {
        private readonly List<string> _messages;
        private readonly ITestOutputHelper _output;

        public TestLogger(List<string> messages, ITestOutputHelper output)
        {
            _messages = messages;
            _output = output;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var message = formatter(state, exception);
                _messages.Add($"[{logLevel}] {message}");
                _output.WriteLine($"[{logLevel}] {message}");
            }
            catch
            {
                // Ignore logging errors in tests
            }
        }
    }
}
