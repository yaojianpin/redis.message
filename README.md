# redis.message 

Wrap Redis pub/sub and store message even if the website stopped
Even if the sub console is breaked, when restarted, it can continue to receive the message

# Requires
.net framework 4.5+

# How to use it
search 	Redis.Message in nuget


# Example
## publish message
```
  var name = "test 1";
  using (var client = new Redis.Message.RedisMessageClient(name, <host>, <port>, <password>))
  {
      client.Publish("test-message", new {
          text = "Hi"
      });
  }
                    
```

## Subscribe message
```
  var name = "client 2";
  using (var client = new Redis.Message.RedisMessageClient(name, <host>, <port>, <password>))
  {
      // message is dynamic type
      client.On("test-message", (message) =>
      {
          string text = message.text;
          Console.WriteLine(text);
          
      }).Run();
  }
```
