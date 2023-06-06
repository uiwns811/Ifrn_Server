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

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; i++)
            {
                Interlocked.Increment(ref number);      
                // 특정 변수를 atomic하게 동작하도록 보장한다
                // cpu 명령어에서 atomic하게 동작해주도록 해준다
                // 단점 : 성능저하
                // -> 이렇게 하면 number++이 atomic하게 동작한다.

                //number++;       // 이 코드가 원자적으로 일어나야 한다.


                //// 아래 코드는 number++; 과 동일하게 동작한다.
                //int temp = number;      // 0
                //temp += 1;              // 1
                //number = temp;          // number = 1
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                int afterValue = Interlocked.Decrement(ref number); 
                // ref number == &number
                // 참조값으로 넣어준다.
                // - number의 주소값을 넣어준다.
                // 이유 : int로 넣어주면 갖고오는 순간에 수정할 수도 있음 
                // - number의 값은 알빠아니고 그 주소의 값을 직접 수정할 것이다.

                // Interlocked.Decrement() 자체가 반환해주는 값만이 그 때의 옳은 값이고
                // 해당 라인이 끝난 뒤 다시 number에 접근하면 원하는 값이 아닐 수 있다.

                //// 아래 코드는 number--;과 같다
                //int temp = number;      // 0
                //temp -= 1;              // -1
                //number = temp;          // number = -1
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

// 원자성 (atomic하다)
// = 더이상 쪼개지면 안되는 작업

// DB에서도 원자성이 나타난다
// ex) 상점에서 검을 구매할 때, 
// 골드 -= 100
// 인벤 += 검
// 위 작업이 원자적으로 나타나야 한다. (골드 -= 100에서 서버 다운되면 검은 안나오게 됨)


// 문제 해결 방법
// - 원자적으로 동작하도록 한다.

// Interlock 계열 함수 사용 시, 내부적으로 Memory Barrier를 간접적으로 사용하고 있다.
// 그래서 volatile하지 않아도 된다. (Interlock하면 volatile 안해도 된다.)

// Interlocked 계열의 함수를 실행했으면 "원자성이 보존된다"
// = All or Nothing (실행이 되거나, 안되거나 둘 중 하나. 중간은 없다)
// 실행하기 시작했으면 끝날 때까지 기다려야 한다.

// - 순서 보장 가능
// 어쨋든 둘 중 하나는 먼저 실행하지만, 실행했으면 그 결과는 무조건 보장된다.

