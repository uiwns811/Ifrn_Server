using Server;
using Server.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        // clientSession이 갖고 있는 Room은 언제든지 바뀔 수 있음 (Push하면 즉시 실행 X)
        GameRoom room = clientSession.Room;
        room.Push(() => room.BroadCast(clientSession, chatPacket.chat));
    }
}