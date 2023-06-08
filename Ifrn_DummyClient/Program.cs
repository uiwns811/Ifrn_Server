using Ifrn_ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_DummyClient
{   
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            Packet packet = new Packet() { size = 4, packetId = 7 };

            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

                byte[] buffer1 = BitConverter.GetBytes(packet.size);       // int형의 값을 byte 배열로 바꿔줌
                byte[] buffer2 = BitConverter.GetBytes(packet.packetId);       // int형의 값을 byte 배열로 바꿔줌

                Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
                Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size    );

                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfferd Bytes : {numOfBytes}");
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}
