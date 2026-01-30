using System.Net;
using System.Net.Sockets;
using TrProtocol; // For CommonCode helpers if needed, though we use inspector

namespace TrProtocol.TestAgent;

public class ProxyServer(int listenPort, string targetHost, int targetPort)
{
    public async Task StartAsync()
    {
        var listener = new TcpListener(IPAddress.Any, listenPort);
        listener.Start();
        Console.WriteLine($"[Proxy] Listening on port {listenPort}, forwarding to {targetHost}:{targetPort}");

        while (true)
        {
            var clientSocket = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(clientSocket);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"[Proxy] Client connected: {client.Client.RemoteEndPoint}");
        try
        {
            using var server = new TcpClient();
            await server.ConnectAsync(targetHost, targetPort);
            Console.WriteLine($"[Proxy] Connected to remote server.");

            using var clientStream = client.GetStream();
            using var serverStream = server.GetStream();

            var taskC2S = PipeAndInspectAsync(clientStream, serverStream, true);
            var taskS2C = PipeAndInspectAsync(serverStream, clientStream, false);

            await Task.WhenAny(taskC2S, taskS2C);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Proxy] Connection Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
            Console.WriteLine("[Proxy] Session closed.");
        }
    }

    private async Task PipeAndInspectAsync(NetworkStream input, NetworkStream output, bool isC2S)
    {
        byte[] readBuffer = new byte[1024 * 1024];
        List<byte> accumulation = [];

        try
        {
            while (true)
            {
                int read = 0;
                try
                {
                    read = await input.ReadAsync(readBuffer, 0, readBuffer.Length);
                }
                catch (IOException) { break; } // Normal close
                catch (SocketException) { break; } // Socket error
                catch (ObjectDisposedException) { break; } // Stream disposed

                if (read == 0) break; // content closed

                // 1. Forward raw bytes
                try
                {
                    await output.WriteAsync(readBuffer, 0, read);
                }
                catch (Exception) { break; } // Connection broken on write

                // 2. Add to inspection buffer
                for (int i = 0; i < read; i++) accumulation.Add(readBuffer[i]);

                // 3. Try to frame packets
                while (true)
                {
                    if (accumulation.Count < 3) break;

                    byte low = accumulation[0];
                    byte high = accumulation[1];
                    // TrProtocol uses short, but for framing we want to ensure we treat it safely
                    ushort length = (ushort)(low | (high << 8));

                    // Sanity check for length (prevent infinite loops on corrupt streams)
                    if (length < 3)
                    {
                        // Invalid packet length (too small for Header+ID). Flush one byte to try to resync?
                        // Or just log and clear?
                        // For a proxy, we can't easily resync. 
                        Console.WriteLine($"[Sync Error] Invalid length {length} detected. Resetting buffer.");
                        accumulation.Clear();
                        break;
                    }

                    if (accumulation.Count < length)
                    {
                        // Wait for more data
                        break;
                    }

                    byte[] packetBytes = [.. accumulation.GetRange(0, length)];
                    accumulation.RemoveRange(0, length);

                    try
                    {
                        if (TestAgentRuntime.ShouldInspectDirection(isC2S))
                        {
                            PacketInspector.Inspect(packetBytes, length, isC2S);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Inspector Error] {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Catch-all for anything else to prevent crash
            Console.WriteLine($"[PipeLoop Error] {ex.GetType().Name}: {ex.Message}");
        }
    }
}
