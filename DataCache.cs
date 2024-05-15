using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TouhouGuessServer
{
    internal static class DataCache
    {
        public static Dictionary<EndPoint,User> OnlineUser = new Dictionary<EndPoint,User>();
    }
    internal class User
    {
        private EndPoint _endPoint;
        private string _username;
        private Socket _clientSocket;
        public EndPoint EndPoint { get { return _endPoint; } }
        public string UserName { get { return _username;} }
        public Socket Socket { get { return _clientSocket; } }
        public User(EndPoint endPoint, string username, Socket socket)
        {
            _endPoint = endPoint;
            _username = username;
            _clientSocket = socket;
        }
    }
}
