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
                Session session = new Session();
                session.Start(clientSocket);
                
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome To MMORPG Server !");
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.Disconnect();
                session.Disconnect();
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
            // endPoint = 목적지 설정

            _listener.Init(endPoint, OnAcceptHandler);
            // endPoint는 얘고, 누군가 들어오면 OnAcceptHandler라는 애로 나한테 알려줘
            
            Console.WriteLine("Listening ... ");

            while (true)
            {

            }
        }
    }
}