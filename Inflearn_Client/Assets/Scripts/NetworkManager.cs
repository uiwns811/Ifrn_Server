using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

// MonoBehavior 상속 : 컴포넌트 방식으로 돌아감
// - Unity 메인 쓰레드에서 구동

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    // Start is called before the first frame update
    void Start()
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint, () => { return _session; }, 1);

        StartCoroutine("CoSendPacket");
    }

    // Update is called once per frame
    void Update()
    {
        // 이렇게 하면 1프레임 당 패킷 1개 처리
        // - While을 하든 알아서 하면 됨
        IPacket packet = PacketQueue.Instance.Pop();

        if (packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    // 1초마다 실행 : 코루틴
    IEnumerator CoSendPacket()
    {
        while(true)
        {
            yield return new WaitForSeconds(3.0f);

            C_Chat packet = new C_Chat();
            packet.chat = "Hello Unity";
            ArraySegment<byte> segment = packet.Write();

            _session.Send(segment);
        }
    }
}
