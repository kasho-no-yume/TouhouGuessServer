using 

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
        RefreshHall = 0,
        EnterRoom,
        ChatInRoom,
        GameStart,
        NewQuesData,
        ChatInGame,
        SomeoneGuess,
        SyncGameData,
        SomeoneExit
    }
    internal class EventMgr
    {
        public static void DealRecvSocketMsgEvent(string msg)
        {
            int type;
            if(int.TryParse(msg.Split(":")[0],out type))
            {
                var t = (RSME)type;
                switch(t)
                {
                    case RSME.ConnectAndLogin:
                        break;
                    case RSME.JoinRoom: 
                        break;
                }
            }
        }
    }
}
