﻿using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class PacketHandler
    {
        // 수동으로 관리.
        // 해당 패킷으로 무엇을 호출할 것인가?
        // session : 어떤 세션에서 조립되었냐
        // packet : 어떤 패킷이냐
        // 함수 이름 : 패킷이름 + Handler

        public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoReq p = packet as PlayerInfoReq;

            Console.WriteLine($"PlayerInfoReq ID : {p.playerId}, playerName : {p.name}");

            foreach (PlayerInfoReq.Skill skill in p.skills)
                Console.WriteLine($"Skill({skill.id}, {skill.level}, {skill.duration})");
        }
    }
}