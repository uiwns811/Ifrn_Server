using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using Ifrn_ServerCore;

namespace Ifrn_Server
{
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //Packet packet = new Packet() { size = 100, packetId = 10 };
            
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            //byte[] buffer1 = BitConverter.GetBytes(packet.size);       // int형의 값을 byte 배열로 바꿔줌
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);       // int형의 값을 byte 배열로 바꿔줌
            
            //Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer1.Length + buffer2.Length);  

            //Send(sendBuff);

            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"RecvPacket ID : {id}, SIZE : {size}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfferd Bytes : {numOfBytes}");
        }
    }

    internal class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return new GameSession(); });
            // GameSession이 아니라 MMOSession일 수도 있다.
            // - 어떤 Session을 만들지 결정해주면 안에서 만들어준다.

            Console.WriteLine("Listening ... ");

            while (true)
            {

            }
        }
    }
}
