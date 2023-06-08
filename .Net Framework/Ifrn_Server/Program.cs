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
   
    internal class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return new ClientSession(); });
            // GameSession이 아니라 MMOSession일 수도 있다.
            // - 어떤 Session을 만들지 결정해주면 안에서 만들어준다.

            Console.WriteLine("Listening ... ");

            while (true)
            {

            }
        }
    }
}
