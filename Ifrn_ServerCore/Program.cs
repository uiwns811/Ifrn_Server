using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    class Lock
    {
        // Auto Reset Event
        /*
        // kernel에서는 bool로 다룸
        AutoResetEvent _available = new AutoResetEvent(true);  
        // false = 들어올수없음 true = 들어올수있음

        // Auto = 문을 자동으로 닫아준다.

        public void Acquire()
        {
            _available.WaitOne();       // 입장 시도
            //_available.Reset();         // false로 바꿔줌 but WaitOne()에 포함되어 있다.
        }

        public void Release() 
        {
            _available.Set();           // 이벤트의 상태를 바꾼다 -> _available을 true로 바꿔준다.
        }
        */

        // Manual Reset Event
        ManualResetEvent _available = new ManualResetEvent(false);
      
        // Acquire에서 WaitOne()과 Reset()이 두 개로 나뉘어짐 -> Lock에서는 사용할 수 없는 방법
        // 두 라인을 한 번에 실행해야 함 -> AutoResetEvent로 하자

        // 그럼 언제 하냐?
        // - 한 번에 하나 씩만 입장해야 할 필요가 없는 경우
        // - 오래 걸리는 작업이 끝났을 때 모든 쓰레드가 재가동해주는 경우

        public void Acquire()
        {
            _available.WaitOne();       // 입장 시도
            _available.Reset();         // false로 바꿔줌
        }

        public void Release()
        {
            _available.Set();           // 이벤트의 상태를 바꾼다 -> _available을 true로 바꿔준다.
        }
    }
    internal class Program
    {
        static int _num = 0;
        //static Lock _lock = new Lock();
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                //_lock.Acquire();
                _num++;
                _lock.ReleaseMutex();
                //_lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                //_lock.Acquire();
                _num--;
                _lock.ReleaseMutex();
                //_lock.Release();
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

