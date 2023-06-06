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
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My Name Is {Thread.CurrentThread.ManagedThreadId}"; });  
        // 전역 : 모든 쓰레드가 공유
        // ThreadLocal<> : 쓰레드마다 접근하면 자신만의 공간에 저장됨
        
        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if (repeat)
                Console.WriteLine(ThreadName.Value + "  (repeat)");
            else
                Console.WriteLine(ThreadName.Value);

            // 이러면 ThreadName.Value == null일때 (repeat == false일 때)
            // 생성자에 넣은 람다함수가 호출됨

            // ThreadLocal은 대부분 static에 넣는다.

        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadName.Dispose();
            // 필요없을 때 날려준다.
        }
    }
}

// 응용 방법
// - Job Queue에서 여러 개를 가져와, 자기만의 공간(TLS)에 넣어둔 뒤
//   Lock을 걸지 않고 하나씩 꺼내서 사용한다.

// 공용 공간 접근 횟수를 줄일 수 있다.
 