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
        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000];
            // 5 * 5 배열
            // [][][][][] [][][][][] [][][][][] [][][][][] [][][][][]

            // 캐시의 공간지역성
            // - 접근한 변수와 인접한 주소가 접근될 확률이 높다.
            // (x, y)는 캐시미스가 너무 많이 난다 -> 공간지역성이 떨어짐

            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[y, x] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 순서 걸린 시간 : {end - now}");
            }

            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[x, y] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(x, y) 순서 걸린 시간 : {end - now}");
            }
        }
    }
}
