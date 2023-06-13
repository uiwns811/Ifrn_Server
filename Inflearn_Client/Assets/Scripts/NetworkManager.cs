using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

// MonoBehavior ��� : ������Ʈ ������� ���ư�
// - Unity ���� �����忡�� ����

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
        // �̷��� �ϸ� 1������ �� ��Ŷ 1�� ó��
        // - While�� �ϵ� �˾Ƽ� �ϸ� ��
        IPacket packet = PacketQueue.Instance.Pop();

        if (packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    // 1�ʸ��� ���� : �ڷ�ƾ
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
