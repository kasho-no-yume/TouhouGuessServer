using System.Net.Sockets;
using Newtonsoft.Json;

namespace TouhouGuessServer
{
    enum RSME
    {
        ConnectAndLogin = 0,
        RefreshHall,
        CreateRoom,
        JoinRoom,
        ChangeRoomSetting,
        StartGame,
        HostUploadQues,
        ChatInRoom,
        ChatInGame,
        Answer,
        ExitRoom
    }
    enum STCME
    {
        LoginSuccess = 0,
        LoginFailure,
        RefreshHall,
        EnterRoom,
        ChangeRoomSetting,
        ChatInRoom,
        GameStart,
        NewQuesData,
        ChatInGame,
        SomeoneGuess,
        SyncGameData,
        SomeoneExit
    }
    internal class Message
    {
        public int id { get; set; }
        public object? data { get; set; }
        public Message(int id, object data)
        {
            this.id = id;
            this.data = data;
        }
    }
    internal class EventMgr
    {
        public static void ReplyToClient(Socket socket,Message data)
        {
            if (socket != null) 
            {
                SocketMgr.getIns().SendToClient(socket, JsonConvert.SerializeObject(data));
            }
        }
        public static void DealRecvSocketMsgEvent(string msg,Socket socket)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<Message>(msg);
                switch ((RSME)obj.id)
                {
                    case RSME.ConnectAndLogin:
                        var player = new User(socket.RemoteEndPoint, (string)obj.data, socket);
                        if(DataCache.OnlineUser.TryAdd(socket.RemoteEndPoint, player))
                        {
                            ReplyToClient(socket, new Message((int)STCME.LoginSuccess, ""));
                        }
                        else
                        {
                            ReplyToClient(socket, new Message((int)STCME.LoginFailure, ""));
                        }
                        break;
                    case RSME.RefreshHall:
                        var temp = new List<GameRoom>();
                        foreach(var i in DataCache.ReadyRoom)
                        {
                            temp.Add(i.Value);
                        }
                        var res = temp.ToArray().Select(o => new { id = o.roomId, name = o.roomName});
                        ReplyToClient(socket, new Message((int)STCME.RefreshHall,JsonConvert.SerializeObject(res)));
                        break;
                    case RSME.CreateRoom:

                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("解析cs信息出现异常："+e.ToString());
            }
        }
    }
}
