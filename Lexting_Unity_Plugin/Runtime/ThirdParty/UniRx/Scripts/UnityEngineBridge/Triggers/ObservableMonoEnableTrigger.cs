using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniRx.Triggers
{
    /// <summary>
    /// Alt version of ObservableEnableTrigger which also listens 
    /// to the enabled property of any number of target monobehvaiours on the same game object
    /// </summary>
    [DisallowMultipleComponent]
    public class ObservableMonoEnableTrigger : ObservableTriggerBase
    {
        public class MonoListener
        {
            public MonoBehaviour behaviour;
            public Subject<Unit> onEnable;
            public Subject<Unit> onDisable;
            
            public MonoListener(MonoBehaviour behaviour)
            {
                this.behaviour = behaviour;
            }
        }



        /// <summary>
        /// A dictionary of monobehaviours on the same game object who each share this trigger.
        /// Each has an onEnable and onDisable subject so their enabled properties can be individually tracked.
        /// </summary>
        private readonly Dictionary<int,MonoListener> listeners = new Dictionary<int,MonoListener>();
        
        /// <summary>
        /// Just like ObservableEnableTrigger, this OnEnable method covers the following use cases:
        /// 1) This trigger component just toggled on...
        /// 2) This trigger's game object just toggled on...
        /// Listeners for the target behaviour(s') enabled property are not found here.
        /// See InitializeOnEnable and InitializeOnDisable methods for those.
        /// </summary>
        void OnEnable()
        {
            foreach (KeyValuePair<int, MonoListener> listenerPair in listeners)
                if (listenerPair.Value.onEnable != null)
                    listenerPair.Value.onEnable.OnNext(Unit.Default);
        }
        
        void Start()
        {
            foreach (KeyValuePair<int, MonoListener> listenerPair in listeners)
                if (listenerPair.Value.onEnable != null && listenerPair.Value.behaviour.enabled)
                    listenerPair.Value.onEnable.OnNext(Unit.Default);
        }

        void OnDisable()
        {
            foreach (KeyValuePair<int, MonoListener> listenerPair in listeners)
                if (listenerPair.Value.onDisable != null)
                    listenerPair.Value.onDisable.OnNext(Unit.Default);
        }

        public IObservable<Unit> OnMonoEnableAsObservable(MonoBehaviour behaviour)
        {
            if (!listeners.ContainsKey(behaviour.GetInstanceID()))
                listeners.Add(behaviour.GetInstanceID(), new MonoListener(behaviour));
                
            if (listeners[behaviour.GetInstanceID()].onEnable == null)
                InitializeOnEnable(behaviour.GetInstanceID());
            
            return listeners[behaviour.GetInstanceID()].onEnable;
        }

        private void InitializeOnEnable(int instanceID)
        {
            listeners[instanceID].onEnable = new Subject<Unit>();
            listeners[instanceID].behaviour.ObserveEveryValueChanged(x => x.enabled)
                .Subscribe(x => {
                    if (listeners[instanceID].behaviour.enabled)
                        listeners[instanceID].onEnable.OnNext(Unit.Default);
                });
        }
        
        /// <summary>This function is called when the behaviour becomes disabled () or inactive.</summary>
        public IObservable<Unit> OnMonoDisableAsObservable(MonoBehaviour behaviour)
        {
            if (!listeners.ContainsKey(behaviour.GetInstanceID()))
                listeners.Add(behaviour.GetInstanceID(), new MonoListener(behaviour));

            if (listeners[behaviour.GetInstanceID()].onDisable == null)
                InitializeOnDisable(behaviour.GetInstanceID());

            return listeners[behaviour.GetInstanceID()].onDisable;
        }

        private void InitializeOnDisable(int instanceID)
        {
            listeners[instanceID].onDisable = new Subject<Unit>();
            listeners[instanceID].behaviour.ObserveEveryValueChanged(x => x.enabled)
                .Subscribe(x => {
                    if (!listeners[instanceID].behaviour.enabled)
                        listeners[instanceID].onDisable.OnNext(Unit.Default);
                });
        }

        protected override void RaiseOnCompletedOnDestroy()
        {
            foreach (KeyValuePair<int,MonoListener> listenerPair in listeners)
            {
                if (listenerPair.Value.onEnable != null)
                    listenerPair.Value.onEnable.OnCompleted();
                
                if (listenerPair.Value.onDisable != null)
                    listenerPair.Value.onDisable.OnCompleted();
            }
        }
        public static IEnumerator FrameHop(int numberOfFramesToHop)
        {
            for (int i = 0; i < numberOfFramesToHop; i++)
                yield return new WaitForEndOfFrame();
        }
    }
}