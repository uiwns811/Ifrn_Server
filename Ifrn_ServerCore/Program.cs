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
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0; 

        static void Thread_1()
        {
            y = 1;           // Store y
            Thread.MemoryBarrier();                 // 위 아래 라인의 순서를 뒤바꿀 수 없게 해준다.
            r1 = x;          // Load X
        }

        static void Thread_2() 
        {
            x = 1;          // Store X
            Thread.MemoryBarrier();
            r2 = y;         // Load y
        }
        static void Main(string[] args)
        {
            int count = 0;
            while(true)
            {
                count++;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);  
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                if (r1 == 0 && r2 == 0)
                    break;
            }
            Console.WriteLine($"{count}번만에 빠져나옴!");
        }
    }
}

// 강의노트

// Thread_1, Thread_2를 보면 내부에서 y = 1; r1 = x; 이 두 명령어가 HW적으로 아무 연관이 없으므로
// HW에서 이를 실행할 때 더 효율적으로 실행하기 위해서 순서를 바꾸기도 한다.
// HW가 지멋대로 최적화해버린다..

// 메모리 베리어의 기능
// 1) 코드 재배치 억제
// -  
// 
// 2) 가시성