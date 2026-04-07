using Dimensions.Core;
using System.Net;
using System.Net.Sockets;

namespace Dimensions
{
    public class Listener
    {
        private readonly TcpListener listener;
        public event Action<Exception> OnError = Console.WriteLine;

        public Listener(IPEndPoint ep)
        {
            listener = new TcpListener(ep);
            listener.Start();
        }

        private void OnAcceptClient(TcpClient client)
        {
            var @default = Program.Config.Servers.First();
            try
            {
                new Client(client).TunnelTo(@default);
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
            }
        }
        public void ListenThread()
        {
            for (; ; )
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    Logger.Log("TcpListener", LogLevel.INFO, $"接受来自客户端的连接: {client.Client.RemoteEndPoint}");
                    Task.Run(() => OnAcceptClient(client));
                }
                catch (Exception e)
                {
                    Logger.Log("TcpListener", LogLevel.ERROR, $"接受客户端连接时发生错误: {e}");
                }
            }
        }
    }
}
