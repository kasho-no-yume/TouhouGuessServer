// See https://aka.ms/new-console-template for more information
using TouhouGuessServer;

Console.WriteLine("原始人，启动！");
SocketMgr.getIns().Run();
while (true)
{
    var cmd = Console.ReadLine();
    var command = cmd?.Split(" ")[0];
    switch (command)
    {
        case "online":
            foreach(var kv in DataCache.OnlineUser)
            {
                Console.WriteLine(kv.Key + "  " + kv.Value.UserName);
            }
            break;
        case "rooms":
            foreach(var kv in DataCache.ReadyRoom)
            {
                Console.WriteLine(kv.Key + " 房主：" + kv.Value.host.UserName);
            }
            break;
    }
}