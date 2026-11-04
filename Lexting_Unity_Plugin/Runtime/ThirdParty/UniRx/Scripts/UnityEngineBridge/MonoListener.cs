using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class MonoListener
{
    public MonoBehaviour behaviour;
    public Subject<Unit> onEnable;
    public Subject<Unit> onDisable;
    public IDisposable enableSubscription;
    public IDisposable disableSubscription;
    public bool isEnableInitialized;

    public MonoListener(MonoBehaviour behaviour)
    {
        this.behaviour = behaviour;
    }

    public void DisposeAll()
    {
        enableSubscription?.Dispose();
        disableSubscription?.Dispose();
    }
}
