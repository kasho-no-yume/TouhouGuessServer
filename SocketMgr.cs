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
            var ip = new IPEndPoint(IPAddress.Any, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                            user.Dispose();
                            DataCache.OnlineUser.Remove(clientSocket.RemoteEndPoint);
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
                Console.WriteLine("socket出现异常: " + ex.Message);
            }
            finally
            {
                clientSocket.Close();
            }
        }

        public void SendToClient(Socket clientSocket,String msg)
        {
            Task.Run(async () =>
            {
                byte[] responseBuffer = Encoding.UTF8.GetBytes(msg);
                // Echo the message back to the client
                await clientSocket.SendAsync(responseBuffer, SocketFlags.None);
                Console.WriteLine("向客户端发送消息：" + clientSocket.RemoteEndPoint + ": " + msg);
            });
        }
    }
}
