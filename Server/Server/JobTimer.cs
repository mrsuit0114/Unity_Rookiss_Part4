using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행 시간 언제 실행되는지..
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            return other.execTick - execTick;  //execTick이 작은것이 우선순위가 높다
            // 실수했다더라 이렇게하면 오름차순이라는데 나중에 고친다니까 그때가서함
        }
    }

    //최적화 더 빡세게 하려면 시간으로 나눌수있는데 나중에나 생각할 일인듯
    internal class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job;

                lock(_lock )
                {
                    if (_pq.Count == 0)
                        break;
                    job = _pq.Peek();  // 실행해야하는지 확인
                    if (job.execTick > now)
                        break;  // 없으면 나가기

                    _pq.Pop();  // 여기까지 온 것 자체가 있다는 것
                }
                job.action.Invoke();  // 락 나와서 job 실행하기
            }  // 루프
        }

    }
}
