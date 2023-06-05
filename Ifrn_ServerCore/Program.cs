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
        int _answer;
        bool _complete;

        void A()
        {
            _answer = 123;
            Thread.MemoryBarrier();         // Barrier 1
            _complete = true;
            Thread.MemoryBarrier();         // Barrier 2
        }

        // store을 연속 두 번 했으니까 각 Memory Barrier가 두 store의 가시성을 보여준다.


        void B()
        {
            Thread.MemoryBarrier();         // Barrier 3
            if (_complete)
            {
                Thread.MemoryBarrier();     // Barrier 4
                Console.WriteLine(_answer);
            }
        }

        // Read하기 전에 Barrier 3을 해서 가시성을 높여준다.

        static void Main(string[] args)
        {

        }
    }
}

// 강의노트

// Thread_1, Thread_2를 보면 내부에서 y = 1; r1 = x; 이 두 명령어가 HW적으로 아무 연관이 없으므로
// HW에서 이를 실행할 때 더 효율적으로 실행하기 위해서 순서를 바꾸기도 한다.
// HW가 지멋대로 최적화해버린다..

// 메모리 베리어의 기능
// - 1) 코드 재배치 억제
// - 2) 가시성

// 메모리 베리어의 종류
// - 1) Full Memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘 다 막는다
// - 2) Store Memory Barrier (ASM SFENCE) : Store만 막는다
// - 3) Load Memory Barrier (ASM LFENCE) : Load만 막는다

