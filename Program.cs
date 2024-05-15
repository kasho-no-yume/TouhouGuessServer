// See https://aka.ms/new-console-template for more information
using TouhouGuessServer;

Console.WriteLine("原始人，启动！");
await SocketMgr.getIns().Run();