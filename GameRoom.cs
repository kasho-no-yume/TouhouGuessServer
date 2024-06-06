using Newtonsoft.Json;

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
        private int roundNum = 20;   //游戏轮次。诶这个服务端计算了，到时候结束了把人全扔回房间。上面的服务端不参与计算，客户端算、
        public int roomId;      //应从1开始。
        public List<User> player;   //host不在里面。
        public User host;
        public string roomName;
        private bool gaming;
        private Gaming? gamingIns; 
        public int playerNum
        {
            get { return player.Count + 1; }
        }
        public Gaming? GamingIns
        {
            get{ return gamingIns;}
        }

        public void GetSettingFromHost(bool useOldAlbum,int aimSecond, int mode,int round)
        {
            this.useOldAlbum = useOldAlbum;
            this.aimSecond = aimSecond;
            this.mode = (GuessMode)mode;
            this.roundNum = round;
            SyncSettingToPlayer(this.host);
            foreach(var i in this.player)
            {
                SyncSettingToPlayer(i);
            }
        }
        private void  SyncSettingToPlayer(User user)
        {
            EventMgr.ReplyToClient(user.Socket, new Message(STCME.ChangeRoomSetting, string.Format("{{\"old\":{0},\"aim\":{1},\"mode\":{2},\"round\":{3}}}"
                ,this.useOldAlbum?1:0,this.aimSecond,(int)this.mode, this.roundNum)));
        }
        private void SyncSettingToAllPlayer()
        {
            SyncSettingToPlayer(host);
            foreach(var i in this.player)
            {
                SyncSettingToPlayer(i);
            }
        }
        public GameRoom(User host)
        {
            player = new List<User>();
            this.host = host;          
            useOldAlbum = false;
            aimSecond = 10;
            mode = GuessMode.choose;    //默认以中等配置。即，不启用旧作，截取10秒，选择题模式。
            this.roomId = ++ExistedRoomId;
            this.host.GameRoomId = this.roomId;
            this.roomName = host.UserName + "的房间";
            gaming = false;
            DataCache.ReadyRoom.Add(roomId, this);
            var temp = new User[] { host };
            EventMgr.ReplyToClient(host.Socket, new Message(STCME.EnterRoom,temp.Select(o =>new { name = o.UserName, host = 1 })));
            SyncSettingToPlayer(host);
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
                BoardcastToRoom(new Message((int)STCME.SomeoneExit, user.UserName));
            }
        }

        public void Chatting(Message message)
        {
            BoardcastToRoom(new Message(STCME.Chatting, message.data));
        }

        private void BoardcastToRoom(Message msg)
        {
            EventMgr.ReplyToClient(this.host.Socket, msg);
            foreach (var item in player)
            {
                EventMgr.ReplyToClient(item.Socket, msg);
            }
        }

        //返回是否成功加入房间
        public bool EnterRoom(User user)
        {
            if(this.player.Count >= MaxOtherPlayer || gaming)
            {
                return false;
            }
            user.GameRoomId = this.roomId;
            this.player.Add(user);
            var arr = new User[]{ this.host }.Concat(this.player.ToArray());
            var response = arr.Select(o => new { name = o.UserName, host = o.UserName == this.host.UserName ? 1 : 0 });
            BoardcastToRoom(new Message(STCME.EnterRoom, response));
            SyncSettingToPlayer(user);
            return true;
        }

        private void EndGame()
        {
            this.gaming = false;
            gamingIns = null;
            var arr = new User[] { this.host }.Concat(this.player.ToArray());
            var response = arr.Select(o => new { name = o.UserName, host = o.UserName == this.host.UserName ? 1 : 0 });
            BoardcastToRoom(new Message(STCME.EnterRoom, response));
            SyncSettingToAllPlayer();
        }

        public void StartGame()
        {
            if(gaming)
            {
                return;
            }
            gaming = true;
            this.gamingIns = new Gaming(this.roundNum, BoardcastToRoom, EndGame);
        }
    }
}
