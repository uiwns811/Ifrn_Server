using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    internal class Program
    {
        static bool _stop = false;           // 전역 : 모든 쓰레드가 접근 가능

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작!");

            while(_stop == false) { 
                // 누군가가 stop 신호를 해주기를 기다린다.
            }

            Console.WriteLine("쓰레드 종료!");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();  

            Thread.Sleep(1000);                // 1초간 쉬겠다.

            _stop = true;

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");

            t.Wait();                           // join의 Task 버전

            Console.WriteLine("종료 성공");
        }
    }
}
