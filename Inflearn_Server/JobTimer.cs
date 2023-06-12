using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick;            // 실행 시간
        public Action action;
        public int CompareTo(JobTimerElem other)
        {
            return execTick - other.execTick;           // 작은 애가 먼저 !
            // 음수 : other보다 순서 빠름
            // 양수 : other보다 순서 느림
            // 0 : 순서가 같음
        }
    }

    //
    // 오래 걸리는 것들은 우선순위큐로 관리
    // 시간이 임박한 애들은 List로 관리
    // - 하나의 bucket에 같은 시간에 실행돼야 하는 job들이 List로 관리됨 -> 하나씩 다 실행
    // - 그리고 또 시간됐으면 다음 bucket으로 이동
    internal class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();    

        public static JobTimer Instance { get; } = new JobTimer();

        // tickAfter : 몇 초 후에 실행해야 하는지
        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter;        // 현재시간 + 몇초후
            job.action = action;

            lock(_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while(true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job;   

                lock(_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();       // 엿본다
                    if (job.execTick > now)
                        break;

                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
