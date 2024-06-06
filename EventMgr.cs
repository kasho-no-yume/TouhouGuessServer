using System.Net.Sockets;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace TouhouGuessServer
{
    enum RSME   //receive socket message event
    {
        ConnectAndLogin = 0,
        RefreshHall,
        CreateRoom,
        JoinRoom,
        ChangeRoomSetting,
        StartGame,
        HostUploadQues,
        Chatting,
        Answer,
        Waiver,
        ExitRoom
    }
    enum STCME  //send to client message event
    {
        LoginSuccess = 0,
        LoginFailure,
        ServerMsg,
        RefreshHall,
        EnterRoom,
        ChangeRoomSetting,
        Chatting,
        GameStart,
        NewQuesData,
        SomeoneGuess,
        SomeoneWaive,
        SomeoneExit
    }
    internal class Message
    {
        public int id { get; set; }
        public object? data { get; set; }
        public Message()
        {

        }
        public Message(int id, object data)
        {
            this.id = id;
            this.data = data;
        }
        public Message(STCME id,object data)
        {
            this.id = (int)id;
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
                var jobj = JsonObject.Parse(msg);
                User? user;
                switch ((RSME)obj.id)
                {
                    case RSME.ConnectAndLogin:
                        foreach(var kv in DataCache.OnlineUser)
                        {
                            if (kv.Value.UserName.Equals((string)obj.data))
                            {
                                ReplyToClient(socket, new Message((int)STCME.LoginFailure, ""));
                                ReplyToClient(socket, new Message((int)STCME.ServerMsg, "昵称重复！请重新取名。"));
                                return;
                            }
                        }
                        var player = new User(socket.RemoteEndPoint, (string)obj.data, socket);
                        if(DataCache.OnlineUser.TryAdd(socket.RemoteEndPoint, player))
                        {
                            ReplyToClient(socket, new Message((int)STCME.LoginSuccess, "Original"));
                        }
                        else
                        {
                            ReplyToClient(socket, new Message((int)STCME.LoginFailure, ""));
                            ReplyToClient(socket, new Message((int)STCME.ServerMsg, "进服失败。"));
                        }
                        break;
                    case RSME.RefreshHall:
                        var temp = new List<GameRoom>();
                        foreach(var i in DataCache.ReadyRoom)
                        {
                            temp.Add(i.Value);
                        }
                        var res = temp.ToArray().Select(o => new { id = o.roomId, name = o.roomName, num = o.playerNum});
                        ReplyToClient(socket, new Message((int)STCME.RefreshHall,JsonConvert.SerializeObject(res)));
                        break;
                    case RSME.CreateRoom:
                        if(DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            new GameRoom(user);
                        }
                        else
                        {
                            ReplyToClient(socket, new Message(STCME.ServerMsg, "创建房间失败。。"));
                        }
                        break;
                    case RSME.JoinRoom:
                        if(DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            int num = (int)(long)obj.data;
                            if (DataCache.ReadyRoom.TryGetValue(num,out var gameRoom))
                            {
                                if (!gameRoom.EnterRoom(user))
                                {
                                    ReplyToClient(socket, new Message(STCME.ServerMsg, "加入失败。游戏已开始或人数已满。"));
                                }
                            }
                            else
                            {
                                ReplyToClient(socket, new Message(STCME.ServerMsg, "房间不存在。"));
                            }
                        }
                        break;
                    case RSME.ChangeRoomSetting:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if(DataCache.ReadyRoom.TryGetValue(user.GameRoomId,out var gameRoom))
                            {
                                var data = jobj["data"];
                                gameRoom.GetSettingFromHost((int)data["old"] == 1?true:false, (int)data["aim"], (int)data["mode"], (int)data["round"]);
                            }
                        }
                        break;
                    case RSME.StartGame:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if (DataCache.ReadyRoom.TryGetValue(user.GameRoomId, out var gameRoom))
                            { 
                                gameRoom.StartGame();
                            }
                        }
                        break;
                    case RSME.HostUploadQues:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if (DataCache.ReadyRoom.TryGetValue(user.GameRoomId, out var gameRoom))
                            {
                                gameRoom.GamingIns?.GetQuestion(obj);
                            }
                        }
                        break;
                    case RSME.Answer:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if (DataCache.ReadyRoom.TryGetValue(user.GameRoomId, out var gameRoom))
                            {
                                gameRoom.GamingIns?.SomeoneAnswer(obj);
                            }
                        }
                        break;
                    case RSME.Chatting:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if (DataCache.ReadyRoom.TryGetValue(user.GameRoomId, out var gameRoom))
                            {
                                gameRoom.Chatting(obj);
                            }
                        }
                        break;
                    case RSME.ExitRoom:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if (DataCache.ReadyRoom.TryGetValue(user.GameRoomId, out var gameRoom))
                            {
                                gameRoom.ExitRoom(user);
                            }
                        }
                        break;
                    case RSME.Waiver:
                        if (DataCache.OnlineUser.TryGetValue(socket.RemoteEndPoint, out user))
                        {
                            if (DataCache.ReadyRoom.TryGetValue(user.GameRoomId, out var gameRoom))
                            {
                                gameRoom.GamingIns?.SomeoneWaive(obj);
                            }
                        }
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
