using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Redis.Message
{
    public class RedisMessageClient : IDisposable
    {
        public string ClientId { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Password { get; private set; }

        Subscriber _subscriber;
        public RedisMessageClient(string clientId, string host, int port = 6379, string password = null)
        {
            if (string.IsNullOrEmpty(clientId)) throw new InvalidOperationException("invalid clientId");
            ClientId = clientId;
            Host = host;
            Port = port;
            Password = password;

            _subscriber = new Subscriber(this);

        }

        public void Publish(string channel, dynamic message)
        {
            _subscriber.Publish(channel, message);
        }

        public Subscriber On(string channel, Action<dynamic> callback)
        {
            return _subscriber.On(channel, callback);
        }

        public void Dispose()
        {
            _subscriber.Dispose();
        }
    }
}
