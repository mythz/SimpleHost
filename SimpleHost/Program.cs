using System;
using System.Diagnostics;
using Funq;
using ServiceStack;
using ServiceStack.Redis;

namespace SimpleHost
{
    [Route("/hello/{Name}")]
    public class Hello
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public long Times { get; set; }
        public string Result { get; set; }
    }

    public class MyServices : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse
            {
                Times = base.Cache.Increment("key", 1),
                Result = "Hello, {0}!".Fmt(request.Name),
            };
        }
    }

    public class AppHost : AppSelfHostBase
    {
        public AppHost() : base("Linux Test", typeof(MyServices).Assembly) { }

        public override void Configure(Container container)
        {
            container.Register<IRedisClientsManager>(c => new PooledRedisClientManager("localhost:6379"));
            container.Register(c => c.Resolve<IRedisClientsManager>().GetCacheClient());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new AppHost()
                .Init()
                .Start("http://*:1337/");

            Process.Start("http://localhost:1337/hello/World");

            Console.ReadLine();
        }
    }
}
