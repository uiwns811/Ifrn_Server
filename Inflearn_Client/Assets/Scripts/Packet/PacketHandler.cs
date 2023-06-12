using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class PacketHandler
{
    // 수동으로 관리.
    // 해당 패킷으로 무엇을 호출할 것인가?
    // session : 어떤 세션에서 조립되었냐
    // packet : 어떤 패킷이냐
    // 함수 이름 : 패킷이름 + Handler

    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat p = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        if (p.playerId == 1)
            UnityEngine.Debug.Log(p.chat);

        //if (p.playerId == 1)
        //    Console.WriteLine(p.chat);
    }
}