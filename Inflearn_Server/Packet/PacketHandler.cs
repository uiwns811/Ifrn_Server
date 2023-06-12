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
    // 수동으로 관리.
    // 해당 패킷으로 무엇을 호출할 것인가?
    // session : 어떤 세션에서 조립되었냐
    // packet : 어떤 패킷이냐
    // 함수 이름 : 패킷이름 + Handler

    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        clientSession.Room.BroadCast(clientSession, chatPacket.chat);
    }
}