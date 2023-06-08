using Inflearn_ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inflearn_Server
{
    abstract class Packet
    {
        public ushort size;
        public ushort packetId;

        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;

        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);            // size
            count += sizeof(ushort);            // packetid
            this.playerId = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);

            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));        // GetString : Byte -> string
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // s.Slice : count부터 s.Length - count만큼 잘라줌. but s가 변하는 것은 아님. 결과를 새로 return해줌

            // string len[2]
            // byte []
            // string의 길이를 보내면 모르니까 2byte짜리 string.length를 보내고, 그 다음 그 길이의 string을 보내자
            // (고정길이 가변길이)

            // name의 길이 보내주기
            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);       // Byte배열로 변환됐을 때의 길이
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);

            // 실제 name 데이터 보내주기
            Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);       // string을 Byte 배열로 변환
            // GetBytes한 것을, segment의 count부터 nameLen만큼에다 복사하자
            count += nameLen;

            // 최종 count
            success &= BitConverter.TryWriteBytes(s, count);

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
                        Console.WriteLine($"PlayerInfoReq ID : {p.playerId}, playerName : {p.name}");
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
