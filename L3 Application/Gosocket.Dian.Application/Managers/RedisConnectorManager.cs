using Gosocket.Dian.Infrastructure;
using StackExchange.Redis;
using System;

namespace Gosocket.Dian.Application.Managers
{
    class RedisConnectorManager
    {
        static RedisConnectorManager()
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(ConfigurationManager.GetValue("GlobalRedis"));
            });
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
