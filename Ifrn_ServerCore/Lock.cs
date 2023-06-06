using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ifrn_ServerCore
{
    // 재귀적 Lock을 허용할지 (N0)
    // - Writer Lock을 Acquire한 상태에서 재귀적으로 같은 쓰레드에서 Acquire하는 것을 허용할 것인지
    // 만약 허용한다면,
    // 누가 Write를 잡고 있는지 알아야 하므로 WriteThreadId를 기록해준다.

    // 재귀적 Lock 허용
    // - WriteLock -> WriteLock OK / WriteLock -> ReadLock OK
    // - 단, ReadLock -> WriteLock X

    // SpinLock : 5000번 -> Yield

    internal class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // 32bit
        // [Unused(1)] [WriterThreadId(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;            // write는 상호배타적이므로 멀티쓰레드에 안전

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 획득하고 있는지
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하지 않을 때, 경합해서 소유권을 얻는다
            
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            // 현재 쓰레드의 ID를 16비트만큼 밀어서 WRITE_MASK 자리로 옮기고, WRITE_MASK와 AND연산해서 WRITE_MASK를 제외한 위치는 0을 보장한다.

            while(true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0) 
                 Interlocked.Exchange(ref _flag, EMPTY_FLAG);
            // _flag를 깨끗한 값으로 밀어준다.
        }

        public void ReadLock() 
        {
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면 ReadCount를 1 늘린다
            while (true)
            {
                for (int i =0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);
                    // 내가 예상하는 값 == WRITE_MASK가 0인 값. 그래서 READ_MASK만 추출된 _flag & READ_MASK가 expected
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;



                    // 쓰레도 A와 B가 동시에 접근하더라도, 결국 먼저 실행되는 애가 있다.
                    // A가 성공했다 치면, B는 실패할 수 밖에 없다 (두 라인 사이에서 _flag와 expected가 달라짐)
                    // 그러면 B는 한 spin 더 돌아서 성공하게 될 것이다.
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
            // _flag를 1 줄여줌
        }
    }
}
