using System;
using System.Reflection;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    /// <summary>
    /// A Redis connection that manages its own connection multiplexer.
    /// </summary>
    internal class StandaloneRedisConnection : RedisConnectionBase
    {
        private readonly ConfigurationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandaloneRedisConnection"/> class.
        /// </summary>
        /// <param name="configurationOptions">The Redis configuration string.</param>
        public StandaloneRedisConnection(string configurationOptions)
        {
            _options = ConfigurationOptions.Parse(configurationOptions);
        }

        /// <summary>
        /// Establishes a connection to Redis using the configured options.
        /// </summary>
        /// <returns>true if the connection was established successfully; otherwise, false.</returns>
        public override bool Connect()
        {
            if(Multiplexer == null)
            {
                //overrides here
                _options.ConnectTimeout = 5000;
                _options.ConnectRetry = 3;
                _options.KeepAlive = 90;
                _options.AbortOnConnectFail = false;
                _options.ClientName = "InvalidationClient_" + Environment.MachineName + "_" + Assembly.GetCallingAssembly().GetName().Version;

                Multiplexer = ConnectionMultiplexer.Connect(_options);
            }

            return Multiplexer.IsConnected;
        }

        /// <summary>
        /// Disconnects from Redis and cleans up resources.
        /// </summary>
        public override void Disconnect()
        {
            UnsubscribeAll();
            Multiplexer.Close(false);
        }
    }
}
