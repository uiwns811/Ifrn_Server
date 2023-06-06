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
            while (true)
            {
                //int original = Interlocked.Exchange(ref _locked, 1);
                //if (original == 0)
                //    break;

                // 원래는 0(false)이었는데 내가 1(true)로 바꿨다
                // original은 stack의 지역변수니까 read해도 된다.
                // _lock은 조심해야하고, original은 stack이니까 접근해도 괜찮다.

                // 위 코드는 아래와 같다
                // int original = _locked;
                // _locked = 1;
                // if (original == 0) break;

                // 단, original에 대입하고 lock에 1을 넣는 두 라인 사이가 벌어지면 문제될 수 있으므로
                // Exchange를 이용해준다 (CAS와 유사한 동작 방식)

                int expected = 0;
                int desired = 1;
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;
                // CAS (Compare And Swap) 연산

            }
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

