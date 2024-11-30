using StackExchange.Redis;

namespace SimpleWS_Server.Helper
{
    public class ConnectionHelper
    {
        static ConnectionHelper()
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(configuration: ConfigurationSettings.AppSetting["RedisURL"]);
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
