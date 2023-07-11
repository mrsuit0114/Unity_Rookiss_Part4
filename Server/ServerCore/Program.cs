// See https://aka.ms/new-console-template for more information

using ServerCore;

int count = 0;
Lock _lock = new Lock();

Task t1 = new Task(delegate ()
{
    for (int i = 0; i < 100000; i++)
    {
        _lock.WriteLock();
        _lock.WriteLock();
        count++;
        _lock.WriteUnLock();
        _lock.WriteUnLock();
    }
});

Task t2 = new Task(delegate ()
{
    for (int i = 0; i < 100000; i++)
    {
        _lock.WriteLock();
        count--;
        _lock.WriteUnLock();
    }
});

t1.Start();
t2.Start();

// 기다리지 않으면 메인쓰레드 종료시 다 종료되는듯 -> 0
Task.WaitAll(t1, t2);
Console.WriteLine(count);





