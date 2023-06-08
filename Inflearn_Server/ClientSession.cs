﻿using Inflearn_ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inflearn_Server
{    abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }
 
    class PlayerInfoReq : Packet
    {
        public long playerId;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;

        }

        public override void Read(ArraySegment<byte> s)
        {
            // 역직렬화 : 버퍼에 있는거 꺼내쓰기
            ushort count = 0;
            //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
            count += 2;
            // ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
            count += 2;
            // count가 버퍼의 크기를 벗어나진 않았는지 지속적으로 체크해줘야 함 

            this.playerId = BitConverter.ToUInt16(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count));
            // this.playerId = BitConverter.ToUInt16(s.Array, s.Offset + count);       // 충분한 공간이 있는지 체크 필요
            // - 범위를 정해서 넣어줄 수 있는 위 버전 (ReadOnlySpan)이 훨씬 안전하다.
            count += 8;
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> s = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            // - success와 & 연산을 통해 TryWriteBytes가 한 번이라도 실패하면 success = false;
            // - 실패하는 경우 : s가 2byte짜린데 우리가 8byte packet을 넣어준 경우 등 .. 
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), this.playerId);
            count += 8;

            success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count);
            // 실제 count하기 전에는 packet.size를 알 수 없음. 그래서 패킷을 다 채우고 난 뒤에 맨 앞에다가 count만큼 넣어주자

            if (success == false)
                return null;

            return SendBufferHelper.Close(count);

        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2,
    }


    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // 역직렬화 : 버퍼에 있는거 꺼내쓰기
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            // count가 버퍼의 크기를 벗어나진 않았는지 지속적으로 체크해줘야 함 

            switch ((PacketID)id)
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq ID : {p.playerId}");
                    }
                    break;
                case PacketID.PlayerInfoOK:
                    break;
            }


            Console.WriteLine($"RecvPacket ID : {id}, SIZE : {size}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfferd Bytes : {numOfBytes}");
        }
    }

}
