using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace Redis.Message
{
    public class Subscriber: IDisposable
    {
        ServiceStack.Redis.RedisClient _redis;
        RedisMessageClient _client;
        Dictionary<string, Action<dynamic>> _channels = new Dictionary<string, Action<dynamic>>();
        Context _context;
        public Subscriber(RedisMessageClient client)
        {
            _client = client;
            _redis = new RedisClient(client.Host, client.Port, client.Password);
            _context = Thread.CurrentContext;

            // registry client
            _redis.SAdd(Consts.CONST_CLIENTS_SET_NAME, Encoding.UTF8.GetBytes(client.ClientId));
        }
        public Subscriber On(string channel, Action<dynamic> callback)
        {
            if (!_channels.ContainsKey(channel))
            {
                _channels.Add(channel, callback);
            }

            return this;
        }

        public void Publish(string channel, dynamic message)
        {
            // 序列化消息
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message() { Channel = channel, Data = message }));

            // 将消息存放在各自client的队列
            var clients = _redis.SMembers(Consts.CONST_CLIENTS_SET_NAME);
            foreach (var client in clients)
            {
                var clientId = Encoding.UTF8.GetString(client);
                if (clientId == _client.ClientId) continue;
                _redis.LPush($"{Consts.CONST_PREFIX}{clientId}", data);
            }

            _redis.Publish(channel, data);
        }


        public void Run()
        {
            // 处理队列缓存的消息
            ProcessQueueMessages();

            using (var sub = _redis.CreateSubscription())
            {
                sub.OnMessage = (channel, m) =>
                {
                    ProcessQueueMessages();
                };
                sub.SubscribeToChannels(_channels.Keys.ToArray());
            }

        }

        private void ProcessQueueMessages()
        {
            Console.WriteLine("ProcessQueueMessages");
            byte[] message;
            using (var redis = new RedisClient(_client.Host, _client.Port, _client.Password))
            {
                Console.WriteLine("CHECK: {0}", $"{Consts.CONST_PREFIX}{_client.ClientId}");
                while (redis != null && (message = redis.RPop($"{Consts.CONST_PREFIX}{_client.ClientId}")) != null)
                {
                    var m = Encoding.UTF8.GetString(message);
                    var obj = JsonConvert.DeserializeObject<Message>(m);
                    Console.WriteLine("POP: {0}", m);
                    if (_channels.ContainsKey(obj.Channel))
                    {
                        _channels[obj.Channel](obj.Data);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_redis != null)
            {
                _redis.Dispose();
            }
        }
    }
}
