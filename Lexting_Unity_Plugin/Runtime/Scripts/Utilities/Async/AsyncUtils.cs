using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;
using System;

public static class AsyncUtils
{
    public static IEnumerator WaitUntil(Func<bool> condition)
    {
        yield return new WaitUntil(condition);
    }
    
    /// <summary>
    /// WaitUntil shortcut
    /// </summary>
    public static void Wait(this MonoBehaviour behaviour, Func<bool> condition, UnityAction callback)
    {
        WaitUntil(condition).ToObservable().Subscribe(_=> callback.I());
    }

    public static IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public static IEnumerator WaitS(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// WaitForSeconds shortcut
    /// </summary>
    public static void WaitS(this MonoBehaviour behaviour, float seconds, UnityAction callback)
    {
        //Observable.Timer(TimeSpan.FromSeconds(seconds)).Subscribe(_ => callback.I());
        WaitForSeconds(seconds).ToObservable().Subscribe(_ => callback.I());
    }

    /// <summary>
    /// WaitForSeconds shortcut
    /// </summary>
    public static IDisposable CancellableWaitS(this MonoBehaviour behaviour, float seconds, UnityAction callback)
    {
        //Observable.Timer(TimeSpan.FromSeconds(seconds)).Subscribe(_ => callback.I());

        return WaitForSeconds(seconds).ToObservable().Subscribe(_ => callback.I());
    }

    /// <summary>
    /// WaitForEndOfFrame shortcut
    /// </summary>
    public static void WaitF(this MonoBehaviour behaviour, int numFrames, UnityAction callback)
    {
        Observable.EveryUpdate().SampleFrame(numFrames).Take(1).Subscribe(_ => callback.I());
    }

    /// <summary>
    /// Returns false until a time interval has elapsed
    /// </summary>
    /// <param name="start">the time at which the lock began</param>
    /// <param name="interval">amount of time before the lock returns true thereafter</param>
    /// <returns></returns>
    public static bool TLock(this MonoBehaviour behaviour, float start, float interval)
    {
        return Time.time - start > interval;
    }

    /// <summary>
    /// A continuous TLock. Executes a desire action only if the TLock has elapsed, and
    /// then the TLock automatically restarts.
    /// </summary>
    /// <param name="start">the start time of the TLock that activated the last time the action executed</param>
    /// <param name="interval">the amount of time that must elapse before each TLock will return true once it has started</param>
    /// <param name="callback">the action to execute if the current TLock returns true</param>
    public static void TLock(this MonoBehaviour behaviour, ref float start, float interval, UnityAction callback)
    {
        if (behaviour.TLock(start, interval))
        {
            callback?.I();
            start = Time.time;
        }
    }

    /// <summary>
    /// StartCoroutine shortcut
    /// </summary>
    public static void SC(this MonoBehaviour behaviour, IEnumerator coroutine)
    {
        behaviour.StartCoroutine(coroutine);
    }
    
    /// <summary>
    /// StopCoroutine shortcut with built-in null-check
    /// </summary>
    public static void XC(this MonoBehaviour behaviour, IEnumerator coroutine)
    {
        if (coroutine != null)
            behaviour.StopCoroutine(coroutine);
    }

}
