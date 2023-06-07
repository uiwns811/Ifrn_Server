using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    internal class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                // 받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From Client] {recvData}");

                // 보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server !");
                clientSocket.Send(sendBuff);

                // 쫓아낸다
                clientSocket.Shutdown(SocketShutdown.Both);     // 더이상 듣기도 싫고 알기도 싫다
                clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Main(string[] args)
        {
            // DNS : Domain Name System
            // www.sumin.com -> 123.123.123.12 이렇게 해보자
           
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, OnAcceptHandler);
            // endPoint는 얘고, 누군가 들어오면 OnAcceptHandler라는 애로 나한테 알려줘
            
            Console.WriteLine("Listening ... ");

            while (true)
            {

            }
        }
    }
}