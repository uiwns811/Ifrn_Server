using DummyClient;
using ServerCore;
using UnityEngine;

internal class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat p = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        //if (p.playerId == 1)
        {
            Debug.Log(p.chat);

            GameObject go = GameObject.Find("Player");
            if (go == null)
                Debug.Log("Player Not Found");
            else
                Debug.Log("Player Found");
        }
    }
}

// Unity : 게임 구동 쓰레드가 아닌 다른 쓰레드가 게임 관련 부분에 참여하는 것을 차단해놓음
// - 비동기로 Recv하니까 실제로 Recv하는 부분은 메인(편의상) 쓰레드가 아닐 수 있다.
// - Unity를 구동하는 메인 쓰레드가 아니라 쓰레드 풀의 쓰레드가 네트워크 패킷을 처리하고 있다.
// - 그래서 Player를 찾는 코드(Unity 관련 내용 접근)가 문제를 일으키고 있다

// 패킷 처리를 Handler에서 하지 않고, Queue에 저장해주고,
// Unity 게임 쓰레드가 시간될 때 꺼내서 처리