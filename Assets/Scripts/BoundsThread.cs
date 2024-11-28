using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

public class BoundsThread : MonoBehaviour
{
    static BoundsThread instance;
    Queue<ThreadInfo> DataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
        instance = this;
    }

    public static void RequestValidTargetPosition(Func<Bounds> validPositionMethod, Action<Bounds> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.RequestThread(validPositionMethod, callback);
        };

        new Thread(threadStart).Start();
    }

    void RequestThread(Func<Bounds> validPositionMethod, Action<Bounds> callback)
    {
        Bounds currentRoomBounds = validPositionMethod();
        lock (DataQueue)
        {
            DataQueue.Enqueue(new ThreadInfo(currentRoomBounds, callback));
        }
    }

    void Update()
    {
        if (DataQueue.Count > 0)
        {
            for (int i = 0; i < DataQueue.Count; i++)
            {
                ThreadInfo threadInfo = DataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }
}

struct ThreadInfo
{
    public readonly Bounds parameter;
    public readonly Action<Bounds> callback;

    public ThreadInfo(Bounds bounds, Action<Bounds> callback)
    {
        this.parameter = bounds;
        this.callback = callback;
    }
}