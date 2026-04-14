using RealmNexus;
using RealmNexus.Logging;
using RealmNexus.Models;

ConsoleLogger.OnLogger += (msg, level) =>
{
    Console.WriteLine(msg);
};
var server = new ProxyServer(Config.Instance.Port);
await server.StartAsync();