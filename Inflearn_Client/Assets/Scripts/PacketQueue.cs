using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> _packetQueue = new Queue<IPacket>(); 
    object _lock = new object();   

    public void Push(IPacket packet)
    {
        lock(_lock)
        {
            _packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock(_lock)
        {
            if (_packetQueue.Count == 0)
                return null;
            
            return _packetQueue.Dequeue();
        }
    }
}

// Component XX
// 패킷 저장 용도로 사용

// Unity 게임 메인 쓰레드가 아닌 쓰레드 풀에 있는 백그라운드 쓰레드에서 Recv
// Recv했을 때 NetworkManager에서 PacketQueue에 넣어주고
// Unity 메인 쓰레드에서 Update할 때 PacketQueue에서 Pop해서 패킷 처리 (HandlePacket)