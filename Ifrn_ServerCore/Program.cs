using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    // 서로 lock이 뒤엉켜 deadlock이 발생하는 상황
    class SessionManager
    {
        static object _lock = new object();
        
        public static void TestSession()
        {
            lock(_lock)
            {

            }
        }

        public static void Test()
        {
            lock(_lock)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object _lock = new object();

        public static void Test()
        {
            lock(_lock)
            {
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock(_lock)
            {

            }
        }
    }
  
    internal class Program
    {
        static volatile int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++) 
            {
                UserManager.Test();
            }
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}

// 멀티쓰레드 환경에서 Read 연산은 크게 중요하지 않다.
// 진짜 문제가 되는 부분 : Write 연산
// 다른 곳에서 Write하면 그 때부터 Read도 문제가 된다.

// 임계영역 : 쓰레드가 동시에 접근하면 문제되는 코드들.