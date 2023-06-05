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
        static void MainThread(object state)
        {
            for(int i = 0; i < 5; i++)
                Console.WriteLine("Hello Thread!");
        }
        static void Main(string[] args)
        {
            // new로 스레드를 새로 할당하면 부담이 클 때, 단기적으로 일을 시키자. 
            // C#에는 쓰레드 풀이 있다.

            // 쓰레드 풀의 원리
            // - new Thread : 모든 책임을 져야 한다. (정직원)
            // - 쓰레드 풀 : 유동적으로 기다리고 있는 직원들에게 일을 시킬 수 있다.
            // 풀링 : 창고에 넣어놨다가 필요할 때 꺼내서 쓴다.

            // 쓰레드 풀의 장점
            // 쓰레드 : 개수 제한이 없다.
            // - new Thread를 1000번 해도 문제가 없다.
            // 쓰레드 풀 : 최대 개수 제한이 있음.
            // - 너무 많은 개수의 쓰레드 실행을 요구하면 기존 작업을 하던 쓰레드가 끝나야 실행한다.
            // - 가급적 짧은 일을 시키는게 좋다.
            // - 과도하게 긴 일을 던지면 통째로 먹통이 될 수 있다.


            ThreadPool.SetMinThreads(1, 1);                 // 인자 1 : 일을 할 아이
            ThreadPool.SetMaxThreads(5, 5);                 // 최대 쓰레드 = 5개. 그 이상은 실행안됨

            for (int i  = 0; i < 5; i++)
            {
                // ThreadPool.QueueUserWorkItem((obj) => { while (true) { } });   

                // Task : 직원이 할 일감을 정의한다.
                // - 쓰레드 풀에 넣어서 사용한다.
                // - 오래 걸리는 일이라면 LongRunning 옵션을 사용해도 좋다.
                // - 인자로 옵션을 넣어줄 수 있다. 

                // -- TaskCreationOptions.LongRunning : 별도의 쓰레드를 만들어서 쓰레드 풀과 무관하게 처리한다.

                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);         // - 쓰레드 풀에 들어가지만 오래 걸리는 작업이라는 것을 알려줌 -> 별도 처리
                t.Start();
            }

            ThreadPool.QueueUserWorkItem(MainThread);

            while(true) { }

            // 위 코드에 대한 설명
            // - 쓰레드 풀의 최대 쓰레드 개수 만큼 쓰레드를 풀로 돌리고 있음
            // - 그래서 MainThread를 실행할 수 없음. 먹통이 되어 버림
            // - 최대 쓰레드 개수 이하로 하면 잘 돌아감. 단) 위 상황 주의할 것


            //Thread t = new Thread(MainThread);
            //t.Name = "Test Thread";                         // 스레드 이름 지정
            //t.IsBackground = true;                          // 백그라운드에서 실행되므로 메인이 끝나면 종료된다. (디폴트 : false)
            //t.Start();

            //Console.WriteLine("Waiting for Thread!");

            //t.Join();                                       // 스레드 t의 종료를 기다린다. 끝나면 다음 실행

            //Console.WriteLine("Hello World!");
        }
    }
}
