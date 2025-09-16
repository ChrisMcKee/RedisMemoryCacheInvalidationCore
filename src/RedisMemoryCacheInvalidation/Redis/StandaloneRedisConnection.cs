using System;
using System.Reflection;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class StandaloneRedisConnection : RedisConnectionBase
    {
        private readonly ConfigurationOptions options;
        public StandaloneRedisConnection(string configurationOptions)
        {
            options = ConfigurationOptions.Parse(configurationOptions);
        }

        public override bool Connect()
        {
            if(Multiplexer == null)
            {
                //overrides here
                options.ConnectTimeout = 5000;
                options.ConnectRetry = 3;
                // options.DefaultVersion = new Version("2.8.0");
                options.KeepAlive = 90;
                options.AbortOnConnectFail = false;
                options.ClientName = "InvalidationClient_" + Environment.MachineName + "_" + Assembly.GetCallingAssembly().GetName().Version;

                Multiplexer = ConnectionMultiplexer.Connect(options);
            }

            return Multiplexer.IsConnected;
        }

        public override void Disconnect()
        {
            UnsubscribeAll();
            Multiplexer.Close(false);
        }
    }
}
