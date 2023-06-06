using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    internal class Program
    {
        static volatile int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                // 상호 배제 (Mutual Exclusive)
                // : 나만 사용할거다. 얼씬도 하지 마라.

                lock(_obj)
                {
                    number++;
                }

                // Enter, Exit 짝을 잘 맞춰줘야 한다.

                //try
                //{
                //    Monitor.Enter(_obj);
                //    number++;

                //    return;
                //}
                //finally
                //{
                //    Monitor.Exit(_obj); 
                //}
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++) 
            { 
                Monitor.Enter(_obj);
                number--;
                Monitor.Exit(_obj);
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