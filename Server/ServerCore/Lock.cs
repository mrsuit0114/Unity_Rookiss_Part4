using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 재귀적 락을 허용할지 (No) -> WriteThreadId가 1비트여도 괜찮다
    // 재귀적 락을 허용할지 (Yes) -> WL -> WL (ok) WL -> RL (ok) RL -> WL(no)
    // 스핀락 정책 (5000번 -> Yield)
    internal class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)] -> 32bit
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WL을 이미 획득한 상태인지
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if(Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                _writeCount++;
                return;
            }

            // 아무도 WL or RL을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
            // 1부터 늘어나는 숫자 -> Unique하므로 Id로 사용
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for(int i = 0;i<MAX_SPIN_COUNT;i++)
                {
                    // 비교와 값을 넣는 과정의 원자성 보장
                    if(Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }
                Thread.Yield();  // 관대한 양보 우선권 낮은 쓰레드에도 양보가능
            }
        }
        public void WriteUnLock()
        {
            int lockCount = --_writeCount;
            // 플래그 확인을 해서 락을 소유한 쓰레드만 호출할수있도록 해야할텐데 밖에서 해주려는건가?
            if(lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            // 동일 쓰레드가 WL을 이미 획득한 상태인지
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WL을 획득하고 있지 않으면 ReadCOunt 1늘린다.
            while (true)
            {
                int expected = (_flag & READ_MASK);
                for(int i=0;i<MAX_SPIN_COUNT; i++)
                {
                    // 이미 expected에서 WR == 0을 확인하기때문에 따로 더 검사하지 않아도된다.
                    // 반환값이 bool이 아니라 이렇게 비교해서 성공여부를 판단하는군
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }
                Thread.Yield ();
            }

        }

        public void ReadUnLock()
        {
            Interlocked.Decrement(ref _flag);
        }

    }
}
