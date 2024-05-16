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
        public static Dictionary<int,GameRoom> ReadyRoom = new Dictionary<int,GameRoom>();
    }
    internal class User
    {
        private EndPoint _endPoint;
        private string _username;
        private Socket _clientSocket;
        private int? _gameRoomId;
        public EndPoint EndPoint { get { return _endPoint; } }
        public string UserName { get { return _username;} }
        public Socket Socket { get { return _clientSocket; } }
        public int? GameRoomId { get { return _gameRoomId; } set { _gameRoomId = value; } }
        public User(EndPoint endPoint, string username, Socket socket)
        {
            _endPoint = endPoint;
            _username = username;
            _clientSocket = socket;
        }
    }
}
