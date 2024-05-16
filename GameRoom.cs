using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouhouGuessServer
{
    enum GuessMode
    {
        choose,
        menuChoose
    }
    internal class GameRoom
    {
        private static int ExistedRoomId;
        private static int MaxOtherPlayer = 3;
        private bool useOldAlbum;   //启用旧作
        private int aimSecond;  //需要截取的秒数。
        private GuessMode mode;
        public int roomId;      //应从1开始。
        public List<User> player;
        public User host;
        public string roomName;
        private bool gaming;
        public GameRoom(User host)
        {
            player = new List<User>();
            this.host = host;          
            useOldAlbum = false;
            aimSecond = 10;
            mode = GuessMode.choose;    //默认以中等配置。
            this.roomId = ++GameRoom.ExistedRoomId;
            this.host.GameRoomId = this.roomId;
            this.roomName = host.UserName + "的房间";
            gaming = false;
            DataCache.ReadyRoom.Add(roomId, this);
        }
        public void ExitRoom(User user)
        {
            user.GameRoomId = 0;
            if(user.Equals(host))
            {
                foreach(var item in player)
                {
                    EventMgr.ReplyToClient(item.Socket, new Message((int)STCME.SomeoneExit, user.UserName));
                }
                if(player.Count > 0)
                {
                    this.host = player.First();
                    player.RemoveAt(0);
                }
                else
                {
                    DataCache.ReadyRoom.Remove(this.roomId);
                }
            }
            else
            {
                this.player.Remove(user);
                foreach (var item in player)
                {
                    EventMgr.ReplyToClient(item.Socket, new Message((int)STCME.SomeoneExit, user.UserName));
                }
                EventMgr.ReplyToClient(host.Socket, new Message((int)STCME.SomeoneExit, user.UserName));
            }
        }
    }
}
