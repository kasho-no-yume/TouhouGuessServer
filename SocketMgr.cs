using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;

namespace TouhouGuessServer
{
    internal class SocketMgr
    {
        private Socket socket;
        private int port = 9988;
        private const int BufferSize = 1024;
        private byte[] _buffer = new byte[BufferSize];
        private static SocketMgr ins = new SocketMgr();
        public static SocketMgr getIns()
        {
            return ins;
        }
        public SocketMgr() 
        { 
            var ip = new IPEndPoint(IPAddress.IPv6Any, port);
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.DualMode = true;
            socket.Bind(ip);
            socket.Listen(100);
        }

        public async Task Run()
        {
            Console.WriteLine("socket正常启动！");
            while (true)
            {               
                var clientSocket = await socket.AcceptAsync();
                Console.WriteLine("客户端连接: " + clientSocket.RemoteEndPoint);
                Task.Run(() => HandleClient(clientSocket));
            }
        }

        private async Task HandleClient(Socket clientSocket)
        {
            try
            {
                while (clientSocket.Connected)
                {
                    int received = await clientSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), SocketFlags.None);
                    if (received == 0) // Client disconnected
                    {
                        Console.WriteLine("客户端下号: " + clientSocket.RemoteEndPoint);
                        if(DataCache.OnlineUser.TryGetValue(clientSocket.RemoteEndPoint, out var user))
                        {                            
                            DataCache.OnlineUser.Remove(clientSocket.RemoteEndPoint);
                            user.Dispose();
                        }                      
                        break;
                    }

                    string message = Encoding.UTF8.GetString(_buffer, 0, received);
                    EventMgr.DealRecvSocketMsgEvent(message, clientSocket);
                    Console.WriteLine("收到来自客户端的消息 " + clientSocket.RemoteEndPoint + ": " + message);                  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("socket断开连接: " + ex.Message + " " + clientSocket.RemoteEndPoint);
                if (DataCache.OnlineUser.TryGetValue(clientSocket.RemoteEndPoint, out var user))
                {                    
                    DataCache.OnlineUser.Remove(clientSocket.RemoteEndPoint);
                    user.Dispose();
                }
            }
            finally
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }
        }

        public void SendToClient(Socket clientSocket, String msg)
        {
            Task.Run(async () =>
            {
                // 将消息字符串转换为字节数组
                byte[] messageBytes = Encoding.UTF8.GetBytes(msg);

                // 获取消息长度前缀（4个字节，表示消息的长度）
                byte[] lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

                // 创建一个包含长度前缀和消息的最终发送缓冲区
                byte[] responseBuffer = new byte[lengthPrefix.Length + messageBytes.Length];

                // 将长度前缀和消息字节数组复制到发送缓冲区
                Array.Copy(lengthPrefix, 0, responseBuffer, 0, lengthPrefix.Length);
                Array.Copy(messageBytes, 0, responseBuffer, lengthPrefix.Length, messageBytes.Length);

                // 发送完整的消息（包含长度前缀和消息内容）
                await clientSocket.SendAsync(responseBuffer, SocketFlags.None);
                Console.WriteLine("向客户端发送消息：" + clientSocket.RemoteEndPoint + ": " + msg);
            });
        }

    }
}
