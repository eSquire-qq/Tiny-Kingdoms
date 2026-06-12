using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static MainThreadDispatcher _instance;

    public static void EnsureExists()
    {
        if (_instance != null) return;

        var go = new GameObject("MainThreadDispatcher");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<MainThreadDispatcher>();
    }

    public static void Enqueue(Action a)
    {
        lock (_queue) _queue.Enqueue(a);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
            {
                var a = _queue.Dequeue();
                a?.Invoke();
            }
        }
    }
}