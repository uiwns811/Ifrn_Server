using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    class SpinLock
    {
        volatile int _locked = 0;

        public void Acquire()
        {
            int expected = 0;
            int desired = 1;
            while (Interlocked.CompareExchange(ref _locked, desired, expected) == desired);

            // 쉬다 올게 ~
            // Thread.Sleep(1);        // 무조건 휴식 : 1ms만큼 쉴게요
            // Thread.Sleep(0);        // 조건부 휴식 : 우선순위가 나보다 같거나 높은 쓰레드가 없으면 다시 본인에게
            Thread.Yield();         // 관대한 양보 : 지금 실행 가능한 쓰레드가 있으면 실행하세요 -> 실행 가능한 애 없으면 다시 본인에게
        }

        public void Release() 
        {
            _locked = 0;
        }
    }
    internal class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }

        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}

