using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);

    }

    // 이런식으로 JobQueue를 하면 한 번에 한 쓰레드만 Flush(실행) 할 수 있다
    // -> Action 구현 시 lock걸지 않아도 됨
    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false;

        public void Push(Action job)
        {
            bool flush = false;
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                    flush = _flush = true;
            }

            if (flush)
                Flush();
        }

        void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if (action == null)
                    return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock(_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;             // 볼 일 다 끝났으니까 또 push할거면 관리 하세요~
                    return null;
                }

                return _jobQueue.Dequeue();
            }
        }
    }
}
